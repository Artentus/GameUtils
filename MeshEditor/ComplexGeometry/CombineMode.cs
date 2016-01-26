namespace MeshEditor.ComplexGeometry
{
    /// <summary>
    /// Defines how two geometry figures are combined.
    /// </summary>
    public enum CombineMode
    {
        /// <summary>
        /// Logical AND.
        /// </summary>
        And = 0x0,
        /// <summary>
        /// Logical OR.
        /// </summary>
        Or = 0x1,
        /// <summary>
        /// Logical NOT.
        /// </summary>
        Not = 0x2,
        /// <summary>
        /// Logical XOR.
        /// </summary>
        XOr = 0x3,
        /// <summary>
        /// An area which is inside both figures.
        /// </summary>
        Intersect = And,
        /// <summary>
        /// An area which is inside either of the figures.
        /// </summary>
        Union = Or,
        /// <summary>
        /// An area which is inside the first figure but not inside the second.
        /// </summary>
        Difference = Not,
    }
}
