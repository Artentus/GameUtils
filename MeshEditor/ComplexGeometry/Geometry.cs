using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Stores complex 2D geometry data.
    /// </summary>
    sealed class Geometry
    {
        public readonly BindingList<GeometryFigure> Figures;

        public Geometry()
        {
            Figures = new BindingList<GeometryFigure>();
        }

        private static List<GeometryFigure> CopyFigureList(List<GeometryFigure> list)
        {
            var result = new List<GeometryFigure>(list.Count);
            foreach (GeometryFigure figure in list)
                result.Add(figure.Clone());
            return result;
        }

        #region Line
        private void AddLinesInner(IEnumerable<Vector2> points)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            if (currentFigure == null || currentFigure.IsClosed)
                throw new InvalidOperationException("There is no open figure.");

            currentFigure.Points.AddRange(points);
            lastPoint = currentFigure.Points.Last();
        }

        /// <summary>
        /// Adds straight lines to the current figure.
        /// </summary>
        /// <param name="points">The points getting connected by the lines.</param>
        public void AddLines(IEnumerable<Vector2> points)
        {
            AddLinesInner(points);
        }

        /// <summary>
        /// Adds straight lines to the current figure.
        /// </summary>
        /// <param name="points">The points getting connected by the lines.</param>
        public void AddLines(params Vector2[] points)
        {
            AddLinesInner(points);
        }

        /// <summary>
        /// Adds a straight line to the current figure.
        /// </summary>
        /// <param name="point">The end point of the line segment.</param>
        public void AddLine(Vector2 point)
        {
            if (currentFigure == null || currentFigure.IsClosed)
                throw new InvalidOperationException("There is no open figure.");

            currentFigure.Points.Add(point);
            lastPoint = point;
        }
        #endregion

        #region Bezier
        private bool IsFlat(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p1234, out List<Vector2> result)
        {
            const float twoPi = MathHelper.Pi * 2;

            float collinearityEpsilon = MathHelper.Epsilon;
            float distanceTolerance = MathHelper.Sqrt(Precision);
            float angleTolerance = MathHelper.Pi / 10f * Precision;
            //float cuspLimit = MathHelper.Pi / 15f * Precision;

            result = new List<Vector2>(2);

            Vector2 delta = p4 - p1;
            float d2 = System.Math.Abs(((p2.X - p4.X) * delta.Y - (p2.Y - p4.Y) * delta.X));
            float d3 = System.Math.Abs(((p3.X - p4.X) * delta.Y - (p3.Y - p4.Y) * delta.X));

            if ((d2 > collinearityEpsilon) && (d3 > collinearityEpsilon))
            {
                float d = d2 + d3;
                if ((d * d) <= (distanceTolerance * Vector2.DotProduct(delta, delta)))
                {
                    float a23 = MathHelper.Atan2(p3.Y - p2.Y, p3.X - p2.X);
                    float da1 = System.Math.Abs(a23 - MathHelper.Atan2(p2.Y - p1.Y, p2.X - p1.X));
                    float da2 = System.Math.Abs(MathHelper.Atan2(p4.Y - p3.Y, p4.X - p3.X) - a23);
                    if (da1 >= MathHelper.Pi) da1 = twoPi - da1;
                    if (da2 >= MathHelper.Pi) da2 = twoPi - da2;

                    if ((da1 + da2) < angleTolerance)
                    {
                        result.Add(p1234);
                        return true;
                    }

                    //if (da1 > cuspLimit)
                    //{
                    //    result.Add(p2);
                    //    return true;
                    //}
                    //if (da2 > cuspLimit)
                    //{
                    //    result.Add(p3);
                    //    return true;
                    //}
                }
            }
            else if (d2 > collinearityEpsilon)
            {
                if ((d2 * d2) <= (distanceTolerance * Vector2.DotProduct(delta, delta)))
                {
                    float da = System.Math.Abs(MathHelper.Atan2(p3.Y - p2.Y, p3.X - p2.X) - MathHelper.Atan2(p2.Y - p1.Y, p2.X - p1.X));
                    if (da >= MathHelper.Pi) da = twoPi - da;

                    if (da < angleTolerance)
                    {
                        result.Add(p2);
                        result.Add(p3);
                        return true;
                    }

                    //if (da > cuspLimit)
                    //{
                    //    result.Add(p2);
                    //    return true;
                    //}
                }
            }
            else if (d3 > collinearityEpsilon)
            {
                if ((d3 * d3) <= (distanceTolerance * Vector2.DotProduct(delta, delta)))
                {
                    float da = System.Math.Abs(MathHelper.Atan2(p4.Y - p3.Y, p4.X - p3.X) - MathHelper.Atan2(p3.Y - p2.Y, p3.X - p2.X));
                    if (da >= MathHelper.Pi) da = twoPi - da;

                    if (da < angleTolerance)
                    {
                        result.Add(p2);
                        result.Add(p3);
                        return true;
                    }

                    //if (da > cuspLimit)
                    //{
                    //    result.Add(p3);
                    //    return true;
                    //}
                }
            }
            else
            {
                delta = p1234 - (p1 + p4) * 0.5f;
                if (Vector2.DotProduct(delta, delta) <= distanceTolerance)
                {
                    result.Add(p1234);
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Vector2> GetCubicBezierPoints(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, bool firstStep)
        {
            Vector2 p12 = (p1 + p2) * 0.5f;
            Vector2 p23 = (p2 + p3) * 0.5f;
            Vector2 p34 = (p3 + p4) * 0.5f;

            Vector2 p123 = (p12 + p23) * 0.5f;
            Vector2 p234 = (p23 + p34) * 0.5f;

            Vector2 p1234 = (p123 + p234) * 0.5f;

            List<Vector2> result;
            if (!firstStep && IsFlat(p1, p2, p3, p4, p1234, out result))
                return result;

            return GetCubicBezierPoints(p1, p12, p123, p1234, false).Concat(GetCubicBezierPoints(p1234, p234, p34, p4, false));
        }

        private void AddBeziersInner(Vector2[] points)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            var linePoints = new List<Vector2>();

            for (int i = 0; i < points.Length - 1; i += 3)
            {
                if (i > 0) linePoints.Add(points[i - 1]);
                linePoints.AddRange(GetCubicBezierPoints(i - 1 >= 0 ? points[i - 1] : lastPoint, points[i], points[i + 1], points[i + 2], true));
            }
            linePoints.Add(points.Last());

            AddLinesInner(linePoints);
        }

        /// <summary>
        /// Adds bezier curves to the current figure.
        /// </summary>
        /// <param name="controlPoints">The control points representing the curves.</param>
        public void AddBeziers(IEnumerable<Vector2> controlPoints)
        {
            AddBeziersInner(controlPoints.ToArray());
        }

        /// <summary>
        /// Adds bezier curves to the current figure.
        /// </summary>
        /// <param name="controlPoints">The control points representing the curves.</param>
        public void AddBeziers(params Vector2[] controlPoints)
        {
            AddBeziersInner(controlPoints);
        }

        /// <summary>
        /// Adds a bezier curve to the current figure.
        /// </summary>
        /// <param name="controlPoint1">The first control point of the curve.</param>
        /// <param name="controlPoint2">The second control point of the curve.</param>
        /// <param name="endPoint">The third control point or the end point of the curve.</param>
        public void AddBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            AddBeziers(controlPoint1, controlPoint2, endPoint);
        }
        #endregion

        #region Curve
        /// <summary>
        /// Adds a cardinal spline curve to the current figure.
        /// </summary>
        /// <param name="points">The points traversed by the curve.</param>
        /// <param name="tension">Optional. Defines how tight the path curves around the points.
        /// The default value of 0 means normal curvature, negative values let the curvature become wider and a value of 1 produces straight lines.</param>
        public void AddCurve(Vector2[] points, float tension = 0f)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            float s = (1 - tension) / 2;
            var allPoints = new Vector2[points.Length + 1];
            allPoints[0] = lastPoint;
            Array.Copy(points, 0, allPoints, 1, points.Length);

            var tangents = new Vector2[allPoints.Length];
            for (int i = 1; i < allPoints.Length - 1; i++)
                tangents[i] = s * (allPoints[i + 1] - allPoints[i - 1]) / 3;
            tangents[0] = s * (allPoints[1] - allPoints[0]) / 3;
            tangents[allPoints.Length - 1] = s * (allPoints[allPoints.Length - 1] - allPoints[allPoints.Length - 2]) / 3;

            var controlPoints = new Vector2[(allPoints.Length - 1) * 3];
            //var segments = new BezierSegment[allPoints.Length - 1];
            for (int i = 0; i < allPoints.Length - 1; i++)
            {
                Vector2 control1 = allPoints[i] + tangents[i];
                Vector2 control2 = allPoints[i + 1] - tangents[i + 1];

                controlPoints[i * 3] = control1;
                controlPoints[i * 3 + 1] = control2;
                controlPoints[i * 3 + 2] = allPoints[i + 1];
            }

            this.AddBeziersInner(controlPoints);
        }
        #endregion

        #region Rectangle
        /// <summary>
        /// Adds a rectangle to this geometry.
        /// </summary>
        public void AddRectangle(Rectangle rectangle, CombineMode combineMode)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            if (currentFigure != null && !currentFigure.IsClosed)
                throw new InvalidOperationException("All open figures must be closed to add a rectangle.");

            OpenFigure(rectangle.Location, FillMode.Alternate);
            AddLines(new[]
            {
                new Vector2(rectangle.X + rectangle.Width, rectangle.Y),
                rectangle.Location + rectangle.Size,
                new Vector2(rectangle.X, rectangle.Y + rectangle.Height)
            });
            CloseFigure(FigureEnd.Closed, combineMode);
        }

        /// <summary>
        /// Adds a rectangle to this geometry.
        /// </summary>
        public void AddRectangle(Rectangle rectangle)
        {
            AddRectangle(rectangle, CombineMode);
        }
        #endregion

        #region Polygon
        /// <summary>
        /// Adds a polygon to this geometry.
        /// </summary>
        public void AddPolygon(Polygon polygon, CombineMode combineMode)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            if (currentFigure != null && !currentFigure.IsClosed)
                throw new InvalidOperationException("All open figures must be closed to add a polygon.");

            if (!polygon.IsValid)
                throw new ArgumentException("The polygon was not valid.", "polygon");

            OpenFigure(polygon.Points[0]);
            AddLines(polygon.Points.Skip(1));
            CloseFigure(FigureEnd.Closed, combineMode);
        }

        /// <summary>
        /// Adds a polygon to this geometry.
        /// </summary>
        public void AddPolygon(Polygon polygon)
        {
            AddPolygon(polygon, CombineMode);
        }
        #endregion

        #region Arc
        static Vector2 PointOnCircle(float radius, float angle)
        {
            return new Vector2(MathHelper.Cos(angle) * radius, MathHelper.Sin(angle) * radius);
        }

        static Vector2[] GetApproximatingBeziers(Vector2 center, float radius, float eta, float alpha, SweepDirection sweepDirection)
        {
            const float twoPi = 2 * MathHelper.Pi;
            const float piOverTwo = MathHelper.Pi / 2;

            if (sweepDirection == SweepDirection.Counterclockwise) alpha = -alpha;
            if (alpha < 0)
            {
                alpha = -alpha;
                eta -= alpha;
            }
            eta = eta % twoPi;

            var result = new List<Vector2>();

            float startAngle = eta;
            while (alpha > 0)
            {
                float a = MathHelper.Min(alpha, piOverTwo);
                float l = 4 * MathHelper.Tan(a / 4) / 3 * radius;
                Vector2 p0 = PointOnCircle(radius, startAngle);
                Vector2 p1 = p0 + p0.Normalize().CrossProduct() * l;
                Vector2 p3 = PointOnCircle(radius, startAngle + a);
                Vector2 p2 = p3 + p3.Normalize().CrossProduct() * -l;
                p1 += center;
                p2 += center;
                p3 += center;
                result.Add(p1);
                result.Add(p2);
                result.Add(p3);

                alpha -= a;
                startAngle += a;
            }

            if (sweepDirection == SweepDirection.Counterclockwise)
            {
                result.Reverse();
                result.RemoveAt(0);
                result.Add(PointOnCircle(radius, eta) + center);
            }
            return result.ToArray();
        }

        public void AddArc(Vector2 endPoint, Vector2 center, SweepDirection sweepDirection)
        {
            if (Closed) throw new InvalidOperationException("The geometry can not be changed after is has been closed.");

            float radius = (endPoint - center).Length;

            if (System.Math.Abs(radius - (lastPoint - center).Length) > MathHelper.Epsilon)
                throw new ArgumentException("The center must have the same distance to the start and end point of the arc.", "center");

            float eta1 = MathHelper.Atan2(lastPoint.Y - center.Y, lastPoint.X - center.X);
            float eta2 = MathHelper.Atan2(endPoint.Y - center.Y, endPoint.X - center.X);
            float alpha = sweepDirection == SweepDirection.Clockwise ? eta2 - eta1 : MathHelper.Pi * 2 - (eta2 - eta1);

            Vector2[] beziers = GetApproximatingBeziers(center, radius, eta1, alpha, sweepDirection);
            AddBeziersInner(beziers);
        }
        #endregion

    }
}
