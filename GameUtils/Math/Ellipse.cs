using System;

namespace GameUtils.Math
{
    [Serializable]
    public struct Ellipse
    {
        float radiusX, radiusY;

        /// <summary>
        /// The center point of this ellipse.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        /// The horizontal radius of this ellipse.
        /// </summary>
        /// <exception cref="System.ArgumentException">Is thrown when value is less then zero.</exception>
        public float RadiusX
        {
            get { return radiusX; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The radius cannot be less than zero.", "value");

                radiusX = value;
            }
        }

        /// <summary>
        /// The vertical radius of this ellipse.
        /// </summary>
        /// <exception cref="System.ArgumentException">Is thrown when value is less then zero.</exception>
        public float RadiusY
        {
            get
            {
                return radiusY;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The radius cannot be less than zero.", "value");

                radiusY = value;
            }
        }

        /// <summary>
        /// Creates a new ellipse.
        /// </summary>
        public Ellipse(Vector2 center, float radiusX, float radiusY)
            : this()
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }
    }
}
