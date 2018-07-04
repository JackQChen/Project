using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DesktopWidget
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //using (var frm = new Form())
            //{
            //    frm.FormBorderStyle = FormBorderStyle.None;
            //    //frm.ShowInTaskbar = false;
            //    frm.Size = Size.Empty;
            //    frm.Show();
            //    frm.Close();
            //}
            Application.Run(new FrmMain());
        }
    }
}
