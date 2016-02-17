using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public sealed class RadialGradientBrush : Brush
    {
        public override bool IsAsync => false;

        public override bool IsReady => true;

        GradientStop[] gradientStops;
        float radiusX;
        float radiusY;

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

        public Vector2 Center { get; set; }

        public float RadiusX
        {
            get { return radiusX; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                radiusX = value;
            }
        }

        public float RadiusY
        {
            get { return radiusY; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                radiusY = value;
            }
        }

        public float Angle { get; set; }

        public Vector2? FocusPoint { get; set; }

        public RadialGradientBrush()
        { }

        public RadialGradientBrush(GradientStop[] gradientStops, Vector2 center, float radiusX, float radiusY, float angle = 0f, Vector2? focusPoint = null)
        {
            if (gradientStops.Length > MaximumGradientColorCount)
                throw new ArgumentOutOfRangeException(nameof(gradientStops));
            if (radiusX < 0) throw new ArgumentOutOfRangeException(nameof(radiusX));
            if (radiusY < 0) throw new ArgumentOutOfRangeException(nameof(radiusY));

            this.gradientStops = gradientStops;
            Center = center;
            this.radiusX = radiusX;
            this.radiusY = radiusY;
            Angle = angle;
            FocusPoint = focusPoint;
        }

        protected override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 3;

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

            Matrix2x3 transformFromCircle = Matrix2x3.Translation(Center) * Matrix2x3.Rotation(Angle) * Matrix2x3.Scaling(RadiusX, RadiusY);
            Matrix2x3 transformToCircle = (Transform * transformFromCircle).Invert();
            fixed (float* matrix = buffer.Matrix)
            {
                for (int i = 0; i < 6; i++)
                    matrix[i * 4] = transformToCircle[i / 3, i % 3];
            }

            if (FocusPoint.HasValue)
            {
                Vector2 transformedFocusPoint = transformToCircle.ApplyTo(FocusPoint.Value);
                buffer.Point1 = new SharpDX.Vector2(transformedFocusPoint.X, transformedFocusPoint.Y);
                buffer.Point2 = new SharpDX.Vector2(1, 0);
            }
        }
    }
}
