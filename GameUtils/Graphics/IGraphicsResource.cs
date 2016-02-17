using System;

namespace GameUtils.Graphics
{
    /// <summary>
    /// A resource for the graphics subsystem, e.g. textures, brushes.
    /// </summary>
    public interface IGraphicsResource : IDisposable
    {
        /// <summary>
        /// Defines if this resource makes use of asynchroneous loading. A resource may never change this value.
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Indicates whether the resource is ready to be used.
        /// </summary>
        /// <remarks>
        /// Using resources that are not ready may cause unexpected behavior or crash.
        /// Resources that do not have the IsAsync-flag set are always ready.
        /// </remarks>
        bool IsReady { get; }
    }
}
