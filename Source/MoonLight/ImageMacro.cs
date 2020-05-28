using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using OpenCvSharp;
using System.Windows.Forms;

namespace MoonLight
{
	class ImageMacro
    {
        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement);

        [System.Runtime.InteropServices.DllImport("User32", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string lpszClass, string lpszWindows);

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;

        public Dictionary<string, Bitmap> picture = new Dictionary<string, Bitmap>();
        public IntPtr gameProcess;
        public IntPtr gameProcess_child;

        public Graphics Graphicsdata;
        public Rectangle programRect;
        public Bitmap fullScreen;

        public void Init()
        {
            gameProcess = FindWindow(null, "본캐");
			if (gameProcess.ToInt32() == 0)
			{
				MessageBox.Show("찾는 프로그램이 없습니다.\n프로그램을 키고 실행해주시기 바랍니다.");
				Environment.Exit(0);
			}

			gameProcess_child = FindWindowEx(gameProcess, IntPtr.Zero, "RenderWindow", "TheRender");

			//찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
			Graphicsdata = Graphics.FromHwnd(gameProcess);

            //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
            GetProgramRect(ref programRect);

            //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
            fullScreen = new Bitmap(programRect.Width, programRect.Height);

            LoadImage();
        }

        public void GetProgramRect(ref Rectangle rect)
        {
            if (gameProcess.ToInt32() == 0)
                return;

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);

            GetWindowPlacement(gameProcess, ref placement);

			rect = new Rectangle(placement.normal_position.Left, placement.normal_position.Top, placement.normal_position.Right - (placement.normal_position.Left * 2), placement.normal_position.Bottom - (placement.normal_position.Top * 2));
        }

        private void LoadImage()
        {
            picture.Add("자동사냥", new Bitmap(@"img\자동사냥.png"));
            picture.Add("귀환주문서", new Bitmap(@"img\귀환주문서.png"));
            picture.Add("타겟찾기", new Bitmap(@"img\타겟찾기.png"));
            picture.Add("장비개조", new Bitmap(@"img\장비개조.png"));
            picture.Add("장비특화", new Bitmap(@"img\장비특화.png"));
        }

        public Bitmap GetBitmap(string name)
        {
            return picture[name];
        }

        public Bitmap GetFullScreen()
        {
            return fullScreen;
        }

        public POINT FindPoint(string imgName)
        {
            Bitmap fullScreen = GetFullScreen();
            OpenCvSharp.Point pt = searchIMG(fullScreen, picture[imgName]);
            return new POINT(pt.X, pt.Y);
        }

        public void UpdateFullScreen()
        {
            if (gameProcess != IntPtr.Zero)
            {               
                try
                {
                    //비트맵을 바탕으로 그래픽스 함수로 선언해줍니다.
                    using (Graphics g = Graphics.FromImage(fullScreen))
                    {
                        //찾은 플레이어의 크기만큼 화면을 캡쳐합니다.
                        IntPtr hdc = g.GetHdc();
                        PrintWindow(gameProcess, hdc, 0x2);
                        g.ReleaseHdc(hdc);
                    }
                }
                catch
                {

                }
            }
        }
        
        public string ScreenCapture(Rectangle rect, PictureBox pb)
        {
            try
            {
                string msg;
                using (var engine = new TesseractEngine(@"./tessdata", "kor", EngineMode.Default))
                {
                    Bitmap image = ResizeImage(rect);
                    Pix pix = PixConverter.ToPix(image);
                    var result = engine.Process(pix);
                    msg = result.GetText();
                }

                return msg;
            }
            catch
            {
                return "";
            }
        }
		public Bitmap ResizeImage(Rectangle rect)
		{
			var destRect = new Rectangle(0, 0, rect.Width, rect.Height);
			var destImage = new Bitmap(rect.Width, rect.Height);

			destImage.SetResolution(fullScreen.HorizontalResolution, fullScreen.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(fullScreen, destRect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}

		//x,y 값을 전달해주면 클릭이벤트를 발생합니다.
		public void InClick(int x, int y)
        {
            //플레이어를 찾았을 경우 클릭이벤트를 발생시킬 핸들을 가져옵니다.
            IntPtr lparam = new IntPtr(x | (y << 16));

            //플레이어 핸들에 클릭 이벤트를 전달합니다.
            SendMessage(gameProcess_child, WM_LBUTTONDOWN, 1, lparam);
            SendMessage(gameProcess_child, WM_LBUTTONUP, 0, lparam);
        }


        public OpenCvSharp.Point searchIMG(Bitmap screen_img, Bitmap find_img)
        {
            OpenCvSharp.Point result = new OpenCvSharp.Point();
            //스크린 이미지 선언
            using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img))
            //찾을 이미지 선언
            using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img))
            //스크린 이미지에서 FindMat 이미지를 찾아라
            using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed))
            {
                //찾은 이미지의 유사도를 담을 더블형 최대 최소 값을 선언합니다.
                double minval, maxval = 0;
                //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                OpenCvSharp.Point minloc, maxloc;
                //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);
                Debug.WriteLine("찾은 이미지의 유사도 : " + maxval);

                //이미지를 찾았을 경우 클릭이벤트를 발생!!
                if (maxval >= 0.8)
                {
                    result = maxloc;
                    //InClick(maxloc.X, maxloc.Y);
                    return result;
                }
            }

            return result;
        }

    }
}
