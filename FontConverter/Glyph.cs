using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FontConverter
{
    sealed class Glyph
    {
        [Flags]
        enum PointFlags : byte
        {
            OnCurve = 0x01,
            XShortVector = 0x02,
            YShortVector = 0x04,
            Repeat = 0x08,
            XIsSame = 0x10,
            XIsPositive = XIsSame,
            YIsSame = 0x20,
            YIsPositive = YIsSame,
        }

        [Flags]
        enum ComponentFlags : short
        {
            ArgsAreWords = 0x0001,
            ArgsAreXYValues = 0x0002,
            IsScaled = 0x0008,
            MoreComponents = 0x0020,
            DifferentXYScale = 0x0040,
            TwoByTwoScale = 0x0080,
            UseMetrics = 0x0200,
        }

        public static Glyph Empty => new Glyph();

        public int XMin { get; private set; }

        public int XMax { get; private set; }

        public int YMin { get; private set; }

        public int YMax { get; private set; }

        public bool IsCompound { get; private set; }

        public List<ControlPoint>[] Contours { get; private set; }

        public List<GlyphComponent> Components { get; private set; }

        public Glyph(Stream stream, BinaryReader reader)
        {
            int contourCount = reader.ReadInt16();
            XMin = reader.ReadInt16();
            YMin = reader.ReadInt16();
            XMax = reader.ReadInt16();
            YMax = reader.ReadInt16();

            if (contourCount < 0)
            {
                IsCompound = true;
                ReadCompound(stream, reader);
            }
            else
            {
                IsCompound = false;
                ReadSimple(stream, reader, contourCount);
            }
        }

        private Glyph()
        {
            IsCompound = false;
            Contours = new List<ControlPoint>[0];
        }

        void ReadCompound(Stream stream, BinaryReader reader)
        {
            Components = new List<GlyphComponent>();
            bool moreComponents = true;
            while (moreComponents)
            {
                ComponentFlags flags = (ComponentFlags)reader.ReadInt16();
                int index = reader.ReadUInt16();

                int arg1, arg2;
                if (flags.HasFlag(ComponentFlags.ArgsAreWords))
                {
                    arg1 = reader.ReadUInt16();
                    arg2 = reader.ReadUInt16();
                }
                else
                {
                    arg1 = reader.ReadByte();
                    arg2 = reader.ReadByte();
                }

                decimal a, b, c, d;
                if (flags.HasFlag(ComponentFlags.IsScaled))
                {
                    a = reader.ReadFixed16();
                    d = a;
                    b = c = 0;
                }
                else if (flags.HasFlag(ComponentFlags.DifferentXYScale))
                {
                    a = reader.ReadFixed16();
                    d = reader.ReadFixed16();
                    b = c = 0;
                }
                else if (flags.HasFlag(ComponentFlags.TwoByTwoScale))
                {
                    a = reader.ReadFixed16();
                    b = reader.ReadFixed16();
                    c = reader.ReadFixed16();
                    d = reader.ReadFixed16();
                }
                else
                {
                    a = d = 1;
                    b = c = 0;
                }

                decimal m = Math.Max(Math.Abs(a), Math.Abs(b));
                if (Math.Abs(Math.Abs(a) - Math.Abs(c)) <= 33m / 65536m) m *= 2; //b?
                decimal n = Math.Max(Math.Abs(c), Math.Abs(d));
                if (Math.Abs(Math.Abs(c) - Math.Abs(d)) <= 33m / 65536m) n *= 2;

                Components.Add(new GlyphComponent()
                {
                    GlyphIndex = index,
                    UseMetrics = flags.HasFlag(ComponentFlags.UseMetrics),
                    Arg1 = arg1,
                    Arg2 = arg2,
                    ArgsAreXYValues = flags.HasFlag(ComponentFlags.ArgsAreXYValues),
                    Transform = new[] { a, b, c, d, m, n }
                });

                moreComponents = flags.HasFlag(ComponentFlags.MoreComponents);
            }
        }

        void ReadSimple(Stream stream, BinaryReader reader, int contourCount)
        {
            var endPointIndices = new int[contourCount];
            for (short i = 0; i < contourCount; i++)
                endPointIndices[i] = reader.ReadUInt16();
            int pointCount = endPointIndices.Last() + 1;

            int instructionLength = reader.ReadUInt16();
            stream.Position += instructionLength;

            var flags = new PointFlags[pointCount];
            int flagCount = 0;
            while (flagCount < pointCount)
            {
                PointFlags currentFlags = (PointFlags)reader.ReadByte();
                flags[flagCount] = currentFlags;
                flagCount++;

                if (currentFlags.HasFlag(PointFlags.Repeat))
                {
                    int newCount = flagCount + reader.ReadByte();
                    for (; flagCount < newCount; flagCount++)
                        flags[flagCount] = currentFlags;
                }
            }

            var xCoordinates = new List<int>[contourCount];
            int contourIndex = 0;
            int previousX = 0;
            for (int i = 0; i < pointCount; i++)
            {
                if (xCoordinates[contourIndex] == null)
                    xCoordinates[contourIndex] = new List<int>();

                int x = flags[i].HasFlag(PointFlags.XShortVector)
                    ? ((flags[i].HasFlag(PointFlags.XIsPositive) ? reader.ReadByte() : -reader.ReadByte()) + previousX)
                    : (flags[i].HasFlag(PointFlags.XIsSame) ? previousX : (reader.ReadInt16() + previousX));
                xCoordinates[contourIndex].Add(x);
                previousX = x;

                if (endPointIndices.Contains(i))
                    contourIndex++;
            }

            Contours = new List<ControlPoint>[contourCount];
            contourIndex = 0;
            int pointIndex = 0;
            int previousY = 0;
            for (int i = 0; i < pointCount; i++)
            {
                if (Contours[contourIndex] == null)
                    Contours[contourIndex] = new List<ControlPoint>();

                int y = flags[i].HasFlag(PointFlags.YShortVector)
                    ? ((flags[i].HasFlag(PointFlags.YIsPositive) ? reader.ReadByte() : -reader.ReadByte()) + previousY)
                    : (flags[i].HasFlag(PointFlags.YIsSame) ? previousY : (reader.ReadInt16() + previousY));
                Contours[contourIndex].Add(new ControlPoint(xCoordinates[contourIndex][pointIndex], y, flags[i].HasFlag(PointFlags.OnCurve)));
                previousY = y;

                if (endPointIndices.Contains(i))
                {
                    contourIndex++;
                    pointIndex = 0;
                }
                else
                {
                    pointIndex++;
                }
            }
        }
    }
}
