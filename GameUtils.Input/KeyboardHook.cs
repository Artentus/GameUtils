using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameUtils.Input.Win32;

namespace GameUtils.Input
{
    sealed class KeyboardHook : IDisposable
    {
        IntPtr hook;
        User32.HookProc callbackDelegate;

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        public KeyboardHook()
        {
            callbackDelegate = new User32.HookProc(ReceiveCallback);
            IntPtr hModule;
            using (var p = Process.GetCurrentProcess())
                using (var m = p.MainModule)
                    hModule = Kernel32.GetModuleHandle(m.ModuleName);         
            hook = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, callbackDelegate, hModule, 0);
        }

        private IntPtr ReceiveCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == 0)
            {                
                var hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var keys = (Keys)hookStruct.vkCode;
                if ((hookStruct.flags & KBDLLHOOKSTRUCTFlags.LLKHF_UP) == KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                {
                    if (KeyUp != null)
                        KeyUp(this, new KeyEventArgs(keys | Control.ModifierKeys));
                }
                else
                {
                    if (KeyDown != null)
                        KeyDown(this, new KeyEventArgs(keys | Control.ModifierKeys));
                }
            }
            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        public void Dispose()
        {
            User32.UnhookWindowsHookEx(hook);
        }
    }
}
