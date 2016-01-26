namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Defines how two line segments are joined together.
    /// </summary>
    public enum LineJoin
    {
        /// <summary>
        /// The corners are beveled.
        /// </summary>
        Bevel,
        /// <summary>
        /// The corners are rounded.
        /// </summary>
        Round,
        /// <summary>
        /// Regular angular corners unless the miter limit is exceeded, in this case bevel is used.
        /// </summary>
        Miter,
    }
}
