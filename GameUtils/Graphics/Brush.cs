using System;
using System.Runtime.InteropServices;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public abstract class Brush : IGraphicsResource
    {
        [StructLayout(LayoutKind.Explicit)]
        protected internal unsafe struct BrushBuffer
        {
            [FieldOffset(0)] public int Type;
            [FieldOffset(4)] public float Opacity;
            [FieldOffset(8)] public SharpDX.Vector2 Point1;
            [FieldOffset(16)] public SharpDX.Vector2 Point2;
            [FieldOffset(32)] public fixed float Matrix[21];
            [FieldOffset(116)] public int ColorCount;
            [FieldOffset(128)] public fixed float GradientColors[64];
            [FieldOffset(384)] public fixed float GradientPositions[64];
        }

        public const int MaximumGradientColorCount = 16;

        float opacity;

        public float Opacity
        {
            get { return opacity; }
            set { opacity = MathHelper.Clamp(value, 0f, 1f); }
        }

        public Matrix2x3 Transform { get; set; }

        public abstract bool IsAsync { get; }

        public abstract bool IsReady { get; }

        protected Brush()
        {
            opacity = 1f;
            Transform = Matrix2x3.Identity;
        }

        protected abstract void FillBuffer(ref BrushBuffer buffer);

        internal BrushBuffer CreateBuffer()
        {
            var buffer = new BrushBuffer { Opacity = opacity };
            FillBuffer(ref buffer);
            return buffer;
        }

        protected virtual Math.Vector2 GetTexturePosition(Math.Vector2 vertex, Matrix2x3 inverseTransform)
        {
            return Math.Vector2.Zero;
        }

        internal void UpdateVertices(Vertex[] vertices)
        {
            Matrix2x3 inverseTransform = Transform.Invert();

            for (int i = 0; i < vertices.Length; i++)
            {
                SharpDX.Vector4 vertexPos = vertices[i].Position;
                Math.Vector2 texturePos = GetTexturePosition(new Math.Vector2(vertexPos.X, vertexPos.Y), inverseTransform);
                vertices[i].TexturePosition = new SharpDX.Vector2(texturePos.X, texturePos.Y);
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        { }

        ~Brush()
        {
            Dispose(false);
        }
    }
}
