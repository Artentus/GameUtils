using System;

namespace GameUtils.Math
{
    [Serializable]
    public struct Rectangle
    {
        /// <summary>
        /// A rectangle with all fields set to zero.
        /// </summary>
        public static readonly Rectangle Empty;

        static Rectangle()
        {
            Empty = default(Rectangle);
        }

        Vector2 location;
        Vector2 size;

        /// <summary>
        /// The location of this rectangle.
        /// </summary>
        public Vector2 Location
        {
            get { return location; }
            set { location = value; }
        }

        /// <summary>
        /// The size of this rectangle.
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// The X coordinate of the rectangle's location.
        /// </summary>
        public float X
        {
            get { return location.X; }
            set { location.X = value; }
        }

        /// <summary>
        /// The Y coordinate of the rectangle's location.
        /// </summary>
        public float Y
        {
            get { return location.Y; }
            set { location.Y = value; }
        }

        /// <summary>
        /// The rectangle's width.
        /// </summary>
        public float Width
        {
            get { return size.X; }
            set { size.X = value; }
        }

        /// <summary>
        /// The rectangle's height.
        /// </summary>
        public float Height
        {
            get { return size.Y; }
            set { size.Y = value; }
        }

        /// <summary>
        /// The X coordinate of the rectangle's upper left corner.
        /// </summary>
        public float Left
        {
            get { return System.Math.Min(X, X + Width); }
        }

        /// <summary>
        /// The Y coordinate of the rectangle's upper left corner.
        /// </summary>
        public float Top
        {
            get { return System.Math.Min(Y, Y + Height); }
        }

        /// <summary>
        /// The X coordinate of the rectangle's lower right corner.
        /// </summary>
        public float Right
        {
            get { return System.Math.Max(X, X + Width); }
        }

        /// <summary>
        /// The Y coordinate of the rectangle's lower right corner.
        /// </summary>
        public float Bottom
        {
            get { return System.Math.Max(Y, Y + Height); }
        }

        /// <summary>
        /// Creates a new rectangle.
        /// </summary>
        public Rectangle(Vector2 location, Vector2 size)
            : this()
        {
            Location = location;
            Size = size;
        }

        /// <summary>
        /// Creates a new rectangle.
        /// </summary>
        public Rectangle(float x, float y, float width, float height)
            : this()
        {
            location = new Vector2(x, y);
            size = new Vector2(width, height);
        }

        /// <summary>
        /// Creates a new rectangle out of two single points.
        /// </summary>
        public static Rectangle FromPoints(Vector2 p1, Vector2 p2)
        {
            var rect = default(Rectangle);
            rect.location = new Vector2(System.Math.Min(p1.X, p2.X), System.Math.Min(p1.Y, p2.Y));
            rect.size = new Vector2(System.Math.Max(p1.X, p2.X), System.Math.Max(p1.Y, p2.Y)) - rect.location;
            return rect;
        }

        /// <summary>
        /// Creates a new rectangle out of the given values for top, bottom, left and right.
        /// </summary>
        public static Rectangle FromTopBottomLeftRight(float top, float bottom, float left, float right)
        {
            var rect = default(Rectangle);
            rect.X = left;
            rect.Y = top;
            rect.Width = right - left;
            rect.Height = bottom - top;
            return rect;
        }

        /// <summary>
        /// Checks if this rectangle contains a given point.
        /// </summary>
        public bool Contains(Vector2 point)
        {
            return Left <= point.X && point.X <= Right && Top <= point.Y && point.Y <= Bottom;
        }

        /// <summary>
        /// Checks if this rectangle intersects with another rectangle.
        /// </summary>
        public bool IntersectsWith(Rectangle rect)
        {
            return !(this.Left > rect.Right || this.Right < rect.Left || this.Top > rect.Bottom || this.Bottom < rect.Top);
        }
    }
}
