using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace OCR
{
    public delegate void HotkeyEventHandler(int hotKeyID);
    public class Hotkey : System.Windows.Forms.IMessageFilter
    {
        List<UInt32> keyIDs = new List<UInt32>();
        IntPtr hWnd;

        public event HotkeyEventHandler OnHotkey;

        public enum KeyFlags
        {
            MOD_NONE = 0x0,
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8
        }
        [DllImport("user32.dll")]
        public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

        [DllImport("user32.dll")]
        public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalAddAtom(String lpString);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);

        public Hotkey(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            Application.AddMessageFilter(this);
        }

        public int RegisterHotkey(Keys Key, KeyFlags keyflags)
        {
            UInt32 hotkeyid = GlobalAddAtom(System.Guid.NewGuid().ToString());
            RegisterHotKey((IntPtr)hWnd, hotkeyid, (UInt32)keyflags, (UInt32)Key);
            keyIDs.Add(hotkeyid);
            return (int)hotkeyid;
        }

        public void UnregisterHotkeys()
        {
            Application.RemoveMessageFilter(this);
            foreach (UInt32 key in keyIDs)
            {
                UnregisterHotKey(hWnd, key);
                GlobalDeleteAtom(key);
            }
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 0x312)
            {
                if (OnHotkey != null)
                {
                    var para = (UInt32)m.WParam;
                    if (keyIDs.FindAll(key => { return para == key; }).Count > 0)
                    {
                        OnHotkey((int)m.WParam);
                        return true;
                    }
                }
            }
            return false;
        }
    }

}