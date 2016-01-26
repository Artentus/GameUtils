using System.Runtime.InteropServices;
using SharpDX;

namespace GameUtils.Graphics
{
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct BrushBuffer
    {
        [FieldOffset(0)] public int Type;
        [FieldOffset(4)] public float Opacity;
        [FieldOffset(8)] public Vector2 Point1;
        [FieldOffset(16)] public Vector2 Point2;
        [FieldOffset(32)] public fixed float Matrix[21];
        [FieldOffset(116)] public int ColorCount;
        [FieldOffset(128)] public fixed float GradientColors[64];
        [FieldOffset(384)] public fixed float GradientPositions[64];
    }
}