using System.IO;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public sealed class TextureBrush : Brush
    {
        readonly bool disposeTexture;
        readonly Texture texture;

        public override bool IsAsync { get; }

        public override bool IsReady => texture.IsReady;

        public Texture Texture => texture;

        public Color4 Color { get; set; }

        public WrapMode WrapMode { get; set; }

        public InterpolationMode InterpolationMode { get; set; }

        public TextureBrush(Texture texture)
        {
            IsAsync = texture.IsAsync;
            disposeTexture = false;
            this.texture = texture;

            Color = Color4.White;
            InterpolationMode = InterpolationMode.Default;
        }

        public TextureBrush(string fileName, bool loadAsync = false)
        {
            IsAsync = loadAsync;
            disposeTexture = true;
            texture = new Texture(fileName, loadAsync);

            Color = Color4.White;
            InterpolationMode = InterpolationMode.Default;
        }

        public TextureBrush(Stream stream, bool loadAsync = false)
        {
            IsAsync = loadAsync;
            disposeTexture = true;
            texture = new Texture(stream, loadAsync);

            Color = Color4.White;
            InterpolationMode = InterpolationMode.Default;
        }

        protected override unsafe void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 4;
            fixed (float* colors = buffer.GradientColors)
            {
                colors[0] = Color.R;
                colors[1] = Color.G;
                colors[2] = Color.B;
                colors[3] = Color.A;
            }
        }

        protected override Vector2 GetTexturePosition(Vector2 vertex, Matrix2x3 inverseTransform)
        {
            if (texture == null) return Vector2.Zero;

            Vector2 textPos = inverseTransform.ApplyTo(vertex);
            textPos.X /= texture.Width;
            textPos.Y /= texture.Height;
            return textPos;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposeTexture) texture.Dispose();

            base.Dispose(disposing);
        }
    }
}
