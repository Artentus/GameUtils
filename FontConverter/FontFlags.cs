using System;

namespace FontConverter
{
    [Flags]
    internal enum FontFlags : short
    {
        BaselineAtZero = 0x0001,
        LsbAtLeftmostBlackBit = 0x0002,
        UseExplicitPointSize = 0x0004,
        UseIntegerScaling = 0x0008,
    }
}
