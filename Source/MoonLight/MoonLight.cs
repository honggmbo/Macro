using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.IO;
using System.Net;
using System.Timers;
using System.Runtime.InteropServices;
using Tesseract;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Input;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;

namespace MoonLight
{
    public partial class MoonLight : Form
    {
        [DllImport("user32")]
        public static extern Int32 GetCursorPos(out POINT pt);
        [DllImport("user32")]
        public static extern Int32 SetCursorPos(Int32 x, Int32 y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

 
        public int delayMinute = 5;
        public Dictionary<string, ActionData> dActionData = new Dictionary<string, ActionData>();
        public Dictionary<MacroType, List<PointData>> macro = new Dictionary<MacroType, List<PointData>>();
        string[] replaceWord = { ".", ",", "/", "|", "]", "'", "`", "-" };

        System.Windows.Forms.Timer timerCapture = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timerAll = new System.Windows.Forms.Timer();

        // telegram
        static string botKey = "295108393:AAHC7BzhuM7jKSMpbkJQp6yaAzEHH2KaK7o";
        static public int botID = 109855719;
        static public Telegram.Bot.TelegramBotClient bot = new Telegram.Bot.TelegramBotClient(botKey);

        // debug
        public int saveCount = 0;
        public POINT prevPoint = new POINT();
        public int defaultResolutionX = 1600;
        public int defaultResolutionY = 900;
        public int resolutionX = 1600;
        public int resolutionY = 900;
        public bool questStart = false;
        public bool[] iteamBreate = new bool[4];
        public int currentQuestCount = 0;
        public Stopwatch stopwatch = new Stopwatch();
        public Stopwatch dieCheck = new Stopwatch();
        public DateTime bossTime = DateTime.Now;
        public int bossTimeMinute = 0;
        public int maxHGCnt = 0;
        public IntPtr hwnd;

        // possition info
        public Rectangle programInfo = new Rectangle(0, 0, 0, 0);
		public Rectangle ssQuest = new Rectangle(635, 275, 426, 300);
        public Rectangle ssQuest2 = new Rectangle(1316, 365, 278, 65);
        public Rectangle ssHP = new Rectangle(132, 10, 100, 30);
        public Rectangle ssHunting = new Rectangle(402, 52, 48, 16);
        public Rectangle ssSQuest = new Rectangle(560, 476, 480, 71);
        public Rectangle ssQuestCount = new Rectangle(1238, 418, 82, 22);
        public Rectangle ssBoss = new Rectangle(568, 258, 477, 39);
        public Rectangle ssHuntingReward = new Rectangle(586, 366, 434, 140);
        public Rectangle ssRepeatedQuest = new Rectangle(517, 377, 576, 46);
        public Rectangle ssGohome = new Rectangle(641, 377, 328, 56);
		public Rectangle ssSafeMode = new Rectangle(628, 736, 359, 118);
		public Rectangle ssInventory = new Rectangle(1045, 104, 544, 44);

		// Lock
		static readonly object _locker = new object();
        static bool isMacroSwitch = false;
        static bool isRunning = false;
        static bool isAutoClick = false;

        // Key Thread
        //Thread KeyThread;
        Thread ScreenShopThread;
        Thread RunMacroThread;

        static public Bitmap gameScreenShot;
        static public List<MacroType> runMacroLis = new List<MacroType>();

        ImageMacro imgMacro = new ImageMacro();

        public MoonLight()
        {
            InitializeComponent();

            imgMacro.Init();

            LoadProgramInfo();
            LoadXMl();

            setTelegramEvent();

            ScreenShopThread = new Thread(ScreenShop);
            ScreenShopThread.Start();

            RunMacroThread = new Thread(RunMacro);
            RunMacroThread.Start();

            chkBoss.Checked = true;
            stopwatch.Start();

            for (int i = 1; i <= maxHGCnt; i++)
            {
                cbHG.Items.Add("사냥터" + i.ToString());
            }

            SetItemBreak(false);

            cbHG.SelectedIndex = GetXmlHuntingGround() - 1;

            this.Location = new System.Drawing.Point(programInfo.X, programInfo.Y + programInfo.Height);
        }

        public void SetItemBreak(bool on)
        {
            for (int i = 0; i < 4; i++)
            {
                iteamBreate[i] = on;
            }
        }

        public void RunMacro()
        {
            while(true)
            {
                int error = 0;
                if (IsMacroSwitch() == true && runMacroLis.Count > 0)
                {
                    try
                    {
                        MacroType type = runMacroLis[0];
                        runMacroLis.RemoveAt(0);
                        SetRunning(true);
                        error = OneClickMacro(type);

                        if (error != 0)
                            BotSendMessage(string.Format("MsgType = {0}\nError = {1}", type, error));

                        // 예외처리
                        if (error == 1)
                        {
                            RemoveMacro();
                            AddMacro(MacroType.eConfirm);
                            AddMacro(MacroType.eShop);
                            AddMacro(MacroType.eReturn);
                        }
                        else if (error == 2)
                        {
                            RemoveMacro();
                            AddMacro(MacroType.eReQuest);
                            AddMacro(MacroType.eShop);
                            AddMacro(MacroType.eReturn);
                        }

                        if (runMacroLis.Count == 0)
                            SetRunning(false);
                    }
                    catch
                    {
                        BotSendMessage("RunMacro Error");
                    }
                }
                               
                Thread.Sleep(1000);
            }
        }


        public void ScreenShop()
        {
            while(true)
            {
                try
                {
                    imgMacro.UpdateFullScreen();

                    if (IsMacroSwitch() == true && IsRunning() == false)
                    {
                        lock (_locker)
                        {
                            CheckUserInfo();
                        }
                    }

                    Thread.Sleep(1000);
                }
                catch
                {
                    BotSendMessage("ScreenShop() 에러");
                }
            }
        }

        public void LoadProgramInfo()
        {
            imgMacro.GetProgramRect(ref programInfo);

            UpdatePossition(ref ssHP);
            UpdatePossition(ref ssQuest);
            UpdatePossition(ref ssQuest2);
            UpdatePossition(ref ssHunting);
            UpdatePossition(ref ssSQuest);
            UpdatePossition(ref ssQuestCount);
            UpdatePossition(ref ssBoss);
            UpdatePossition(ref ssHuntingReward);
            UpdatePossition(ref ssRepeatedQuest);
            UpdatePossition(ref ssGohome);
			UpdatePossition(ref ssSafeMode);
		}

        void UpdatePossition(ref Rectangle ssp)
        {
            ssp.Y += programInfo.Height - resolutionY;
        }

        public Point GetCurrentPoint(ref Point pt)
        {
            float rateX = resolutionX / defaultResolutionX;
            float rateY = resolutionY / defaultResolutionY;

            pt.X = (int)(pt.X * rateX);
            pt.Y = (int)(pt.Y * rateY);

            return pt;
        }

        public async void BotSendMessage(string msg)
        {
			string path = @"img\fullscreen.bmp";
            try
            {
                imgMacro.GetFullScreen().Save(path, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch
            {

            }

			if (bot != null)
            {
				FileStream stream = System.IO.File.OpenRead(path);
                await bot.SendPhotoAsync(botID, stream, msg);
			}
        }

		private void LoadXMl()
        {
            macro.Clear();

            XmlTextReader reader = new XmlTextReader("Setting.xml");
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(reader);

            // default
            XmlNode defaultResulution = xdoc.DocumentElement.SelectSingleNode("Config/DefaultResolution");
            if (defaultResulution != null)
            {
                defaultResolutionX = int.Parse(defaultResulution.Attributes["x"].Value);
                defaultResolutionY = int.Parse(defaultResulution.Attributes["y"].Value);
            }

            // current
            XmlNode resolution = xdoc.DocumentElement.SelectSingleNode("Config/Resolution");
            if (resolution != null)
            {
                resolutionX = int.Parse(resolution.Attributes["x"].Value);
                resolutionY = int.Parse(resolution.Attributes["y"].Value);
            }

            // Action
            XmlNodeList menu = xdoc.DocumentElement.SelectNodes("Config/Menu");
            if (menu != null)
            {
                XmlNodeList list = menu[0].ChildNodes;
                maxHGCnt = 0;
                foreach (XmlNode action in list)
                {
                    ActionData ad = new ActionData();
                    ad.name = action.Attributes["name"].Value;
                    ad.pt = new Point();
                    ad.pt.X = int.Parse(action.Attributes["x"].Value);
                    ad.pt.Y = int.Parse(action.Attributes["y"].Value);
                    GetCurrentPoint(ref ad.pt);
                    dActionData.Add(ad.name, ad);

                    if (ad.name.IndexOf("사냥터") != -1)
                    {
                        int cnt = int.Parse(ad.name.Substring(3));
                        if (maxHGCnt < cnt)
                            maxHGCnt = cnt;
                    }
                }
            }

            // itemBreak
            AddMacro(xdoc, "IteamBreak", MacroType.eItemBreak);

            // Quest
            AddMacro(xdoc, "Quest", MacroType.eQuest);

            // Return
            AddMacro(xdoc, "Return", MacroType.eReturn);

            // Hunting
            AddMacro(xdoc, "Hunting", MacroType.eHunting);
           
            // Scroll Quest
            AddMacro(xdoc, "ScrollQuest", MacroType.eSQuest);

            // Click
            AddMacro(xdoc, "Click", MacroType.eClick);

            // Cancle
            AddMacro(xdoc, "Cancle", MacroType.eCancle);

            // ReQuest
            AddMacro(xdoc, "ReQuest", MacroType.eReQuest);

            // Confirm
            AddMacro(xdoc, "Confirm", MacroType.eConfirm);

            // Shop
            AddMacro(xdoc, "Shop", MacroType.eShop);

            // Safe Mode
            AddMacro(xdoc, "SafeMode", MacroType.eSafeMode);

			reader.Close();

            AddLog("데이터 로드 완료");
        }

        private void AddMacro(XmlDocument xdoc, string sNode, MacroType eType)
        {
            XmlNodeList node = xdoc.DocumentElement.SelectNodes(sNode);
            if (node != null)
            {
                List<PointData> listPD;
                XmlNodeList list = node[0].ChildNodes;
                GetNodeList(list, out listPD);
                macro.Add(eType, listPD);
            }
        }

        private void GetNodeList(XmlNodeList list, out List<PointData> listPD)
        {
            listPD = new List<PointData>();

            foreach (XmlNode point in list)
            {
                PointData pd = new PointData();
                pd.delay = int.Parse(point.Attributes["delay"].Value);
                pd.actionName = point.Attributes["actionname"].Value;

                ActionData ad = dActionData[pd.actionName];
                pd.pt = new Point();
                pd.pt = ad.pt;

                listPD.Add(pd);
            }
        }

        private void btReturn_Click(object sender, EventArgs e)
        {
            OneClickMacro(MacroType.eShop);
            OneClickMacro(MacroType.eReturn);
        }

        private void btItemBreak_Click(object sender, EventArgs e)
        {
            OneClickMacro(MacroType.eItemBreak);
        }

        private void btQuest_Click(object sender, EventArgs e)
        {
            OneClickMacro(MacroType.eQuest);
        }

        public void SetMacroSwitch(bool on)
        {
            lock (_locker)
            {
                isMacroSwitch = on;
                isAutoClick = on;

                if (on == false)
                {
                    RemoveMacro();
                }
            }
        }

        public bool IsMacroSwitch()
        {
            lock (_locker)
            {
                return isMacroSwitch;
            }
        }

        public void SetRunning(bool on)
        {
            lock (_locker)
            {
                isRunning = on;
            }
        }

        public bool IsRunning()
        {
            lock (_locker)
            {
                return isRunning;
            }
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            SetMacroSwitch(true);
        }
        
        private void btStop_Click(object sender, EventArgs e)
        {
            SetMacroSwitch(false);
        }

        void AddLog(string msg)
        {
            if (tbLog != null)
            {
                try
                {
                    tbLog.AppendText(msg + "\r\n");
                    tbLog.ScrollToCaret();
                }
                catch
                {

                }
            }
        }

        public int OneClickMacro(MacroType type)
        {
            List<PointData> list = macro[type];
            foreach (PointData pd in list)
            {
                if (IsMacroSwitch() == false)
                {
                    SetRunning(false);
                    return 0;
                }

                if (pd.actionName == "반퀘체크")
                {
                    if (CheckUserInfo_RepeatQuest() == true)
                    {
                        BotSendMessage("반퀘 퀘스트 확인 클릭 안됨");
                        Thread.Sleep(pd.delay * 1000);
                        return 1;
                    }
                    else if (CheckUserInfo_NoQuest() == true)
                    {
                        BotSendMessage("퀘스트 없음");
                        Thread.Sleep(pd.delay * 1000);
                        return 2;
                    }
                }
				else if (pd.actionName == "상점종료체크")
				{
					while(CheckUserInfo_Shop())
					{
						Thread.Sleep(2000);
					}
				}

                imgMacro.InClick(pd.pt.X, pd.pt.Y);
                Thread.Sleep(pd.delay * 1000);
            }

            return 0;
        }

        private void setTelegramEvent()
        {
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            // Keyword,
            if (message.Text.IndexOf("1") == 0)
            {
                BotSendMessage("Quest Count = " + currentQuestCount.ToString());
            }
            else if (message.Text.IndexOf("2") == 0)
            {
                BotSendMessage("현재");
            }
        }

        private void MoonLight_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            Keys key = keyData & ~(Keys.Shift | Keys.Control);

            switch (key)
            {
                case Keys.L:
                    {
                        LoadXMl();
                    }
                    break;
                case Keys.Q:
                    {
                        GoQuestStart();
                    }
                    break;
                case Keys.A:
                    {
                        POINT pt = new POINT();
                        GetCursorPos(out pt);

                        pt.x = pt.x - programInfo.X;
                        pt.y = pt.y - (programInfo.Height - resolutionY);

                        AddLog(string.Format("X = {0}, Y = {1}", pt.x, pt.y));
                    }
                    break;
                case Keys.B:
                    {
                        OneClickMacro(MacroType.eItemBreak);
                    }
                    break;
                case Keys.S:
                    {
                        SetMacroSwitch(true);
                    }
                    break;
                case Keys.T:
                    {
                        SetMacroSwitch(false);
                    }
                    return true;
                case Keys.R:
                    {
                        AddMacro(MacroType.eShop);
                    }
                    break;
                case Keys.W:
                    {
                        if (saveCount == 0)
                        {
                            GetCursorPos(out prevPoint);
                            saveCount++;
                        }
                        else if (saveCount == 1)
                        {
                            saveCount = 0;
                            POINT curPT = new POINT();
                            GetCursorPos(out curPT);

                            curPT.x = curPT.x - programInfo.X;
                            curPT.y = curPT.y - programInfo.Y;

                            prevPoint.x = prevPoint.x - programInfo.X;
                            prevPoint.y = prevPoint.y - programInfo.Y;

                            Rectangle ptTemp = new Rectangle(prevPoint.x, prevPoint.y, curPT.x - prevPoint.x, curPT.y - prevPoint.y);
                            string ss = imgMacro.ScreenCapture(ptTemp, pbQuest);
                            ss = string.Format("{0} : {1}, {2}, {3}, {4}", ss, prevPoint.x, prevPoint.y, curPT.x - prevPoint.x, curPT.y - prevPoint.y);
                            AddLog(ss);
                        }
                    }
                    break;
                case Keys.F:
                    {
                        AddMacro(MacroType.eShop);
                    }
                    break;
                case Keys.V:
                    {
                        AddMacro(MacroType.eReQuest);
                    }
                    break;
                case Keys.P:
                    {
                        string qc2 = imgMacro.ScreenCapture(ssQuest2, null);
                        AddLog(qc2);
                    }
                    break;
                case Keys.Z:
                    {
                        imgMacro.InClick(_x, _y);
                    }
                    break;
                case Keys.X:
                    {
                        isAutoClick = false;
                    }
                    break;
            }


            return base.ProcessCmdKey(ref msg, keyData);
        }
        static int _x = 1024;
        static int _y = 257;
        public void RunAutoClick(object _pt)
        {
            POINT pt = (POINT)_pt;
            while (true)
            {
                if (isAutoClick == false)
                    return;

                if (IsRunning() == false && runMacroLis.Count == 0)
                {
                    try
                    {
                        imgMacro.InClick(pt.x, pt.y);
                    }
                    catch
                    {
                        BotSendMessage("RunMacro Error");
                    }

                }

                Thread.Sleep(2500);
            }
        }

        public void AddMacro(MacroType type)
        {
            if (runMacroLis.Contains(type) == false)
            {
                lock (_locker)
                {
                    runMacroLis.Add(type);
                    SetRunning(true);
                }
            }
        }

        public void AddFirstMacro(MacroType type)
        {
            if (runMacroLis.Contains(type) == false)
            {
                lock (_locker)
                {
                    runMacroLis.Insert(0, type);
                    SetRunning(true);
                }
            }
        }
        public void RemoveMacro()
        {
            lock (_locker)
            {
                runMacroLis.Clear();
            }
        }

        public void CheckUserInfo()
        {
            // HP
            string hp = imgMacro.ScreenCapture(ssHP, pbHP);
            try
            {
                hp = hp.Trim();
                foreach (string word in replaceWord)
                {
                    hp = hp.Replace(word, "");
                }

                AddLog("HP : " + hp);
                int nHP = int.Parse(hp);
                if (nHP < 6000 && nHP > 2000)
                {
                    BotSendMessage("HP : " + hp);
                    AddMacro(MacroType.eShop);
                    AddMacro(MacroType.eReturn);
                    return;
                }
            }
            catch
            {
            }

            // QuestComplate
            string qc = imgMacro.ScreenCapture(ssQuest, pbQuestComplate);
            string qc2 = imgMacro.ScreenCapture(ssQuest2, null);
            try
            {
                if (qc.IndexOf("퀘스트") >= 0 || qc.IndexOf("완료") >= 0)
                {
                    AddMacro(MacroType.eClick);
                    return;
                }

                AddLog("Quest = " + qc);
                AddLog("Quest2 = " + qc2);

                if (questStart == false && qc2.IndexOf("완료") >= 0 && stopwatch.Elapsed.Minutes > 1)
                {
                    BotSendMessage(string.Format("{0} (소요시간 : {1}분", "퀘스트 완료", stopwatch.Elapsed.Minutes));
                    stopwatch.Stop();

                    questStart = true;
                    stopwatch.Reset();
                    stopwatch.Start();

                    GoQuestStart();
                    return;
                }

                if (questStart == true && (qc2.Contains("서브") || qc2.Contains("반인") || qc2.Contains("탐린")))
                {
                    BotSendMessage("퀘스트 시작");
                    questStart = false;
                }

                if (questStart == true && stopwatch.Elapsed.Minutes > 1 && qc2.IndexOf("탐린") == -1)
                {
                    BotSendMessage("반복퀘스트 문제 생김");
                    RemoveMacro();
                    GoQuestReStart();
                }
            }
            catch
            {
            }

            // Scroll Quest
            string sq = imgMacro.ScreenCapture(ssSQuest, null);
            try
            {
                if (sq.IndexOf("두루마리") > -1)
                {
                    AddLog("ScrollQuest Complate = " + sq);
                    BotSendMessage("Scroll Quest Complate");
                    AddMacro(MacroType.eSQuest);
                    return;
                }
            }
            catch
            {
            }

            // Hunting
            string mobCnt = imgMacro.ScreenCapture(ssHunting, null);
            try
            {
                mobCnt = mobCnt.Trim();
                foreach (string word in replaceWord)
                {
                    mobCnt = mobCnt.Replace(word, "");
                }

                int mobCount = int.Parse(mobCnt);
                AddLog("mobCount = " + mobCount.ToString());

                if (mobCount == 500)
                {
                    AddMacro(MacroType.eHunting);
                    return;
                }
            }
            catch
            {

            }

            // QuestCount
            string questCount = imgMacro.ScreenCapture(ssQuestCount, pbQuest);
            try
            {
                questCount = questCount.Trim();
                AddLog("QuestCount = " + questCount);
                if (questCount.Contains("/"))
                {
                    questCount = questCount.Substring(0, questCount.IndexOf("/"));
                    int qCnt = 0;
                    try
                    {
                        qCnt = int.Parse(questCount);
                    }
                    catch
                    {
                        qCnt = -1;
                    }

                    if (currentQuestCount == qCnt && dieCheck.Elapsed.Minutes > 1)
                    {
                        BotSendMessage("캐릭터 다이 ? ? ? ");
                        dieCheck.Stop();
                        dieCheck.Start();
                    }

                    if (currentQuestCount != qCnt && qCnt > 0)
                    {
                        currentQuestCount = qCnt;
                    }

                    if (CheckItemBreak(qCnt) == true)
                        return;

                    if (questStart)
                    {
                        if (qCnt > 0)
                        {
                            BotSendMessage("퀘스트 시작");
                            questStart = false;
                        }

                        if (qCnt == 0 && stopwatch.Elapsed.Minutes > 1)
                        {
                            BotSendMessage(string.Format("{0} (걸린시간 : {1}분", "퀘스트 완료(다시시도)", stopwatch.Elapsed.Minutes));

                            GoQuestStart();
                            return;
                        }
                    }

                }
            }
            catch
            {

            }

            // Boss
            if (chkBoss.Checked == true)
            {
                string sBossMsg = imgMacro.ScreenCapture(ssBoss, null);
                try
                {
                    sBossMsg = sBossMsg.Trim();
                    if (sBossMsg.Contains("코카트리스") && bossTime.Ticks < DateTime.Now.Ticks)
                    {
                        bossTime = DateTime.Now.AddMinutes(5);

                        BotSendMessage(sBossMsg);
                    }
                }
                catch
                {

                }
            }

            // HuntingReward
            string sHuntingReward = imgMacro.ScreenCapture(ssHuntingReward, null);
            try
            {
                sHuntingReward = sHuntingReward.Trim();
                if (sHuntingReward.Contains("보상") || sHuntingReward.Contains("보샹"))
                {
                    AddMacro(MacroType.eClick);
                    return;
                }
            }
            catch
            {

            }

            // Go Home
            string sGoHome = imgMacro.ScreenCapture(ssGohome, null);
            try
            {
                sGoHome = sGoHome.Trim();
                if (sGoHome.Contains("집"))
                {
                    AddMacro(MacroType.eCancle);
                    return;
                }
            }
            catch
            {

            }

            // Safe Mode
            string sSafeMode = imgMacro.ScreenCapture(ssSafeMode, null);
			try
			{
                sSafeMode = sSafeMode.Trim();
				if (sSafeMode.Contains("절전") || sSafeMode.Contains("모드") || sSafeMode.Contains("해제"))
				{
                    AddMacro(MacroType.eSafeMode);
					return;
				}
			}
			catch
			{

			}

        }

        public bool CheckItemBreak(int cnt)
        {
            if (cnt <= 0)
                return false;

            int compareCnt = 0;
            for (int i = 0; i < 4; i++)
            {
                compareCnt = 100 * (i + 1);
                if (iteamBreate[i] == true && (cnt > compareCnt && cnt < (compareCnt + 50)))
                {
                    AddMacro(MacroType.eItemBreak);
                    iteamBreate[i] = false;
                    return true;
                }
            }

            return false;
        }

        public void GoQuestStart()
        {
            AddMacro(MacroType.eQuest);
            AddMacro(MacroType.eShop);
            AddMacro(MacroType.eReturn);
            SetItemBreak(true);
        }

        public void GoQuestReStart()
        {
            AddMacro(MacroType.eReQuest);
            AddMacro(MacroType.eShop);
            AddMacro(MacroType.eReturn);
            SetItemBreak(true);
        }

        public bool CheckUserInfo_RepeatQuest()
        {
            // RepeatedQuest
            string sRepeatedQuest = imgMacro.ScreenCapture(ssRepeatedQuest, null);
            try
            {
                if (sRepeatedQuest.Contains("반복") && sRepeatedQuest.Contains("퀘스트"))
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }

        public bool CheckUserInfo_NoQuest()
        {
            // qc2 Check
            string qc2 = imgMacro.ScreenCapture(ssQuest2, null);
            try
            {
                if (qc2.Contains("서브") || qc2.Contains("반인") || qc2.Contains("탐린"))
                {
                    BotSendMessage("퀘스트 시작");
                    questStart = false;
                    return false;
                }
            }
            catch
            {

            }

            return true;
        }

		public bool CheckUserInfo_Shop()
		{
			// qc2 Check
			string shop = imgMacro.ScreenCapture(ssInventory, null);
			try
			{
				if (shop.Contains("모두") || shop.Contains("장비") || shop.Contains("소모") || shop.Contains("음식") || shop.Contains("재료") || shop.Contains("기타"))
				{
					ActionData ad = dActionData["종료"];
					imgMacro.InClick(ad.pt.X, ad.pt.Y);
					return true;
				}
			}
			catch
			{

			}

			return false;
		}

		public int GetXmlHuntingGround()
        {
            List<PointData> hg = macro[MacroType.eReturn];
            foreach (PointData pd in hg)
            {
                if (pd.actionName.IndexOf("사냥터") != -1)
                {
                    return int.Parse(pd.actionName.Substring(3));
                }
            }

            return 0;
        }

        public string GetCurHuntingGround()
        {
            return cbHG.Items[cbHG.SelectedIndex].ToString();
        }

        private void cbHG_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int index = cbHG.SelectedIndex;
            string actionName = GetCurHuntingGround();
            ActionData ad = dActionData[actionName];

            List<PointData> hg = macro[MacroType.eReturn];
            for(int i = 0; i < hg.Count; i++)
            {
                PointData pd = hg[i];
                if (pd.actionName.IndexOf("사냥터") != -1)
                {
                    pd.actionName = ad.name;                    
                    pd.pt = ad.pt;
                    hg[i] = pd;
                    break;
                }
            }
        }
    }
}
