using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public sealed class LinearGradientBrush : Brush
    {
        GradientStop[] currentGradientStops;
        GradientStop[] newGradientStops;

        Vector2 currentStartPoint;
        Vector2 newStartPoint;

        Vector2 currentEndPoint;
        Vector2 newEndPoint;

        public GradientStop[] GradientStops
        {
            get { return currentGradientStops; }
            set
            {
                if (value.Length > Brush.MaximumGradientColorCount)
                    throw new ArgumentOutOfRangeException("value");

                newGradientStops = value;
            }
        }

        public Vector2 StartPoint
        {
            get { return currentStartPoint; }
            set { newStartPoint = value; }
        }

        public Vector2 EndPoint
        {
            get { return currentEndPoint; }
            set { newEndPoint = value; }
        }

        internal override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 2;
            buffer.ColorCount = GradientStops.Length;

            fixed (float* colors = buffer.GradientColors)
            {
                fixed (float* positions = buffer.GradientPositions)
                {
                    for (int i = 0; i < GradientStops.Length; i++)
                    {
                        colors[4 * i + 0] = GradientStops[i].Color.R;
                        colors[4 * i + 1] = GradientStops[i].Color.G;
                        colors[4 * i + 2] = GradientStops[i].Color.B;
                        colors[4 * i + 3] = GradientStops[i].Color.A;

                        positions[i * 4] = GradientStops[i].Position;
                    }
                }
            }

            buffer.Point1 = Transform.ApplyTo(StartPoint).ToSharpDXVector();
            buffer.Point2 = Transform.ApplyTo(EndPoint).ToSharpDXVector();
        }

        protected override void ApplyChanges()
        {
            base.ApplyChanges();

            currentGradientStops = newGradientStops;
            currentStartPoint = newStartPoint;
            currentEndPoint = newEndPoint;
        }

        public LinearGradientBrush()
        { }

        public LinearGradientBrush(GradientStop[] gradientStops, Vector2 startPoint, Vector2 endPoint)
        {
            if (gradientStops.Length > Brush.MaximumGradientColorCount)
                throw new ArgumentOutOfRangeException("gradientStops");

            currentGradientStops = gradientStops;
            newGradientStops = gradientStops;
            currentStartPoint = startPoint;
            newStartPoint = startPoint;
            currentEndPoint = endPoint;
            newEndPoint = endPoint;
        }
    }
}
