using System;
using System.Collections.Generic;

namespace FontConverter
{
    sealed class Triangle
    {
        public ControlPoint A { get; private set; }
        public ControlPoint B { get; private set; }
        public ControlPoint C { get; private set; }

        public LinkedListNode<ControlPoint> BNode { get; set; }

        public bool IsInside { get; set; }

        public LinkedList<ControlPoint> Contour
        {
            get { return BNode.List; }
        }

        public Triangle(ControlPoint a, ControlPoint b, ControlPoint c)
        {
            A = a;
            B = b;
            C = c;
        }

        static bool Contains(List<ControlPoint> polygon, ControlPoint point)
        {
            int alpha = 0;
            ControlPoint v1 = polygon[polygon.Count - 1];

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

            for (int i = 0; i < polygon.Count; i++)
            {
                ControlPoint v2 = polygon[i];

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
                        double zx = ((v2.X - v1.X) * (point.Y - v1.Y) / (v2.Y - v1.Y)) + v1.X;
                        if (point.X - zx == 0)
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

            return Math.Abs(alpha) == 4;
        }

        public bool IntersectsWith(Triangle other)
        {
            var poly1 = new List<ControlPoint>() { this.A, this.B, this.C };
            var poly2 = new List<ControlPoint>() { other.A, other.B, other.C };

            if (Contains(poly2, this.A)) return true;
            if (Contains(poly2, this.B)) return true;
            if (Contains(poly2, this.C)) return true;

            if (Contains(poly1, other.A)) return true;
            if (Contains(poly1, other.B)) return true;
            if (Contains(poly1, other.C)) return true;

            return false;
        }

        public double Area()
        {
            return Math.Abs((B.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (B.Y - A.Y));
        }

        public Triangle[] Split(out ControlPoint newOnPoint)
        {
            var a1 = (B + A) / 2;
            var a2 = (C + A) / 2;
            var cb = (a1 + a2) / 2;

            newOnPoint = cb;

            var result = new Triangle[2];
            result[0] = new Triangle(a1, B, cb);
            result[1] = new Triangle(a2, cb, C);
            return result;
        }
    }
}
