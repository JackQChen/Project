using System.Collections.Generic;
using System.Windows.Forms;
using VirtualKey.Properties;

namespace VirtualKey
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Hook hook;
        WinRing winRing;
        bool inNum = false;
        Dictionary<Keys, char> dicKeys;

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            hook = new Hook();
            hook.OnKey += new Hook.Func(hook_OnKey);
            hook.Start();
            winRing = new WinRing();
            winRing.Init();
            dicKeys = new Dictionary<Keys, char>();
            dicKeys.Add(Keys.M, '0');
            dicKeys.Add(Keys.J, 'a');
            dicKeys.Add(Keys.K, 'b');
            dicKeys.Add(Keys.L, 'c');
            dicKeys.Add(Keys.U, 'd');
            dicKeys.Add(Keys.I, 'e');
            dicKeys.Add(Keys.O, 'f');
            this.notifyIcon1.Icon = Resources.off;
        }

        private void MainForm_Shown(object sender, System.EventArgs e)
        {
            this.Hide();
        }

        bool hook_OnKey(Hook.KeyBoardHookStruct key)
        {
            if (key.vkCode == 93 || key.vkCode == 49)
            {
                if (key.flags == 1)
                    inNum = !inNum;
                this.notifyIcon1.Icon = inNum ? Resources.on : Resources.off;
                return true;
            }
            else if (inNum)
            {
                if (key.flags == 0)
                {
                    var k = (Keys)key.vkCode;
                    if (dicKeys.ContainsKey(k))
                    {
                        winRing.KeyDown(dicKeys[k]);
                        return true;
                    }
                }
            }
            return false;
        }

        private void 退出ToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            hook.Stop();
        }
    }
}
