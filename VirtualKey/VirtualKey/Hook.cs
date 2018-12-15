using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VirtualKey
{
    public class Hook : IDisposable
    {
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        public delegate bool Func(KeyBoardHookStruct key);

        private int hHook;
        public const int WH_KEYBOARD_LL = 13;
        private HookProc KeyBoardHookProcedure;
        public event Func OnKey;

        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void Start()
        {
            // 安装键盘钩子
            if (hHook == 0)
            {
                KeyBoardHookProcedure = KeyBoardHookProc;
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyBoardHookProcedure,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                //如果设置钩子失败
                if (hHook == 0)
                    Stop();
            }
        }

        public void Stop()
        {
            var retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
        }

        public int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
                if (OnKey != null)
                    if (OnKey(kbh))
                        return 1;
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
