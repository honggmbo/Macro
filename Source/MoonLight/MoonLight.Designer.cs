using System.Windows.Forms;

namespace MoonLight
{
    partial class MoonLight
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btReturn = new System.Windows.Forms.Button();
            this.btIteamBreak = new System.Windows.Forms.Button();
            this.btQuest = new System.Windows.Forms.Button();
            this.btStart = new System.Windows.Forms.Button();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btStop = new System.Windows.Forms.Button();
            this.pbQuest = new System.Windows.Forms.PictureBox();
            this.pbHP = new System.Windows.Forms.PictureBox();
            this.pbQuestComplate = new System.Windows.Forms.PictureBox();
            this.chkBoss = new System.Windows.Forms.CheckBox();
            this.cbHG = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbQuest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbQuestComplate)).BeginInit();
            this.SuspendLayout();
            // 
            // btReturn
            // 
            this.btReturn.Location = new System.Drawing.Point(389, 12);
            this.btReturn.Name = "btReturn";
            this.btReturn.Size = new System.Drawing.Size(168, 37);
            this.btReturn.TabIndex = 0;
            this.btReturn.Text = "Return";
            this.btReturn.UseVisualStyleBackColor = true;
            this.btReturn.Click += new System.EventHandler(this.btReturn_Click);
            // 
            // btIteamBreak
            // 
            this.btIteamBreak.Location = new System.Drawing.Point(389, 64);
            this.btIteamBreak.Name = "btIteamBreak";
            this.btIteamBreak.Size = new System.Drawing.Size(168, 37);
            this.btIteamBreak.TabIndex = 1;
            this.btIteamBreak.Text = "IteamBreak";
            this.btIteamBreak.UseVisualStyleBackColor = true;
            this.btIteamBreak.Click += new System.EventHandler(this.btItemBreak_Click);
            // 
            // btQuest
            // 
            this.btQuest.Location = new System.Drawing.Point(389, 118);
            this.btQuest.Name = "btQuest";
            this.btQuest.Size = new System.Drawing.Size(168, 37);
            this.btQuest.TabIndex = 2;
            this.btQuest.Text = "Quest";
            this.btQuest.UseVisualStyleBackColor = true;
            this.btQuest.Click += new System.EventHandler(this.btQuest_Click);
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(389, 170);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(168, 37);
            this.btStart.TabIndex = 3;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // tbLog
            // 
            this.tbLog.Location = new System.Drawing.Point(12, 12);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(362, 143);
            this.tbLog.TabIndex = 4;
            // 
            // btStop
            // 
            this.btStop.Location = new System.Drawing.Point(389, 223);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(168, 37);
            this.btStop.TabIndex = 5;
            this.btStop.Text = "Stop";
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.Click += new System.EventHandler(this.btStop_Click);
            // 
            // pbQuest
            // 
            this.pbQuest.Location = new System.Drawing.Point(12, 161);
            this.pbQuest.Name = "pbQuest";
            this.pbQuest.Size = new System.Drawing.Size(184, 33);
            this.pbQuest.TabIndex = 6;
            this.pbQuest.TabStop = false;
            // 
            // pbHP
            // 
            this.pbHP.Location = new System.Drawing.Point(12, 200);
            this.pbHP.Name = "pbHP";
            this.pbHP.Size = new System.Drawing.Size(184, 35);
            this.pbHP.TabIndex = 7;
            this.pbHP.TabStop = false;
            // 
            // pbQuestComplate
            // 
            this.pbQuestComplate.Location = new System.Drawing.Point(12, 241);
            this.pbQuestComplate.Name = "pbQuestComplate";
            this.pbQuestComplate.Size = new System.Drawing.Size(184, 75);
            this.pbQuestComplate.TabIndex = 9;
            this.pbQuestComplate.TabStop = false;
            // 
            // chkBoss
            // 
            this.chkBoss.AutoSize = true;
            this.chkBoss.Location = new System.Drawing.Point(211, 161);
            this.chkBoss.Name = "chkBoss";
            this.chkBoss.Size = new System.Drawing.Size(72, 16);
            this.chkBoss.TabIndex = 10;
            this.chkBoss.Text = "보스알림";
            this.chkBoss.UseVisualStyleBackColor = true;
            // 
            // cbHG
            // 
            this.cbHG.FormattingEnabled = true;
            this.cbHG.Location = new System.Drawing.Point(253, 183);
            this.cbHG.Name = "cbHG";
            this.cbHG.Size = new System.Drawing.Size(106, 20);
            this.cbHG.TabIndex = 11;
            this.cbHG.SelectionChangeCommitted += new System.EventHandler(this.cbHG_SelectionChangeCommitted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(209, 186);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "사냥터";
            // 
            // MoonLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 328);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbHG);
            this.Controls.Add(this.chkBoss);
            this.Controls.Add(this.pbQuestComplate);
            this.Controls.Add(this.pbHP);
            this.Controls.Add(this.pbQuest);
            this.Controls.Add(this.btStop);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.btQuest);
            this.Controls.Add(this.btIteamBreak);
            this.Controls.Add(this.btReturn);
            this.Name = "MoonLight";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MoonLight";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoonLight_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbQuest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbQuestComplate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btReturn;
        private System.Windows.Forms.Button btIteamBreak;
        private System.Windows.Forms.Button btQuest;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btStop;
        private System.Windows.Forms.PictureBox pbQuest;
        private System.Windows.Forms.PictureBox pbHP;
        private PictureBox pbQuestComplate;
        private CheckBox chkBoss;
        private ComboBox cbHG;
        private Label label1;
    }
}

