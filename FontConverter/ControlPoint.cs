using System;

namespace FontConverter
{
    public struct ControlPoint :IEquatable<ControlPoint>
    {
        public ControlPoint(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }

        public ControlPoint(double x, double y, bool isOnCurve)
            : this()
        {
            X = x;
            Y = y;
            IsOnCurve = isOnCurve;
        }

        public double X;

        public double Y;

        public bool IsOnCurve;

        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y); }
        }

        public static ControlPoint Add(ControlPoint left, ControlPoint right)
        {
            return new ControlPoint(left.X + right.X, left.Y + right.Y, false);
        }

        public static ControlPoint Subtract(ControlPoint left, ControlPoint right)
        {
            return new ControlPoint(left.X - right.X, left.Y - right.Y, false);
        }

        public static ControlPoint Negate(ControlPoint value)
        {
            return new ControlPoint(-value.X, -value.Y, false);
        }

        public static ControlPoint Multiply(ControlPoint value, double scalar)
        {
            return new ControlPoint(value.X * scalar, value.Y * scalar, false);
        }

        public static ControlPoint Divide(ControlPoint value, double scalar)
        {
            return new ControlPoint(value.X / scalar, value.Y / scalar, false);
        }

        public static double VectorProduct(ControlPoint left, ControlPoint right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        public override bool Equals(object obj)
        {
            if (obj is ControlPoint)
                return this.Equals((ControlPoint)obj);
            else
                return false;
        }

        public bool Equals(ControlPoint other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Point {{ X={0}, Y={1} }}", X, Y);
        }

        public static bool operator ==(ControlPoint left, ControlPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ControlPoint left, ControlPoint right)
        {
            return !left.Equals(right);
        }

        public static ControlPoint operator +(ControlPoint left, ControlPoint right)
        {
            return ControlPoint.Add(left, right);
        }

        public static ControlPoint operator +(ControlPoint value)
        {
            return value;
        }

        public static ControlPoint operator -(ControlPoint left, ControlPoint right)
        {
            return ControlPoint.Subtract(left, right);
        }

        public static ControlPoint operator -(ControlPoint value)
        {
            return ControlPoint.Negate(value);
        }

        public static ControlPoint operator *(ControlPoint value, double scalar)
        {
            return ControlPoint.Multiply(value, scalar);
        }

        public static ControlPoint operator *(double scalar, ControlPoint value)
        {
            return ControlPoint.Multiply(value, scalar);
        }

        public static ControlPoint operator /(ControlPoint value, double scalar)
        {
            return ControlPoint.Divide(value, scalar);
        }
    }
}
