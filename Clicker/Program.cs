using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Clicker
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        const int VK_SHIFT = 0x10;

        static bool isRunning = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            new Thread(() =>
            {
                while (true)
                {
                    if ((GetAsyncKeyState((int)Keys.Escape) & 0x8000) != 0)
                        break;

                    bool shiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                    bool leftButtonPressed = (Control.MouseButtons & MouseButtons.Left) != 0;

                    if (shiftPressed && leftButtonPressed && !isRunning)
                    {
                        isRunning = true;

                        for (int i = 0; i < 30; i++)
                        {
                            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                            Thread.Sleep(1);
                        }

                        isRunning = false;
                    }
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();

            Application.Run(new Form
            {
                Size = new Size(300, 200),
                Text = "Clicker",
                Controls = {
                    new Label {
                        Text = "Hold Shift and click to use this feature",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    }
                }
            });
        }
    }
}


