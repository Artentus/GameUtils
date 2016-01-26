using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FontConverter
{
    sealed class GameUtilsFont
    {
        public string FontName { get; set; }

        public FontStyle Style { get; set; }

        public readonly int UnitsPerEm;

        public readonly TriangulatedGlyph[] glyphs;

        public readonly Dictionary<char, int> glyphDictionary;

        readonly int ascend, descend, lineGap;

        public GameUtilsFont(TrueTypeFile trueTypeData)
        {
            FontName = trueTypeData.FamilyName;
            Style = trueTypeData.Style;
            UnitsPerEm = trueTypeData.UnitsPerEm;
            glyphDictionary = trueTypeData.GlyphDictionary;
            ascend = trueTypeData.Ascend;
            descend = trueTypeData.Descend;
            lineGap = trueTypeData.LineGap;

            glyphs = new TriangulatedGlyph[trueTypeData.Glyphs.Length];
            for (int i = 0; i < glyphs.Length; i++)
                glyphs[i] = new TriangulatedGlyph(trueTypeData.Glyphs[i], trueTypeData.GlyphMetrics[i]);
        }

        public void Save(string fileName)
        {
            var file = new FileInfo(fileName);
            using (FileStream stream = file.Open(FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(stream, Encoding.Unicode))
                {
                    writer.Write(Encoding.ASCII.GetBytes("gufont"));

                    byte[] nameBytes = Encoding.UTF8.GetBytes(FontName);
                    writer.Write(nameBytes.Length);
                    writer.Write(nameBytes);

                    writer.Write((int)Style);
                    writer.Write(UnitsPerEm);
                    writer.Write(ascend);
                    writer.Write(descend);
                    writer.Write(lineGap);

                    writer.Write(glyphDictionary.Count);
                    foreach (var kvp in glyphDictionary)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value);
                    }

                    writer.Write(glyphs.Length);
                    foreach (var glyph in glyphs)
                        glyph.Save(writer);
                }
            }
        }
    }
}
