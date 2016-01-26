using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameUtils.Input.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    struct MSLLHOOKSTRUCT
    {
        public Point pt;
        public int mouseData;
        public int flags;
        public int time;
        public UIntPtr dwExtraInfo;
    }
}
