using System;

namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Holds information about how lines are drawn.
    /// </summary>
    public class StrokeStyle
    {
        float miterLimit;

        /// <summary>
        /// Defines how two line segments are joined together.
        /// </summary>
        public LineJoin LineJoin { get; set; }

        /// <summary>
        /// Defines how the end of a line segment is shaped.
        /// </summary>
        public CapStyle CapStyle { get; set; }

        /// <summary>
        /// The miter limit in multiples of the stroke width. Values smaller than 2 are not allowed.
        /// </summary>
        public float MiterLimit
        {
            get { return miterLimit; }
            set
            {
                if (value < 2) throw new ArgumentOutOfRangeException("value");
                miterLimit = value;
            }
        }

        public StrokeStyle()
        {
            miterLimit = 2;
        }
    }
}
