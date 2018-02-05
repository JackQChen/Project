using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OCR
{
    public class FrmMain : Form
    {

        int iStart;
        Hotkey hotKey;

        public FrmMain()
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            hotKey = new Hotkey(this.Handle);
            iStart = hotKey.RegisterHotkey(Keys.F9, Hotkey.KeyFlags.MOD_NONE);
            hotKey.OnHotkey += new HotkeyEventHandler(hotKey_OnHotkey);
            this.Visible = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            hotKey.UnregisterHotkeys();
        }

        void hotKey_OnHotkey(int hotKeyID)
        {
            if (hotKeyID == iStart)
            {
                using (FrmLocate frm = new FrmLocate())
                {
                    frm.ShowDialog(this);
                    Rectangle rectArea = new Rectangle(frm.StartPoint.X, frm.StartPoint.Y, frm.EndPoint.X - frm.StartPoint.X, frm.EndPoint.Y - frm.StartPoint.Y);
                    var imgPath = AppDomain.CurrentDomain.BaseDirectory + "ocr.jpg";
                    using (Bitmap bmpFull = new Bitmap(rectArea.Width, rectArea.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmpFull))
                        {
                            g.CopyFromScreen(rectArea.Location, Point.Empty, rectArea.Size);
                            //灰度化
                            ImageProcess.ToGrey(bmpFull);
                            //二值化
                            ImageProcess.Thresholding(bmpFull);
                            bmpFull.Save(imgPath, ImageFormat.Jpeg);
                        }
                        string strContent = Marshal.PtrToStringAnsi(AspriseOCR.OCRpart(imgPath, 0, 0, 0, bmpFull.Width, bmpFull.Height));
                        MessageBox.Show(strContent);
                    }
                }
            }
        }
    }
}
