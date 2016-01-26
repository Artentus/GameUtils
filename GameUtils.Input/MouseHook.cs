using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameUtils.Input.Win32;

namespace GameUtils.Input
{
    public sealed class MouseHook : IDisposable
    {
        IntPtr hook;
        User32.HookProc callbackDelegate;

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseWheel;
        public event MouseEventHandler MouseMove;

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_MBUTTONUP = 0x0208;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_MOUSEWHEEL = 0x020A;

        MouseButtons buttons;

        public MouseHook()
        {
            callbackDelegate = new User32.HookProc(ReceiveCallback);
            IntPtr hModule;
            using (var p = Process.GetCurrentProcess())
                using (var m = p.MainModule)
                    hModule = Kernel32.GetModuleHandle(m.ModuleName);         
            hook = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, callbackDelegate, hModule, 0);
        }

        private IntPtr ReceiveCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == 0)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                switch (wParam.ToInt32())
                {
                    case WM_LBUTTONDOWN:
                        buttons |= MouseButtons.Left;
                        if (MouseDown != null)
                            MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));                         
                        break;
                    case WM_LBUTTONUP:
                        buttons &= ~MouseButtons.Left;
                        if (MouseUp != null)
                            MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_MBUTTONDOWN:
                        buttons |= MouseButtons.Middle;
                        if (MouseDown != null)
                            MouseDown(this, new MouseEventArgs(MouseButtons.Middle, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_MBUTTONUP:
                        buttons &= ~MouseButtons.Middle;
                        if (MouseUp != null)
                            MouseUp(this, new MouseEventArgs(MouseButtons.Middle, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_RBUTTONDOWN:
                        buttons |= MouseButtons.Right;
                        if (MouseDown != null)
                            MouseDown(this, new MouseEventArgs(MouseButtons.Right, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_RBUTTONUP:
                        buttons &= ~MouseButtons.Right;
                        if (MouseUp != null)
                            MouseUp(this, new MouseEventArgs(MouseButtons.Right, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_MOUSEMOVE:
                        if (MouseMove != null)
                            MouseMove(this, new MouseEventArgs(buttons, 0, hookStruct.pt.X, hookStruct.pt.Y, 0));
                        break;
                    case WM_MOUSEWHEEL:
                        if (MouseWheel != null)
                            MouseWheel(this, new MouseEventArgs(buttons, 0, hookStruct.pt.X, hookStruct.pt.Y, BitConverter.ToInt16(BitConverter.GetBytes(hookStruct.mouseData), 2)));
                        break;
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
