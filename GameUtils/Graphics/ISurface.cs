using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    /// <summary>
    /// Represents a rendering surface.
    /// </summary>
    public interface ISurface : IEngineComponent
    {
        /// <summary>
        /// Is risen when the surfaces size was changed.
        /// </summary>
        event EventHandler Resize;

        /// <summary>
        /// Is risen when the surfaces dpi were changed.
        /// </summary>
        event EventHandler DpiChanged;

        /// <summary>
        /// The bounds of the surface.
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// The dpi of the surface.
        /// </summary>
        float Dpi { get; }

        /// <summary>
        /// A output target whitch is usable by the renderer.
        /// </summary>
        /// <remarks>This property is mainly for internal uses an should always return either a Win32 Window or a Core Window.</remarks>
        object OutputTarget { get; }
    }
}
