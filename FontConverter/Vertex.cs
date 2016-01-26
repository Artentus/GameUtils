namespace FontConverter
{
    struct Vertex
    {
        public float X;
        public float Y;
        public float U;
        public float V;
        public bool IsInside;
        public bool IsCurve;

        public Vertex(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        public Vertex(float x, float y, float u, float v, bool isInside)
        {
            X = x;
            Y = y;
            U = u;
            V = v;
            IsInside = isInside;
            IsCurve = true;
        }
    }
}
