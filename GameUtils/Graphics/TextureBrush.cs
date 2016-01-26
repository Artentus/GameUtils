using System.IO;
using GameUtils.Math;
using Vector2 = SharpDX.Vector2;

namespace GameUtils.Graphics
{
    public sealed class TextureBrush : Brush
    {
        bool disposeTexture;

        Texture currentTexture;
        Texture newTexture;

        WrapMode currentWrapMode;
        WrapMode newWrapMode;

        InterpolationMode currentInterpolationMode;
        InterpolationMode newInterpolationMode;

        public Texture Texture
        {
            get { return currentTexture; }
            set { newTexture = value; }
        }

        public WrapMode WrapMode
        {
            get { return currentWrapMode; }
            set { newWrapMode = value; }
        }

        public InterpolationMode InterpolationMode
        {
            get { return currentInterpolationMode; }
            set { newInterpolationMode = value; }
        }

        public override bool IsReady
        {
            get { return currentTexture.IsReady; }
        }

        internal override void FillBuffer(ref BrushBuffer buffer)
        {
            buffer.Type = 4;
        }

        internal override void UpdateVertices(Vertex[] vertices)
        {
            Matrix2x3 inverseTransform = Transform.Invert();

            for (int i = 0; i < vertices.Length; i++)
            {
                Math.Vector2 textPos = new Math.Vector2(vertices[i].Position.X, vertices[i].Position.Y);
                textPos = inverseTransform.ApplyTo(textPos);
                textPos.X /= currentTexture.Width;
                textPos.Y /= currentTexture.Height;
                vertices[i].TexturePosition = new Vector2(textPos.X, textPos.Y);
            }
        }

        protected override void ApplyChanges()
        {
            base.ApplyChanges();

            if (newTexture != currentTexture)
            {
                if (disposeTexture) currentTexture.Dispose();
                currentTexture = newTexture;
                disposeTexture = false;
            }
            currentWrapMode = newWrapMode;
            currentInterpolationMode = newInterpolationMode;
        }

        public TextureBrush(Texture texture)
        {
            disposeTexture = false;

            currentTexture = texture;
            newTexture = texture;
            currentInterpolationMode = InterpolationMode.Default;
        }

        public TextureBrush(string fileName, bool loadAsync = false)
        {
            disposeTexture = true;

            currentTexture = new Texture(fileName, loadAsync);
            newTexture = currentTexture;
            currentInterpolationMode = InterpolationMode.Default;
        }

        public TextureBrush(Stream stream, bool loadAsync = false)
        {
            disposeTexture = true;

            currentTexture = new Texture(stream, loadAsync);
            newTexture = currentTexture;
            currentInterpolationMode = InterpolationMode.Default;
        }
    }
}
