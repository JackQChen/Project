using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DesktopWidget.Properties;

namespace DesktopWidget
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        #region 半透明不规则窗体创建
        private void InitializeStyles()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();
        }


        protected override void OnHandleCreated(EventArgs e)
        {
            InitializeStyles();//设置窗口样式、双缓冲等
            base.OnHandleCreated(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cParms;
            }
        }

        public void SetBits(Bitmap bitmap)
        {
            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException("The picture must be 32bit picture with alpha channel.");

            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = Win32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = Win32.CreateCompatibleDC(screenDC);

            try
            {
                Win32.Point topLoc = new Win32.Point(Left, Top);
                Win32.Size bitMapSize = new Win32.Size(bitmap.Width, bitmap.Height);
                Win32.BLENDFUNCTION blendFunc = new Win32.BLENDFUNCTION();
                Win32.Point srcLoc = new Win32.Point(0, 0);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = Win32.SelectObject(memDc, hBitmap);

                blendFunc.BlendOp = Win32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = 255;
                blendFunc.AlphaFormat = Win32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;

                Win32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, Win32.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBits);
                    Win32.DeleteObject(hBitmap);
                }
                Win32.ReleaseDC(IntPtr.Zero, screenDC);
                Win32.DeleteDC(memDc);
            }
        }
        #endregion

        #region 无边框移动

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private const int VM_NCLBUTTONDOWN = 0XA1;//定义鼠标左键按下
        private const int HTCAPTION = 2;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            ReleaseCapture();
            SendMessage((IntPtr)this.Handle, VM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        #endregion

        #region 转化为桌面插件

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        const int GWL_HWNDPARENT = -8;
        protected override void OnShown(EventArgs e)
        {
            //该事件及设置顺序不可调整
            this.ShowInTaskbar = false;
            IntPtr hprog = FindWindowEx(
                  FindWindowEx(
                      FindWindow("Progman", "Program Manager"),
                      IntPtr.Zero, "SHELLDLL_DefView", ""
                  ),
                  IntPtr.Zero, "SysListView32", "FolderView"
              );
            SetWindowLong(this.Handle, GWL_HWNDPARENT, hprog);
        }

        #endregion

        Bitmap bmp;
        Graphics gBmp;
        //字体样式须为局部变量
        PrivateFontCollection pfc;
        Font font;
        string configPath = "";

        private void FrmMain_Load(object sender, EventArgs e)
        {
            var glass = Resources.glass;
            bmp = new Bitmap(this.Width, glass.Height * this.Width / glass.Width);
            gBmp = Graphics.FromImage(bmp);
            pfc = new PrivateFontCollection();
            var fontData = Resources.Font;
            IntPtr iFont = Marshal.AllocHGlobal(fontData.Length);
            Marshal.Copy(fontData, 0, iFont, fontData.Length);
            pfc.AddMemoryFont(iFont, fontData.Length);
            Marshal.FreeHGlobal(iFont);
            font = new Font(pfc.Families[0], 14.6f);
            configPath = Application.StartupPath + "\\dw.dat";
            if (File.Exists(configPath))
            {
                var location = File.ReadAllText(configPath).Split(',');
                this.Location = new Point(Convert.ToInt32(location[0]), Convert.ToInt32(location[1]));
            }
            this.timer1.Start();
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        GetWeather();
                    }
                    catch
                    {
                    }
                    Thread.Sleep(1000 * 60 * 60);
                }
            }) { IsBackground = true }.Start();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (string.IsNullOrEmpty(this.configPath))
                return;
            File.WriteAllText(configPath, this.Left + "," + this.Top);
        }

        string[] strWeather = new string[2];

        void GetWeather()
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.yahoo.com/news/weather/china/xian/xian-2157249");
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string responseContent = streamReader.ReadToEnd();
                    var weather = Regex.Match(responseContent, @"<span class=""description.+?</span>", RegexOptions.Singleline).Value;
                    var wArr = Regex.Match(weather, @">.+?<").Value.Replace("<", "").Replace(">", "").Split(' ');
                    weather = wArr[wArr.Length - 1];
                    var temp = Regex.Match(responseContent, @"<span class=""Va\(t\).*?</span>", RegexOptions.Singleline).Value;
                    temp = Regex.Match(temp, @">.+?<").Value.Replace("<", "").Replace(">", "");
                    var tempNum = Convert.ToInt32((Convert.ToInt32(temp) - 32) / 1.8);
                    strWeather[0] = weather + " " + tempNum + "`C";
                    httpWebResponse.Close();
                    streamReader.Close();
                }
            }
            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.pm25.in/xian");
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string responseContent = streamReader.ReadToEnd();
                    var pm25 = Regex.Match(responseContent, @"<td>高新西区.+?</tr>", RegexOptions.Singleline).Value;
                    var ms = Regex.Matches(pm25, "<td>.+?</td>", RegexOptions.Singleline);
                    strWeather[1] = Regex.Match(ms[4].Value, @">.+?<").Value.Replace("<", "").Replace(">", "");
                    httpWebResponse.Close();
                    streamReader.Close();
                }
            }
        }

        string[] GetTimeInfo()
        {
            var strArr = new string[5];
            var dtStart = DateTime.Now.AddMilliseconds(0 - Environment.TickCount);
            strArr[0] = dtStart.ToString("HH:mm:ss");
            var tsRemain = dtStart.AddHours(9.5) - DateTime.Now;
            strArr[1] = tsRemain.TotalMilliseconds > 0 ? tsRemain.ToString().Split('.')[0] : "00:00:00";
            strArr[2] = tsRemain.TotalMilliseconds < 0 ? tsRemain.ToString().Replace("-", "").Split('.')[0] : "00:00:00";
            strArr[3] = strWeather[0];
            strArr[4] = strWeather[1];
            return strArr;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            gBmp.Clear(Color.Transparent);
            gBmp.FillRectangle(new SolidBrush(Color.FromArgb(147, 174, 97)), 32, 33, 310, 210);
            gBmp.DrawString(string.Format(@"
StartTime {0}

 RestTime {1}

 OverTime {2}

  Weather {3}

    PM2.5 {4}", GetTimeInfo()),
            font, Brushes.Black, 55, 34);
            gBmp.DrawImage(Resources.glass, 0, 0, bmp.Width, bmp.Height);
            SetBits(bmp);
        }
    }
}
