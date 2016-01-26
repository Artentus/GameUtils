using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameUtils.Graphics;

namespace GameUtils.Math
{
    /// <summary>
    /// A two-dimensional transformation matrix.
    /// </summary>
    public unsafe struct Matrix2x3
    {
        fixed float values[6];

        /// <summary>
        /// The identity matrix.
        /// </summary>
        public static readonly Matrix2x3 Identity;

        static Matrix2x3()
        {
            Matrix2x3 m = default(Matrix2x3);
            m.values[0] = 1.0f;
            m.values[4] = 1.0f;
            Identity = m;
        }

        /// <summary>
        /// Returns or sets the matrix element at the given Position.
        /// </summary>
        public float this[int x, int y]
        {
            get
            {
                if (x < 0 || x > 1 || y < 0 || y > 2)
                    throw new IndexOutOfRangeException();

                fixed (float* values = this.values)
                    return values[x * 3 + y];
            }
            set
            {
                if (x < 0 || x > 1 || y < 0 || y > 2)
                    throw new IndexOutOfRangeException();

                fixed (float* values = this.values)
                    values[x * 3 + y] = value;
            }
        }

        /// <summary>
        /// The translation in horizontal direction represented by this matrix.
        /// </summary>
        public float OffsetX
        {
            get { fixed (float* values = this.values) return values[2]; }
            set { fixed (float* values = this.values) values[2] = value; }
        }

        /// <summary>
        /// The translation in vertical direction represented by this matrix.
        /// </summary>
        public float OffsetY
        {
            get { fixed (float* values = this.values) return values[5]; }
            set { fixed (float* values = this.values) values[5] = value; }
        }

        /// <summary>
        /// The determinant of this matrix.
        /// </summary>
        public float Determinant
        {
            get
            {
                fixed (float* values = this.values)
                    return values[0] * values[4] - values[1] * values[3];
            }
        }

        /// <summary>
        /// Indicates weather this matrix is the identity matrix.
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                fixed (float* values = this.values)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int rem;
                        int div = System.Math.DivRem(i, 3, out rem);
                        if (System.Math.Abs(values[i] - (div == rem ? 1 : 0)) > MathHelper.Epsilon)
                            return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Applies this matrix to a point.
        /// </summary>
        public Vector2 ApplyTo(Vector2 point)
        {
            fixed (float* values = this.values)
            {
                var newPoint = default(Vector2);
                newPoint.X = point.X * values[0] + point.Y * values[1] + values[2];
                newPoint.Y = point.X * values[3] + point.Y * values[4] + values[5];
                return newPoint;
            }
        }

        /// <summary>
        /// Applies this matrix to an array of vectors.
        /// </summary>
        public Vector2[] ApplyTo(params Vector2[] points)
        {
            var result = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
                result[i] = this.ApplyTo(points[i]);
            return result;
        }

        /// <summary>
        /// Applies this matrix to a set of vectors.
        /// </summary>
        public IEnumerable<Vector2> ApplyTo(IEnumerable<Vector2> points)
        {
            foreach (Vector2 point in points)
                yield return this.ApplyTo(point);
        }

        /// <summary>
        /// Applies this matrix to a polygon.
        /// </summary>
        public Polygon ApplyTo(Polygon polygon)
        {
            return new Polygon(this.ApplyTo(polygon.Points));
        }

        private Vertex ApplyTo(Vertex vertex)
        {
            fixed (float* values = this.values)
            {
                float x = vertex.Position.X * values[0] + vertex.Position.Y * values[1] + values[2];
                float y = vertex.Position.X * values[3] + vertex.Position.Y * values[4] + values[5];
                return new Vertex(x, y);
            }
        }

        internal Vertex[] ApplyTo(Vertex[] vertices)
        {
            var result = new Vertex[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                result[i] = this.ApplyTo(vertices[i]);
            return result;
        }

        private void MultiplyRow(int row, float factor)
        {
            int startIndex = row * 3;
            int endIndex = startIndex + 3;
            fixed (float* values = this.values)
            {
                for (int i = startIndex; i < endIndex; i++)
                    values[i] *= factor;
            }
        }

        private void SwapRows()
        {
            fixed (float* values = this.values)
            {
                for (int i = 0; i < 3; i++)
                {
                    float temp = values[i];
                    values[i] = values[i + 3];
                    values[i + 3] = temp;
                }
            }
        }

        private void SubtractRows(int row1, int row2, float factor)
        {
            int row1Start = row1 * 3;
            int row2Start = row2 * 3;
            fixed (float* values = this.values)
            {
                for (int i = 0; i < 3; i++)
                    values[row1Start + i] -= values[row2Start + i] * factor;
            }
        }

        /// <summary>
        /// Returns the inverse matrix of this matrix.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the matrix is singular an therefore not invertible.</exception>
        public Matrix2x3 Invert()
        {
            Matrix2x3 m1 = this;
            Matrix2x3 m2 = Matrix2x3.Identity;

            if (m1.values[0] == 0 || m1.values[4] == 0)
            {
                if (m1.values[1] == 0 || m1.values[3] == 0)
                    throw new InvalidOperationException("This matrix is singular.");

                m1.SwapRows();
                m2.SwapRows();
            }

            if (m1.values[3] != 0)
            {
                float factor = m1.values[3] / m1.values[0];
                m1.SubtractRows(1, 0, factor);
                m2.SubtractRows(1, 0, factor);
            }
            if (m1.values[1] != 0)
            {
                float factor = m1.values[1] / m1.values[4];
                m1.SubtractRows(0, 1, factor);
                m2.SubtractRows(0, 1, factor);
            }

            float f1 = 1 / m1.values[0];
            m1.MultiplyRow(0, f1);
            m2.MultiplyRow(0, f1);
            float f2 = 1 / m1.values[4];
            m1.MultiplyRow(1, f2);
            m2.MultiplyRow(1, f2);
            m2.OffsetX = -m1.OffsetX;
            m2.OffsetY = -m1.OffsetY;

            return m2;
        }

        public override string ToString()
        {
            var vars = new float[6];
            fixed (float* values = this.values) Marshal.Copy((IntPtr)values, vars, 0, vars.Length);
            return string.Format("{{ {0} }}", string.Join(", ", vars));
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        public static Matrix2x3 Scaling(float factorX, float factorY)
        {
            Matrix2x3 m = default(Matrix2x3);
            m.values[0] = factorX;
            m.values[4] = factorY;
            return m;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        public static Matrix2x3 Scaling(float factor)
        {
            return Scaling(factor, factor);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        public static Matrix2x3 Translation(float x, float y)
        {
            Matrix2x3 m = Matrix2x3.Identity;
            m.values[2] = x;
            m.values[5] = y;
            return m;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        public static Matrix2x3 Translation(Vector2 vector)
        {
            Matrix2x3 m = Matrix2x3.Identity;
            m.values[2] = vector.X;
            m.values[5] = vector.Y;
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix.
        /// </summary>
        public static Matrix2x3 Rotation(float angle)
        {
            float sin = MathHelper.Sin(angle);
            float cos = MathHelper.Cos(angle);

            Matrix2x3 m = default(Matrix2x3);
            m.values[0] = cos;
            m.values[1] = -sin;
            m.values[3] = sin;
            m.values[4] = cos;
            return m;
        }

        /// <summary>
        /// Creates a shearing matrix.
        /// </summary>
        public static Matrix2x3 ShearingX(float value)
        {
            Matrix2x3 m = Matrix2x3.Identity;
            m.values[1] = value;
            return m;
        }

        /// <summary>
        /// Creates a shearing matrix.
        /// </summary>
        public static Matrix2x3 ShearingY(float value)
        {
            Matrix2x3 m = Matrix2x3.Identity;
            m.values[3] = value;
            return m;
        }

        /// <summary>
        /// Multiplies zwo matrices.
        /// </summary>
        public static Matrix2x3 Multiply(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 m = default(Matrix2x3);
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    m[x, y] = left[x, 0] * right[0, y] + left[x, 1] * right[1, y] + (y == 2 ? left[x, 2] : 0);
                }
            }
            return m;
        }

        /// <summary>
        /// Multiplies zwo matrices.
        /// </summary>
        public static Matrix2x3 operator *(Matrix2x3 left, Matrix2x3 right)
        {
            return Matrix2x3.Multiply(left, right);
        }
    }
}
