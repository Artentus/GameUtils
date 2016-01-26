using System;

namespace GameUtils
{
    /// <summary>
    /// A resource.
    /// </summary>
    public interface IResource : IDisposable
    {
        /// <summary>
        /// An unique identifier for the resource.
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Defines how this resource is updated.
        /// </summary>
        UpdateMode UpdateMode { get; }

        /// <summary>
        /// Indicates whether the resource is ready for use. Don't use resources witch aren't ready.
        /// This property should only be set by the engine, don't set it by yourself!
        /// </summary>
        bool IsReady { get; set; }

        /// <summary>
        /// Applies all outstanding changes to this resource.
        /// </summary>
        /// <remarks>Due to synchronization reasons, a resource is intended to not apply any changes until this method is called.</remarks>
        void ApplyChanges();
    }
}
