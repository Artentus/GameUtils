namespace GameUtils
{
    public enum UpdateMode
    {
        /// <summary>
        /// The resource is loaded and updated between two cycles. The data will be available everytime but game performance might be decreased.
        /// Recommended for resources with no or little load and update work.
        /// </summary>
        Synchronous,
        /// <summary>
        /// The resource is loaded asynchronously but updates take place between two cycles.
        /// Recommended for resources with huge load work but no or little update work, e.g. textures.
        /// </summary>
        InitAsynchronous,
        /// <summary>
        /// The resource is loaded and updatet asynchronously. The resource might not be available instantly but game performance is increased.
        /// Recommended for resources with huge update work that would lag the game, e.g. geometries that get changed regularily. 
        /// </summary>
        InitUpdateAsynchronous,
    }
}
