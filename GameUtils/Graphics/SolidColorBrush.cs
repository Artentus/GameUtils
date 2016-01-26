namespace GameUtils.Graphics
{
    public sealed class SolidColorBrush : Brush
    {
        Color4 currentColor;
        Color4 newColor;

        public Color4 Color
        {
            get { return currentColor; }
            set { newColor = value; }
        }

        internal override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 1;
            fixed (float* colors = buffer.GradientColors)
            {
                colors[0] = Color.R;
                colors[1] = Color.G;
                colors[2] = Color.B;
                colors[3] = Color.A;
            }
        }

        protected override void ApplyChanges()
        {
            base.ApplyChanges();

            currentColor = newColor;
        }

        public SolidColorBrush(Color4 color)
        {
            currentColor = color;
            newColor = color;
        }
    }
}
