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

        Color4 currentColor;
        Color4 newColor;

        WrapMode currentWrapMode;
        WrapMode newWrapMode;

        InterpolationMode currentInterpolationMode;
        InterpolationMode newInterpolationMode;

        public Texture Texture
        {
            get { return currentTexture; }
            set { newTexture = value; }
        }

        public Color4 Color
        {
            get { return currentColor; }
            set { newColor = value; }
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

        internal override unsafe void FillBuffer(ref BrushBuffer buffer)
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
            currentColor = newColor;
            currentWrapMode = newWrapMode;
            currentInterpolationMode = newInterpolationMode;
        }

        public TextureBrush(Texture texture)
        {
            disposeTexture = false;

            currentTexture = texture;
            newTexture = texture;

            currentColor = Color4.White;
            newColor = currentColor;

            currentInterpolationMode = InterpolationMode.Default;
            newInterpolationMode = currentInterpolationMode;
        }

        public TextureBrush(string fileName, bool loadAsync = false)
        {
            disposeTexture = true;

            currentTexture = new Texture(fileName, loadAsync);
            newTexture = currentTexture;

            currentColor = Color4.White;
            newColor = currentColor;

            currentInterpolationMode = InterpolationMode.Default;
            newInterpolationMode = currentInterpolationMode;
        }

        public TextureBrush(Stream stream, bool loadAsync = false)
        {
            disposeTexture = true;

            currentTexture = new Texture(stream, loadAsync);
            newTexture = currentTexture;

            currentColor = Color4.White;
            newColor = currentColor;

            currentInterpolationMode = InterpolationMode.Default;
            newInterpolationMode = currentInterpolationMode;
        }
    }
}
