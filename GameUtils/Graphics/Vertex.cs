using SharpDX;

namespace GameUtils.Graphics
{
    internal struct Vertex
    {
        public Vector4 Position;
        public Vector2 TexturePosition;
        public Vector2 BezierCoordinates;
        public VertexMode Mode;

        public Vertex(float x, float y)
        {
            Position = new Vector4(x, y, 0f, 1f);
            TexturePosition = default(Vector2);
            BezierCoordinates = default(Vector2);
            Mode = VertexMode.Default;
        }

        public Vertex(float x, float y, float u, float v)
        {
            Position = new Vector4(x, y, 0f, 1f);
            TexturePosition = new Vector2(u, v);
            BezierCoordinates = default(Vector2);
            Mode = VertexMode.Default;
        }

        public Vertex(Math.Vector2 position)
        {
            Position = new Vector4(position.X, position.Y, 0f, 1f);
            TexturePosition = default(Vector2);
            BezierCoordinates = default(Vector2);
            Mode = VertexMode.Default;
        }
    }
}
