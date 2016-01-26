namespace GameUtils.Graphics
{
    public enum AntiAliasingMode : uint
    {
        /// <summary>
        /// No Anti-Aliasing is used.
        /// </summary>
        None = 0x00000001,
        /// <summary>
        /// 2x Multisampling Anti-Aliasing.
        /// </summary>
        _2xMSAA = 0x00000002,
        /// <summary>
        /// 4x Multisampling Anti-Aliasing.
        /// </summary>
        _4xMSAA = 0x00000004,
        /// <summary>
        /// 8x Multisampling Anti-Aliasing.
        /// </summary>
        _8xMSAA = 0x00000008,
        /// <summary>
        /// 8x Coverage Sampling Anti-Aliasing on Nvidia cards or 4f8x Enhanced Quality Anti-Aliasing on AMD cards.
        /// </summary>
        _8xCSAA = 0x00080004,
        /// <summary>
        /// 8xQ Coverage Sampling Anti-Aliasing on Nvidia cards, no AMD eqivalent.
        /// </summary>
        _8xQCSAA = 0x00080008,
        /// <summary>
        /// 16x Coverage Sampling Anti-Aliasing on Nvidia cards or 4f16x Enhanced Quality Anti-Aliasing on AMD cards.
        /// </summary>
        _16xCSAA = 0x00100004,
        /// <summary>
        /// 16xQ Coverage Sampling Anti-Aliasing on Nvidia cards or 8f16x Enhanced Quality Anti-Aliasing on AMD cards.
        /// </summary>
        _16xQCSAA = 0x00100008,
    }
}
