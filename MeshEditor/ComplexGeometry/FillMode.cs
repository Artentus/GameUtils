namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Describes how a geometry figure's interior is defined.
    /// </summary>
    public enum FillMode
    {
        /// <summary>
        /// Areas with an odd winding number are filled.
        /// </summary>
        Alternate,
        /// <summary>
        /// Areas with a winding number different from zero are filled.
        /// </summary>
        Winding,
    }
}
