using System;
using GameUtils.Input.DefaultDevices;
using GameUtils.Math;

namespace GameUtils.UI
{
    public class MouseEventArgs : EventArgs
    {
        public MouseButtons Buttons { get; private set; }

        public float X { get; private set; }

        public float Y { get; private set; }

        public Vector2 Location { get; private set; }

        public int Delta { get; private set; }

        public MouseEventArgs(MouseButtons buttons, float x, float y, int delta)
        {
            Buttons = buttons;
            X = x;
            Y = y;
            Location = new Vector2(x, y);
            Delta = delta;
        }

        public MouseEventArgs(MouseButtons buttons, Vector2 location, int delta)
        {
            Buttons = buttons;
            X = location.X;
            Y = location.Y;
            Location = location;
            Delta = delta;
        }
    }
}
