using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUtils.Math
{
    [Serializable]
    public struct Polygon : IEnumerable<Vector2>
    {
        /// <summary>
        /// Cretes a polygon out of some given points.
        /// </summary>
        /// <seealso cref="GameUtils.Math.Vector2"/>
        public Polygon(IEnumerable<Vector2> points)
            : this()
        {
            Points = points.ToArray();
        }

        /// <summary>
        /// Cretes a polygon out of some given points.
        /// </summary>
        /// <seealso cref="GameUtils.Math.Vector2"/>
        public Polygon(params Vector2[] points)
            : this()
        {
            Points = points;
        }

        /// <summary>
        /// Returns the point at a given index of this polygon.
        /// </summary>
        /// <seealso cref="GameUtils.Math.Vector2"/>
        public Vector2 this[int index]
        {
            get { return Points[index]; }
            set { Points[index] = value; }
        }

        /// <summary>
        /// All points of this Polygon.
        /// </summary>
        /// <seealso cref="GameUtils.Math.Vector2"/>
        public Vector2[] Points { get; set; }

        /// <summary>
        /// Indicates whether the given points form an actual polygon, which is when there are more than two points.
        /// </summary>
        public bool IsValid
        {
            get { return Points.Length >= 3; }
        }

        /// <summary>
        /// The center of this polygon.
        /// </summary>
        /// <remarks>This property recalculates the center every time called, so you should save it if possible.</remarks>
        public Vector2 Center
        {
            get
            {
                if (!this.IsValid)
                    throw new InvalidOperationException();

                float x = 0, y = 0;
                float vp = 0;
                for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i, i++)
                {
                    Vector2 p1 = Points[j];
                    Vector2 p2 = Points[i];
                    float v = Vector2.VectorProduct(p1, p2);
                    x += (p1 + p2).X * v;
                    y += (p1 + p2).Y * v;
                    vp += v;
                }
                vp *= 3;
                return new Vector2(x / vp, y / vp);
            }
        }

        /// <summary>
        /// Creates a polygon out of a given rectangle.
        /// </summary>
        public static Polygon FromRectangle(Rectangle rect)
        {
            return new Polygon(rect.Location, new Vector2(rect.X + rect.Width, rect.Y), rect.Location + rect.Size, new Vector2(rect.X, rect.Y + rect.Height));
        }

        /// <summary>
        /// Creates a polygon out of a given ellipse.
        /// Because an ellipse can never be represented exact by a polgon, sou have to specify the detail of the polgon.
        /// </summary>
        /// <param name="detail">The detail of the final polygon. Smaller values mean greater detail.</param>
        public static Polygon FromEllipse(Ellipse ellipse, int detail)
        {
            float maxRadius = System.Math.Max(ellipse.RadiusX, ellipse.RadiusY);
            float circumference = 2 * MathHelper.Pi * maxRadius;
            int pointCount = (int)(circumference / detail);
            float angleStep = 2 * MathHelper.Pi / pointCount;
            var points = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float angle = i * angleStep;
                float x = MathHelper.Cos(angle) * ellipse.RadiusX + ellipse.Center.X;
                float y = MathHelper.Sin(angle) * ellipse.RadiusY + ellipse.Center.Y;
                points[i] = new Vector2(x, y);
            }
            return new Polygon(points);
        }

        /// <summary>
        /// Checks if this polgon contains a given point.
        /// </summary>
        public bool Contains(Vector2 point)
        {
            if (!this.IsValid)
                throw new InvalidOperationException();

            int alpha = 0;
            Vector2 v1 = Points[Points.Length - 1];

            int q1;
            if (v1.Y <= point.Y)
            {
                if (v1.X <= point.X)
                    q1 = 0;
                else
                    q1 = 1;
            }
            else if (v1.X <= point.X)
            {
                q1 = 3;
            }
            else
            {
                q1 = 2;
            }

            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 v2 = Points[i];

                int q2;
                if (v2.Y <= point.Y)
                {
                    if (v2.X <= point.X)
                        q2 = 0;
                    else
                        q2 = 1;
                }
                else if (v2.X <= point.X)
                {
                    q2 = 3;
                }
                else
                {
                    q2 = 2;
                }

                switch ((q2 - q1) & 3)
                {
                    case 0:
                        break;
                    case 1:
                        alpha += 1;
                        break;
                    case 3:
                        alpha -= 1;
                        break;
                    default:
                        float zx = ((v2.X - v1.X) * (point.Y - v1.Y) / (v2.Y - v1.Y)) + v1.X;
                        if (point.X - zx <= MathHelper.Epsilon)
                            return true;
                        if ((point.X > zx) == (v2.Y > v1.Y))
                            alpha -= 2;
                        else
                            alpha += 2;
                        break;
                }

                v1 = v2;
                q1 = q2;
            }

            return System.Math.Abs(alpha) == 4;
        }

        private void ProjectTo(Vector2 axis, out float min, out float max)
        {
            min = Vector2.DotProduct(axis, Points[0]);
            max = min;

            for (int i = 1; i < Points.Length; i++)
            {
                float d = Vector2.DotProduct(axis, Points[i]);

                if (d < min)
                    min = d;
                if (d > max)
                    max = d;
            }
        }

        private bool HasSeparatingAxisTo(Polygon other, ref float minOverlap, ref Vector2 axis)
        {
            int prev = Points.Length - 1;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 edge = Points[i] - Points[prev];

                Vector2 v = edge.CrossProduct().Normalize();

                float aMin, aMax, bMin, bMax;
                this.ProjectTo(v, out aMin, out aMax);
                other.ProjectTo(v, out bMin, out bMax);

                if ((aMax < bMin) || (bMax < aMin))
                    return true;

                float overlapping = aMax < bMax ? aMax - bMin : bMax - aMin;
                if (overlapping < minOverlap)
                {
                    minOverlap = overlapping;
                    axis = v;
                }

                prev = i;
            }

            return false;
        }

        /// <summary>
        /// Checks if this polygon intersects with another polgon.
        /// </summary>
        /// <param name="minimumTranslationVector">Returns a vector with which the polgon has to be translated at minimum to not collide with the second polgon.</param>
        public bool IntersectsWith(Polygon other, out Vector2 minimumTranslationVector)
        {
            if (!this.IsValid || !other.IsValid)
                throw new InvalidOperationException();

            minimumTranslationVector = default(Vector2);
            float minOverlap = float.MaxValue;

            if (this.HasSeparatingAxisTo(other, ref minOverlap, ref minimumTranslationVector))
                return false;

            if (other.HasSeparatingAxisTo(this, ref minOverlap, ref minimumTranslationVector))
                return false;

            Vector2 d = this.Center - other.Center;
            if (Vector2.DotProduct(d, minimumTranslationVector) > 0)
                minimumTranslationVector = -minimumTranslationVector;
            minimumTranslationVector *= minOverlap;

            return true;
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return (Points as IEnumerable<Vector2>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Points.GetEnumerator();
        }
    }
}
