using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Web.Script.Serialization;

namespace Clicker
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        string configPath = AppDomain.CurrentDomain.BaseDirectory + "config.dat";
        Point[] pLocate;
        Rectangle rectTabel = Rectangle.Empty;

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (File.Exists(configPath))
            {
                JavaScriptSerializer convert = new JavaScriptSerializer();
                this.pLocate = convert.Deserialize<Point[]>(File.ReadAllText(configPath));
                SetRectangle();
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            JavaScriptSerializer convert = new JavaScriptSerializer();
            File.WriteAllText(configPath, convert.Serialize(pLocate));
        }

        private void btnLocate_Click(object sender, EventArgs e)
        {
            using (FrmLocate frm = new FrmLocate())
            {
                frm.ShowDialog(this);
                this.pLocate = new Point[] { frm.StartPoint, frm.EndPoint };
                SetRectangle();
            }
        }

        void SetRectangle()
        {
            this.rectTabel.Location = pLocate[0];
            this.rectTabel.Width = pLocate[1].X - pLocate[0].X;
            this.rectTabel.Height = pLocate[1].Y - pLocate[0].Y;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var pRec = MousePosition;
            if (Process())
                VirtualMouse.SetCursorPos(pRec.X, pRec.Y);
        }

        bool Process()
        {
            List<Point> listPoint = new List<Point>();
            //5*5
            int r = 5;
            for (int i = 0; i < r; i++)
                for (int j = 0; j < r; j++)
                    listPoint.Add(new Point(i * rectTabel.Width / r, j * rectTabel.Height / r));
            List<Area> listArea = new List<Area>();
            var imgPath = AppDomain.CurrentDomain.BaseDirectory + "full.jpg";
            using (Bitmap bmpFull = new Bitmap(rectTabel.Width, rectTabel.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpFull))
                {
                    g.CopyFromScreen(rectTabel.Location, Point.Empty, rectTabel.Size);
                    //灰度化
                    ImageProcess.ToGrey(bmpFull);
                    //二值化
                    ImageProcess.Thresholding(bmpFull);
                    bmpFull.Save(imgPath, ImageFormat.Jpeg);
                }
                Rectangle rectCell = new Rectangle(0, 0, rectTabel.Width / r - 10, rectTabel.Height / r - 10);
                foreach (var p in listPoint)
                {
                    var pOffset = p;
                    //offset
                    pOffset.Offset(5, 10);
                    var pPos = p;
                    pPos.Offset(rectTabel.Location);
                    try
                    {
                        string s = Marshal.PtrToStringAnsi(AspriseOCR.OCRpart(imgPath, 0, pOffset.X, pOffset.Y, rectCell.Width, rectCell.Height))
                            .Replace("O", "0").Replace(" ", "");
                        listArea.Add(new Area()
                        {
                            Number = Convert.ToInt32(s),
                            Position = pPos
                        });
                    }
                    catch
                    {
                        Bitmap bmpErr = new Bitmap(rectCell.Width, rectCell.Height);
                        Graphics g = Graphics.FromImage(bmpErr);
                        g.DrawImage(bmpFull, rectCell, pOffset.X, pOffset.Y, bmpErr.Width, bmpErr.Height, GraphicsUnit.Pixel);
                        this.picErr.Image = bmpErr;
                        this.labState.BackColor = Color.Red;
                        return false;
                    }
                }
            }
            DoMouseAction(listArea);
            this.labState.BackColor = Color.Green;
            this.statusStrip1.Refresh();
            if (this.chkSeries.Checked)
            {
                Thread.Sleep(200);
                Process();
            }
            return true;
        }

        private void DoMouseAction(List<Area> listArea)
        {
            listArea.Sort((c1, c2) =>
            {
                return c1.Number - c2.Number;
            });
            var iLst = listArea[0].Number - 1;
            foreach (var area in listArea)
            {
                Console.WriteLine("Number:" + area.Number);
                if (area.Number - iLst == 1)
                    iLst = area.Number;
                else
                    break;
                VirtualMouse.SetCursorPos(area.Position.X + 40, area.Position.Y + 40);
                VirtualMouse.mouse_event(VirtualMouse.MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                VirtualMouse.mouse_event(VirtualMouse.MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("Click:" + area.Number);
                Thread.Sleep(1);
            }
        }
    }
}
