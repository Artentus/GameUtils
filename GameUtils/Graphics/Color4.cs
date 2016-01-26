using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    /// <summary>
    /// Represents a color with alpha, red, green and blue channels.
    /// </summary>
    [Serializable]
    public struct Color4 : IEquatable<Color4>
    {
        float a, r, g, b;

        /// <summary>
        /// The color white.
        /// </summary>
        public static readonly Color4 White;
        /// <summary>
        /// The color black.
        /// </summary>
        public static readonly Color4 Black;
        /// <summary>
        /// The color transparent.
        /// </summary>
        public static readonly Color4 Transparent;

        static Color4()
        {
            White = new Color4(1, 1, 1);
            Black = new Color4(0, 0, 0);
            Transparent = new Color4(0, 0, 0, 0);
        }

        /// <summary>
        /// Creates a new color.
        /// </summary>
        public Color4(float a, float r, float g, float b)
            :this()
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Creates a new color.
        /// </summary>
        public Color4(float r, float g, float b)
            : this()
        {
            A = 1;
            R = r;
            G = g;
            B = b;
        }
        
        /// <summary>
        /// The alpha channel.
        /// </summary>
        /// <remarks>This value has to be between 1 and 0.</remarks>
        public float A
        {
            get { return a; }
            set { a = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// The red channel.
        /// </summary>
        /// <remarks>This value has to be between 1 and 0.</remarks>
        public float R
        {
            get { return r; }
            set { r = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// The green channel.
        /// <remarks>This value has to be between 1 and 0.</remarks>
        /// </summary>
        public float G
        {
            get { return g; }
            set { g = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// The blue channel.
        /// </summary>
        /// <remarks>This value has to be between 1 and 0.</remarks>
        public float B
        {
            get { return b; }
            set { b = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// Creates a color from the specified argb-value.
        /// </summary>
        unsafe public static Color4 FromArgb(int argb)
        {
            byte* bytes = (byte*)&argb;
            return Color4.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        /// <summary>
        /// Creates a color from the specified argb-value.
        /// </summary>
        public static Color4 FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color4()
            {
                A = (float)a / byte.MaxValue,
                R = (float)r / byte.MaxValue,
                G = (float)g / byte.MaxValue,
                B = (float)b / byte.MaxValue,
            };
        }

        /// <summary>
        /// Creates a color from the specified argb-value.
        /// </summary>
        public static Color4 FromArgb(byte r, byte g, byte b)
        {
            return new Color4()
            {
                A = 1,
                R = (float)r / byte.MaxValue,
                G = (float)g / byte.MaxValue,
                B = (float)b / byte.MaxValue,
            };
        }

        /// <summary>
        /// Converts this color to its argb-value.
        /// </summary>
        unsafe public int ToArgb()
        {
            int argb;
            byte* bytes = (byte*)&argb;
            bytes[0] = (byte)(A * byte.MaxValue);
            bytes[1] = (byte)(R * byte.MaxValue);
            bytes[2] = (byte)(G * byte.MaxValue);
            bytes[3] = (byte)(B * byte.MaxValue);
            return argb;
        }

        /// <summary>
        /// Converts this color to its argb-value.
        /// </summary>
        public void ToArgb(out byte a, out byte r, out byte g, out byte b)
        {
            a = (byte)(A * byte.MaxValue);
            r = (byte)(R * byte.MaxValue);
            g = (byte)(G * byte.MaxValue);
            b = (byte)(B * byte.MaxValue);
        }

        /// <summary>
        /// Creates a color from the specified hsv-values.
        /// </summary>
        /// <param name="h">The h component (between 0 and 360).</param>
        /// <param name="s">The s component (between 1 and 0).</param>
        /// <param name="v">The v component (between 1 and 0).</param>
        public static Color4 FromHsv(float h, float s, float v)
        {
            h = MathHelper.Clamp(h, 0, 360);
            s = MathHelper.Clamp(s, 0, 1);
            v = MathHelper.Clamp(v, 0, 1);

            if (s == 0)
                return new Color4(v, v, v);

            float pos = h / 60;
            int sector = (int)pos;
            float fract = pos - sector;

            float p = v * (1 - s);
            float q = v * (1 - s * fract);
            float t = v * (1 - s * (1 - fract));

            switch (sector)
            {
                case 0:
                    return new Color4(v, t, p);
                case 1:
                    return new Color4(q, v, p);
                case 2:
                    return new Color4(p, v, t);
                case 3:
                    return new Color4(p, q, v);
                case 4:
                    return new Color4(t, p, v);
                case 5:
                    return new Color4(v, p, q);
                default:
                    return default(Color4);
            }
        }

        /// <summary>
        /// Converts this color to its hsv-values.
        /// </summary>
        /// <param name="h">The h component (between 0 and 360).</param>
        /// <param name="s">The s component (between 1 and 0).</param>
        /// <param name="v">The v component (between 1 and 0).</param>
        public void ToHsv(out float h, out float s, out float v)
        {
            h = default(float);
            s = default(float);
            v = default(float);

            float min = MathHelper.Min(R, G, B);
            float max = MathHelper.Max(R, G, B);

            if (max == 0)
                return;
            
            if (max == R)
            {
                if (g > b)
                    h = 60 * (g - b) / (max - min);
                else if (g < b)
                    h = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == G)
                h = 60 * (b - r) / (max - min) + 120;
            else if (max == b)
                h = 60 * (r - g) / (max - min) + 240;

            s = 1 - min / max;
            v = max;
        }

        public static bool operator ==(Color4 left, Color4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color4 left, Color4 right)
        {
            return !left.Equals(right);
        }

        public static Color4 operator *(Color4 left, Color4 right)
        {
            return Color4.Multiply(left, right);
        }

        public static Color4 Multiply(Color4 left, Color4 right)
        {
            return new Color4(left.A * right.A, left.R * right.R, left.G * right.G, left.B * right.B);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color4)
                return this.Equals((Color4)obj);
            else
                return false;
        }

        public bool Equals(Color4 other)
        {
            return System.Math.Abs(this.A - other.A) <= MathHelper.Epsilon && System.Math.Abs(this.R - other.R) <= MathHelper.Epsilon &&
                System.Math.Abs(this.G - other.G) <= MathHelper.Epsilon && System.Math.Abs(this.B - other.B) <= MathHelper.Epsilon;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Color4 {{ A={0}, R={1}, G={2}, B={3} }}", A, R, G, B);
        }
    }
}
