namespace GameUtils.Graphics
{
    public sealed class SolidColorBrush : Brush
    {
        public override bool IsAsync => false;

        public override bool IsReady => true;

        public Color4 Color { get; set; }

        public SolidColorBrush(Color4 color)
        {
            Color = color;
        }

        protected override unsafe void FillBuffer(ref BrushBuffer buffer)
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
    }
}
