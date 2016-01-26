namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Defines how the end of a line segment is shaped.
    /// </summary>
    public enum CapStyle
    {
        /// <summary>
        /// The segment does not extend past the last vertex.
        /// </summary>
        Flat,
        /// <summary>
        /// The segment extends with half the stroke width past the last vertex.
        /// </summary>
        Square,
        /// <summary>
        /// The line ends in a circle with its diameter being the stroke width.
        /// </summary>
        Round,
    }
}
