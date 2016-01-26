namespace GameUtils.Graphics
{
    /// <summary>
    /// Describes how a texture is interpolated when not drawn in original size.
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// The renderers default interpolation mode is used.
        /// </summary>
        Default,
        /// <summary>
        /// The value of the nearest texel is used. This produces sharp images.
        /// Note that for shrinking the texture linear interpolation is used.
        /// </summary>
        Nearest,
        /// <summary>
        /// The values of the two nearest texels are taken into account. This produces smooth images.
        /// </summary>
        Linear,
        /// <summary>
        /// Anisotrophic filtering is applied. This produces the highest quality smoothing but may be slow.
        /// </summary>
        Anisotropic,
    }
}
