using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace DesktopWidget
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        const int GWL_HWNDPARENT = -8;
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        DispatcherTimer timer = new DispatcherTimer();
        Model model = new Model();
        string configPath = "";
        DateTime dtStart = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = model;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            var handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var hWnd = FindWindow("Progman", "Program Manager");
            hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", "");
            hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysListView32", "FolderView");
            hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysHeader32", "");
            SetWindowLong(handle, GWL_HWNDPARENT, hWnd);
            configPath = AppDomain.CurrentDomain.BaseDirectory + "dw.dat";
            if (File.Exists(configPath))
            {
                var location = File.ReadAllText(configPath).Split(',');
                this.Left = Convert.ToInt32(location[0]);
                this.Top = Convert.ToInt32(location[1]);
            }
            new Thread(() =>
            {
                dtStart = new EventLog("System").Entries.Cast<EventLogEntry>().Where(p => p.TimeGenerated.Date == DateTime.Now.Date).Min(s => s.TimeGenerated);
                model.Text1 = dtStart.ToString("HH:mm:ss");
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
            })
            { IsBackground = true }.Start();
        }

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
                    model.Text3 = weather;
                    model.Text4 = tempNum + "`C";
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
                    var aqi = Regex.Match(responseContent, @"<td>高新西区.+?</tr>", RegexOptions.Singleline).Value;
                    var ms = Regex.Matches(aqi, "<td>.+?</td>", RegexOptions.Singleline);
                    model.Text5 = Regex.Match(ms[1].Value, @">.+?<").Value.Replace("<", "").Replace(">", "");
                    httpWebResponse.Close();
                    streamReader.Close();
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Point position = e.GetPosition(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (position.X >= 0 && position.X < this.ActualWidth && position.Y >= 0 && position.Y < this.ActualHeight)
                {
                    this.DragMove();
                }
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (string.IsNullOrEmpty(this.configPath))
                return;
            File.WriteAllText(configPath, this.Left + "," + this.Top);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (dtStart == DateTime.MinValue)
                return;
            var tsRemain = dtStart.AddHours(9.5) - DateTime.Now;
            var strTime = string.Empty;
            if (tsRemain.TotalMilliseconds > 0)
            {
                strTime = " RestTime " + tsRemain.ToString().Split('.')[0];
            }
            else
            {
                strTime = " OverTime " + tsRemain.ToString().Replace("-", "").Split('.')[0];
            }
            model.Text2 = strTime;
        }
    }

    public class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private string text1 = "--";
        public string Text1 { get { return "StartTime " + text1; } set { text1 = value; OnPropertyChanged("Text1"); } }
        private string text2 = " RestTime --";
        public string Text2 { get { return text2; } set { text2 = value; OnPropertyChanged("Text2"); } }
        private string text3 = "--";
        public string Text3 { get { return "  Weather " + text3; } set { text3 = value; OnPropertyChanged("Text3"); } }
        private string text4 = "--";
        public string Text4 { get { return "    Temp. " + text4; } set { text4 = value; OnPropertyChanged("Text4"); } }
        private string text5 = "--";
        public string Text5 { get { return "      AQI " + text5; } set { text5 = value; OnPropertyChanged("Text5"); } }
    }
}
