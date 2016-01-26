using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpDX;

namespace GameUtils.Graphics
{
    /// <summary>
    /// A font.
    /// </summary>
    public sealed class Font : IResource
    {
        internal sealed class Glyph
        {
            internal readonly int[] Indices;
            internal readonly Vertex[] Vertices;
            internal readonly int AdvanceWidth;
            internal readonly int LeftSideBearing;

            public Glyph(BinaryReader reader)
            {
                AdvanceWidth = reader.ReadInt32();
                LeftSideBearing = reader.ReadInt32();

                Indices = new int[reader.ReadInt32()];
                for (int i = 0; i < Indices.Length; i++)
                    Indices[i] = reader.ReadInt32();

                Vertices = new Vertex[reader.ReadInt32()];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    var v = new Vertex(reader.ReadSingle(), reader.ReadSingle());
                    v.Mode = (VertexMode)reader.ReadByte();
                    if (v.Mode != VertexMode.Default)
                        v.BezierCoordinates = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                    Vertices[i] = v;
                }
            }
        }

        /// <summary>
        /// The Name of the font.
        /// </summary>
        public string FontName { get; private set; }

        /// <summary>
        /// The style of the font.
        /// </summary>
        public FontStyle Style { get; private set; }

        internal int UnitsPerEm { get; private set; }

        internal Glyph[] Glyphs { get; private set; }

        internal Dictionary<char, int> GlyphDictionary { get; private set; }

        internal int Ascend { get; private set; }

        internal int Descend { get; private set; }

        internal int LineGap { get; private set; }

        public object Tag { get; set; }

        internal Glyph GetGlyph(char c)
        {
            int glyphIndex;
            return GlyphDictionary.TryGetValue(c, out glyphIndex) ? Glyphs[glyphIndex] : Glyphs[0];
        }

        void Read(BinaryReader reader)
        {
            int nameLength = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(nameLength);
            FontName = Encoding.UTF8.GetString(nameBytes);

            Style = (FontStyle)reader.ReadInt32();
            UnitsPerEm = reader.ReadInt32();
            Ascend = reader.ReadInt32();
            Descend = reader.ReadInt32();
            LineGap = reader.ReadInt32();

            GlyphDictionary = new Dictionary<char, int>();
            int dictionaryCount = reader.ReadInt32();
            for (int i = 0; i < dictionaryCount; i++)
                GlyphDictionary.Add(reader.ReadChar(), reader.ReadInt32());

            Glyphs = new Glyph[reader.ReadInt32()];
            for (int i = 0; i < Glyphs.Length; i++)
                Glyphs[i] = new Glyph(reader);
        }

        /// <summary>
        /// Creates a font from a stream.
        /// </summary>
        public Font(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Unicode))
            {
                if (Encoding.ASCII.GetString(reader.ReadBytes(6)) != "gufont")
                    throw new ArgumentException("The specified stream does not contain valid font data.", nameof(stream));

                Read(reader);
            }
        }

        /// <summary>
        /// Creates a font from a file.
        /// </summary>
        public Font(string fileName)
        {
            var file = new FileInfo(fileName);
            using (FileStream stream = file.Open(FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.Unicode))
                {
                    if (Encoding.ASCII.GetString(reader.ReadBytes(6)) != "gufont")
                        throw new ArgumentException("The specified file does not contain valid font data.", nameof(fileName));

                    Read(reader);
                }
            }
        }

        UpdateMode IResource.UpdateMode => UpdateMode.Synchronous;

        bool IResource.IsReady
        {
            get { return true; }
            set { }
        }

        void IResource.ApplyChanges()
        { }

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                Glyphs = null;
                GlyphDictionary = null;
            }
        }

        ~Font()
        {
            Dispose(false);
        }
    }
}
