namespace FontConverter
{
    struct GlyphComponent
    {
        public int GlyphIndex;

        public bool UseMetrics;

        public decimal[] Transform;

        public int Arg1, Arg2;

        public bool ArgsAreXYValues;
    }
}
