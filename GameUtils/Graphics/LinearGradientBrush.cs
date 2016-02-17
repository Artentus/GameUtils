using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public sealed class LinearGradientBrush : Brush
    {
        public override bool IsAsync => false;

        public override bool IsReady => true;

        GradientStop[] gradientStops;

        public GradientStop[] GradientStops
        {
            get { return gradientStops; }
            set
            {
                if (value.Length > MaximumGradientColorCount)
                    throw new ArgumentOutOfRangeException(nameof(value));

                gradientStops = value;
            }
        }

        public Vector2 StartPoint { get; set; }

        public Vector2 EndPoint { get; set; }

        public LinearGradientBrush()
        { }

        public LinearGradientBrush(GradientStop[] gradientStops, Vector2 startPoint, Vector2 endPoint)
        {
            if (gradientStops.Length > MaximumGradientColorCount)
                throw new ArgumentOutOfRangeException(nameof(gradientStops));

            this.gradientStops = gradientStops;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        protected override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 2;

            if (gradientStops != null && gradientStops.Length > 0)
            {
                buffer.ColorCount = gradientStops.Length;

                fixed (float* colors = buffer.GradientColors, positions = buffer.GradientPositions)
                {
                    for (int i = 0; i < gradientStops.Length; i++)
                    {
                        colors[4 * i + 0] = gradientStops[i].Color.R;
                        colors[4 * i + 1] = gradientStops[i].Color.G;
                        colors[4 * i + 2] = gradientStops[i].Color.B;
                        colors[4 * i + 3] = gradientStops[i].Color.A;

                        positions[i * 4] = gradientStops[i].Position;
                    }
                }
            }

            buffer.Point1 = Transform.ApplyTo(StartPoint).ToSharpDXVector();
            buffer.Point2 = Transform.ApplyTo(EndPoint).ToSharpDXVector();
        }
    }
}
