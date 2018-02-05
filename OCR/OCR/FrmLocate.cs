using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OCR
{
    public partial class FrmLocate : Form
    {
        public FrmLocate()
        {
            InitializeComponent();
        }

        public Point StartPoint
        {
            get
            {
                return this.pStart;
            }
        }

        public Point EndPoint
        {
            get
            {
                return this.pEnd;
            }
        }

        Rectangle rectRefresh = new Rectangle(0, 0, 50, 50);
        Point pStart = Point.Empty, pEnd = Point.Empty, pCurr = Point.Empty;

        private void FrmLocate_MouseMove(object sender, MouseEventArgs e)
        {
            rectRefresh.Location = pCurr;
            rectRefresh.X -= 25;
            rectRefresh.Y -= 25;
            this.Invalidate(rectRefresh);
            pCurr = MousePosition;
        }

        private void FrmLocate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                pStart = MousePosition;
            else if (e.Button == MouseButtons.Right)
                pEnd = MousePosition;
            this.Invalidate();
        }

        private void FrmLocate_KeyDown(object sender, KeyEventArgs e)
        {
            this.Close();
        }

        private void FrmLocate_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Blue, pStart.X, pStart.Y, pEnd.X - pStart.X, pEnd.Y - pStart.Y);
            e.Graphics.FillEllipse(Brushes.Red, pStart.X - 5, pStart.Y - 5, 9, 9);
            e.Graphics.FillEllipse(Brushes.Green, pEnd.X - 5, pEnd.Y - 5, 9, 9);
            e.Graphics.FillRectangle(Brushes.Black, pCurr.X - 15, pCurr.Y - 2, 30, 3);
            e.Graphics.FillRectangle(Brushes.Black, pCurr.X - 2, pCurr.Y - 15, 3, 30);
        }
    }
}
