using System;

namespace GameUtils.Math
{
    /// <summary>
    /// Represent a pair of coordinates and offers basic vector maths.
    /// </summary>
    [Serializable]
    public struct Vector2 : IEquatable<Vector2>
    {
        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector2 Zero;

        /// <summary>
        /// The X unit vector.
        /// </summary>
        public static readonly Vector2 UnitX;

        /// <summary>
        /// The Y unit vector.
        /// </summary>
        public static readonly Vector2 UnitY;

        static Vector2()
        {
            Zero = default(Vector2);
            UnitX = new Vector2(1, 0);
            UnitY = new Vector2(0, 1);
        }

        /// <summary>
        /// Creates a new vector.
        /// </summary>
        public Vector2(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The X coordinate of this vector.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y coordinate of this vector.
        /// </summary>
        public float Y;

        /// <summary>
        /// The length of this vector.
        /// </summary>
        public float Length
        {
            get { return MathHelper.Sqrt(X * X + Y * Y); }
        }

        /// <summary>
        /// The length of this vector squared.
        /// </summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public Vector2 Normalize()
        {
            return Vector2.Divide(this, this.Length);
        }

        /// <summary>
        /// Returns the vector cross product.
        /// </summary>
        public Vector2 CrossProduct()
        {
            return new Vector2(-Y, X);
        }

        /// <summary>
        /// Returns the vector dot product.
        /// </summary>
        public static float DotProduct(Vector2 left, Vector2 right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        /// <summary>
        /// Returns the vector product.
        /// </summary>
        public static float VectorProduct(Vector2 left, Vector2 right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        public static Vector2 Add(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Subtracts a vector from another.
        /// </summary>
        public static Vector2 Subtract(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        public static Vector2 Negate(Vector2 value)
        {
            return new Vector2(-value.X, -value.Y);
        }

        /// <summary>
        /// Multiplys a vectors by a scalar.
        /// </summary>
        public static Vector2 Multiply(Vector2 value, float scalar)
        {
            return new Vector2(value.X * scalar, value.Y * scalar);
        }

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        public static Vector2 Divide(Vector2 value, float scalar)
        {
            return new Vector2(value.X / scalar, value.Y / scalar);
        }

        /// <summary>
        /// Gets the distance between two vectors.
        /// </summary>
        public static float Distance(Vector2 first, Vector2 second)
        {
            return Vector2.Subtract(first, second).Length;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
                return this.Equals((Vector2)obj);
            else
                return false;
        }

        public bool Equals(Vector2 other)
        {
            return System.Math.Abs(this.X - other.X) <= MathHelper.Epsilon && System.Math.Abs(this.Y - other.Y) <= MathHelper.Epsilon;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Vector2 {{ X={0}, Y={1} }}", X, Y);
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return Vector2.Add(left, right);
        }

        /// <summary>
        /// Returns the vector.
        /// </summary>
        public static Vector2 operator +(Vector2 value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts a vector by another.
        /// </summary>
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return Vector2.Subtract(left, right);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        public static Vector2 operator -(Vector2 value)
        {
            return Vector2.Negate(value);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        public static Vector2 operator *(Vector2 value, float scalar)
        {
            return Vector2.Multiply(value, scalar);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        public static Vector2 operator *(float scalar, Vector2 value)
        {
            return Vector2.Multiply(value, scalar);
        }

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        public static Vector2 operator /(Vector2 value, float scalar)
        {
            return Vector2.Divide(value, scalar);
        }
    }
}
