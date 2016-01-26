using System;
using GameUtils.Input.DefaultDevices;

namespace GameUtils.UI
{
    public class KeyboardEventArgs : EventArgs
    {
        public Key Key { get; private set; }

        public bool ControlKeyDown { get; private set; }

        public bool AltKeyDown { get; private set; }

        public bool ShiftKeyDown { get; private set; }

        public bool WinKeyDown { get; private set; }

        public KeyboardEventArgs(Key key, bool controlKeyDown, bool altKeyDown, bool shiftKeyDown, bool winKeyDown)
        {
            Key = key;
            ControlKeyDown = controlKeyDown;
            AltKeyDown = altKeyDown;
            ShiftKeyDown = shiftKeyDown;
            WinKeyDown = winKeyDown;
        }
    }
}
