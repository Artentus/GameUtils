using System;
using GameUtils.Math;
using Vector2 = GameUtils.Math.Vector2;

namespace GameUtils.Graphics
{
    public sealed class RadialGradientBrush : Brush
    {
        GradientStop[] currentGradientStops;
        GradientStop[] newGradientStops;

        Vector2 currentCenter;
        Vector2 newCenter;

        float currentRadiusX;
        float newRadiusX;

        float currentRadiusY;
        float newRadiusY;

        float currentAngle;
        float newAngle;

        Vector2? currentFocusPoint;
        Vector2? newFocusPoint;

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

        public Vector2 Center
        {
            get { return currentCenter; }
            set { newCenter = value; }
        }

        public float RadiusX
        {
            get { return currentRadiusX; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                newRadiusX = value;
            }
        }

        public float RadiusY
        {
            get { return currentRadiusY; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                newRadiusY = value;
            }
        }

        public float Angle
        {
            get { return currentAngle; }
            set { newAngle = value; }
        }

        public Vector2? FocusPoint
        {
            get { return currentFocusPoint; }
            set { newFocusPoint = value; }
        }
    
        internal override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 3;
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

        protected override void ApplyChanges()
        {
            base.ApplyChanges();

            currentGradientStops = newGradientStops;
            currentCenter = newCenter;
            currentRadiusX = newRadiusX;
            currentRadiusY = newRadiusY;
            currentAngle = newAngle;
            currentFocusPoint = newFocusPoint;
        }

        public RadialGradientBrush()
        { }

        public RadialGradientBrush(GradientStop[] gradientStops, Vector2 center, float radiusX, float radiusY, float angle = 0f, Vector2? focusPoint = null)
        {
            if (gradientStops.Length > Brush.MaximumGradientColorCount)
                throw new ArgumentOutOfRangeException("gradientStops");
            if (radiusX < 0) throw new ArgumentOutOfRangeException("radiusX");
            if (radiusY < 0) throw new ArgumentOutOfRangeException("radiusY");

            currentGradientStops = gradientStops;
            newGradientStops = gradientStops;
            currentCenter = center;
            newCenter = center;
            currentRadiusX = radiusX;
            newRadiusX = radiusX;
            currentRadiusY = radiusY;
            newRadiusY = radiusY;
            currentAngle = angle;
            newAngle = angle;
            currentFocusPoint = focusPoint;
            newFocusPoint = focusPoint;
        }
    }
}
