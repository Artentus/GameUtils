using System;

namespace GameUtils.Graphics
{
    [Flags]
    public enum WrapMode
    {
        /// <summary>
        /// Set pixels beyond the texture to the nearest border pixel of the texture.
        /// </summary>
        Clamp = 0x0,
        /// <summary>
        /// Repeat the texture.
        /// </summary>
        Tile = 0x1,
        /// <summary>
        /// Repeat the texture in x direction and mirror it every second time.
        /// </summary>
        MirrorX = 0x2,
        /// <summary>
        /// Repeat the texture in y direction and mirror it every second time.
        /// </summary>
        MirrorY = 0x4,
        /// <summary>
        /// Repeat the texture in both directions and mirror it every second time in x direction.
        /// The same as Tile | MirrorX.
        /// </summary>
        MirrorXTileY = Tile | MirrorX,
        /// <summary>
        /// Repeat the texture in both directions and mirror it every second time in y direction.
        /// The same as Tile | MirrorY.
        /// </summary>
        MirrorYTileX = Tile | MirrorY,
        /// <summary>
        /// Repeat the texture in both directions and mirror it every second time.
        /// The same as MirrorX | MirrorY.
        /// </summary>
        MirrorXY = MirrorX | MirrorY,
    }
}

