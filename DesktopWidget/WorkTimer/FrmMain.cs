using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WorkTimer.Properties;

namespace WorkTimer
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
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; //WS_EX_LAYERED
                return cp;
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

        private bool isMouseDown = false;
        private Point mouseOffset;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                mouseOffset.X = this.Left - Control.MousePosition.X;
                mouseOffset.Y = this.Top - Control.MousePosition.Y;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isMouseDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown)
            {
                var point = Control.MousePosition;
                point.Offset(mouseOffset);
                this.Location = point;
            }
            base.OnMouseMove(e);
        }

        #endregion

        #region 转化为桌面插件

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        const int GWL_HWNDPARENT = -8;
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const int SE_SHUTDOWN_PRIVILEGE = 0x13;
        const int WM_WINDOWPOSCHANGED = 0x47;

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
            base.OnShown(e);
        }

        bool inProc = false;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_WINDOWPOSCHANGED)
            {
                if (inProc)
                    return;
                inProc = true;
                SetWindowPos(this.Handle, 1, 0, 0, 0, 0, SE_SHUTDOWN_PRIVILEGE);
                inProc = false;
            }
            base.WndProc(ref m);
        }

        #endregion

        Bitmap bmp, bmpBg, bmpBtn, bmpLight;
        Graphics gBmp;
        //字体样式须为局部变量
        PrivateFontCollection pfc;
        Font fontInfo, fontBtn;
        string configPath = "";
        Dictionary<int, Rectangle> dicBtn = new Dictionary<int, Rectangle>();
        Dictionary<int, Rectangle> dicCurrBtn = new Dictionary<int, Rectangle>();
        SolidBrush brush = new SolidBrush(Color.FromArgb(28, 203, 229));
        StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //直接调用Resource对象会导致内存开销变大
            bmpBg = Resources.bg;
            bmpBtn = Resources.btn;
            bmpLight = Resources.light;
            bmp = new Bitmap(this.Width, bmpBg.Height * this.Width / bmpBg.Width);
            gBmp = Graphics.FromImage(bmp);
            pfc = new PrivateFontCollection();
            var fontData = Resources.Font;
            IntPtr iFont = Marshal.AllocHGlobal(fontData.Length);
            Marshal.Copy(fontData, 0, iFont, fontData.Length);
            pfc.AddMemoryFont(iFont, fontData.Length);
            Marshal.FreeHGlobal(iFont);
            fontInfo = new Font(pfc.Families[0], 24f);
            fontBtn = new Font(pfc.Families[0], 16);
            configPath = Application.StartupPath + "\\dw.dat";
            if (File.Exists(configPath))
            {
                var location = File.ReadAllText(configPath).Split(',');
                this.Location = new Point(Convert.ToInt32(location[0]), Convert.ToInt32(location[1]));
            }
            this.dicBtn[0] = new Rectangle(260, 52, bmpBtn.Width, bmpBtn.Height);
            this.dicBtn[1] = new Rectangle(260, 87, bmpBtn.Width, bmpBtn.Height);
            this.dicBtn[2] = new Rectangle(260, 122, bmpBtn.Width, bmpBtn.Height);
            this.dicCurrBtn[0] = Rectangle.Empty;
            this.dicCurrBtn[1] = Rectangle.Empty;
            new Thread(() =>
            {
                dtStart = new EventLog("System").Entries.Cast<EventLogEntry>().Where(p => p.TimeGenerated.Date == DateTime.Now.Date).Min(s => s.TimeGenerated);
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

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            this.Draw();
            this.timer1.Start();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (string.IsNullOrEmpty(this.configPath))
                return;
            File.WriteAllText(configPath, this.Left + "," + this.Top);
        }

        DateTime dtStart = DateTime.MinValue;
        string[] strTime = new string[3] { "--", "RestTime", "--" };
        string[] strWeather = new string[3] { "--", "--", "--" };

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
                    var temp = Regex.Match(responseContent, @"<span class=""Va\(t\).+?</span>", RegexOptions.Singleline).Value;
                    temp = Regex.Match(temp, @">.+?<").Value.Replace("<", "").Replace(">", "");
                    var tempNum = Convert.ToInt32((Convert.ToInt32(temp) - 32) / 1.8);
                    strWeather[0] = weather;
                    strWeather[1] = tempNum + "`C";
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
                    strWeather[2] = Regex.Match(ms[4].Value, @">.+?<").Value.Replace("<", "").Replace(">", "");
                    httpWebResponse.Close();
                    streamReader.Close();
                }
            }
        }

        string[] GetTimeInfo()
        {
            if (dtStart == DateTime.MinValue)
                return strTime.Concat(strWeather).ToArray();
            strTime[0] = dtStart.ToString("HH:mm:ss");
            var tsRemain = dtStart.AddHours(9.5) - DateTime.Now;
            if (tsRemain.TotalMilliseconds > 0)
            {
                strTime[1] = "RestTime";
                strTime[2] = tsRemain.ToString().Split('.')[0];
            }
            else
            {
                strTime[1] = "OverTime";
                strTime[2] = tsRemain.ToString().Replace("-", "").Split('.')[0];
            }
            return strTime.Concat(strWeather).ToArray();
        }

        void Draw()
        {
            gBmp.Clear(Color.Transparent);
            gBmp.DrawImage(bmpBg, 0, 0, bmp.Width, bmp.Height);
            this.DrawButton();
            string strText = string.Format(@"
StartTime  {0}
 {1}  {2}
  Weather  {3}
      Temp.  {4}
      pm2.5  {5}", this.GetTimeInfo());
            int y = -10;
            foreach (var text in strText.Split('\r'))
            {
                this.gBmp.DrawString(text, this.fontInfo, brush, 67, y);
                y += 30;
            }
            SetBits(bmp);
        }

        void DrawButton()
        {
            DrawButton("Setting", dicBtn[0]);
            DrawButton("Statistics", dicBtn[1]);
            DrawButton("Exit", dicBtn[2]);
            if (dicCurrBtn[0] != Rectangle.Empty)
                gBmp.DrawImage(bmpLight, dicCurrBtn[0].X - 20, dicCurrBtn[0].Y, dicCurrBtn[0].Width + 50, dicCurrBtn[0].Height + 3);
        }

        void DrawButton(string text, Rectangle rect)
        {
            if (dicCurrBtn[1] == rect)
            {
                gBmp.DrawImage(bmpBtn, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                rect.Offset(1, 0);
            }
            else
                gBmp.DrawImage(bmpBtn, rect);
            rect.Offset(1, 2);
            this.gBmp.DrawString(text, fontBtn, brush, rect, stringFormat);
        }

        private void FrmMain_MouseMove(object sender, MouseEventArgs e)
        {
            var rectCurr = dicBtn.Where(p => p.Value.Contains(e.Location)).FirstOrDefault().Value;
            if (rectCurr != dicCurrBtn[0])
            {
                dicCurrBtn[0] = rectCurr;
                this.Draw();
            }
        }

        private void FrmMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (dicCurrBtn[0] != Rectangle.Empty)
                {
                    dicCurrBtn[1] = dicCurrBtn[0];
                    this.Draw();
                }
            }
        }

        private void FrmMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (dicCurrBtn[1] != Rectangle.Empty)
            {
                if (dicCurrBtn[1] == dicBtn[2])
                {
                    Application.Exit();
                }
                else
                {
                    dicCurrBtn[1] = Rectangle.Empty;
                    this.Draw();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();
        }
    }
}
