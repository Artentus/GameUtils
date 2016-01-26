using System;
using GameUtils.Graphics;
using GameUtils.Math;

namespace GameUtils.UI
{
    public class PaintEventArgs : EventArgs
    {
        public Renderer Renderer { get; private set; }

        public Rectangle Bounds { get; private set; }

        public PaintEventArgs(Renderer renderer, Rectangle bounds)
        {
            Renderer = renderer;
            Bounds = bounds;
        }
    }
}
