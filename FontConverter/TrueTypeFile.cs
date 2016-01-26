using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FontConverter
{
    sealed class TrueTypeFile
    {
        struct TableHeader
        {
            public string Tag;
            public long Checksum;
            public long Offset;
            public long Length;
        }

        struct NameRecord
        {
            public int PlatformId;
            public int EncodingId;
            public int LanguageId;
            public int NameId;
            public int Length;
            public int Offset;
        }

        struct EncodingTable
        {
            public int PlatformId;
            public int EncodingId;
            public long Offset;
        }

        public struct HorizontalMetric
        {
            public int AdvanceWidth;
            public int LeftSideBearing;
        }

        public string FontName { get; private set; }

        public string FamilyName { get; private set; }

        public string SubfamilyName { get; private set; }

        public Version Version { get; private set; }

        public Version Revision { get; private set; }

        public FontFlags Flags { get; private set; }

        public FontStyle Style { get; private set; }

        public Dictionary<char, int> GlyphDictionary { get; private set; }

        public HorizontalMetric[] GlyphMetrics { get; private set; }

        public Glyph[] Glyphs { get; private set; } 

        public int UnitsPerEm { get; private set; }

        public int Ascend { get; private set; }

        public int Descend { get; private set; }

        public int LineGap { get; private set; }

        public TrueTypeFile(string fileName)
        {
            var file = new FileInfo(fileName);
            using (var stream = new MemoryStream((int)file.Length))
            {
                using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(stream);
                    stream.Position = 0;
                }

                using (var reader = new BigEndianBinaryReader(stream))
                {
                    stream.Position = 4;
                    int tableCount = reader.ReadUInt16();
                    stream.Position += 6;

                    var tableList = new List<TableHeader>();
                    for (int i = 0; i < tableCount; i++)
                    {
                        var header = new TableHeader()
                        {
                            Tag = Encoding.ASCII.GetString(reader.ReadBytes(4)),
                            Checksum = reader.ReadUInt32(),
                            Offset = reader.ReadUInt32(),
                            Length = reader.ReadUInt32(),
                        };
                        tableList.Add(header);
                    }

                    TableHeader maxpHeader = tableList.First(header => header.Tag == "maxp");
                    stream.Position = maxpHeader.Offset + 4;
                    int glyphCount = reader.ReadUInt16();

                    TableHeader headHeader = tableList.First(header => header.Tag == "head");
                    bool useLongIndexFormat = ParseHeadTable(stream, reader, headHeader);

                    TableHeader nameHeader = tableList.First(header => header.Tag == "name");
                    ParseNameTable(stream, reader, nameHeader);

                    TableHeader cmapHeader = tableList.First(header => header.Tag == "cmap");
                    GlyphDictionary = ParseCmapTable(stream, reader, cmapHeader);

                    TableHeader hheaHeader = tableList.First(header => header.Tag == "hhea");
                    int metricCount = ParseHheaTable(stream, reader, hheaHeader);

                    TableHeader hmtxHeader = tableList.First(header => header.Tag == "hmtx");
                    GlyphMetrics = ParseHmtxTable(stream, reader, hmtxHeader, metricCount, glyphCount);

                    TableHeader glyfHeader = tableList.First(header => header.Tag == "glyf");
                    TableHeader locaHeader = tableList.First(header => header.Tag == "loca");
                    Glyphs = ParseGlyfTable(stream, reader, glyfHeader, locaHeader, glyphCount, useLongIndexFormat);
                }
            }
        }

        bool ParseHeadTable(Stream stream, BinaryReader reader, TableHeader header)
        {
            stream.Position = header.Offset;
            Version = new Version(reader.ReadUInt16(), reader.ReadUInt16());
            Revision = new Version(reader.ReadUInt16(), reader.ReadUInt16());
            stream.Position += 8;
            Flags = (FontFlags)reader.ReadInt16();
            UnitsPerEm = reader.ReadUInt16();
            stream.Position += 24;
            Style = (FontStyle)reader.ReadInt16();
            stream.Position += 4;
            return Convert.ToBoolean(reader.ReadUInt16());
        }

        void ParseNameTable(Stream stream, BinaryReader reader, TableHeader header)
        {
            stream.Position = header.Offset + 2;
            int recordCount = reader.ReadUInt16();
            int offset = reader.ReadUInt16();

            var recordList = new List<NameRecord>();
            for (int i = 0; i < recordCount; i++)
            {
                var record = new NameRecord()
                {
                    PlatformId = reader.ReadUInt16(),
                    EncodingId = reader.ReadUInt16(),
                    LanguageId = reader.ReadUInt16(),
                    NameId = reader.ReadUInt16(),
                    Length = reader.ReadUInt16(),
                    Offset = reader.ReadUInt16(),
                };
                recordList.Add(record);
            }

            var englishRecords = recordList.Where(record => record.PlatformId == 3 && record.EncodingId == 1 && record.LanguageId == 0x0409);

            var familyNameRecord = englishRecords.First(record => record.NameId == 1);
            stream.Position = header.Offset + offset + familyNameRecord.Offset;
            var bytes = reader.ReadBytes(familyNameRecord.Length);
            FamilyName = Encoding.BigEndianUnicode.GetString(bytes);

            var subfamilyNameRecord = englishRecords.First(record => record.NameId == 2);
            stream.Position = header.Offset + offset + subfamilyNameRecord.Offset;
            bytes = reader.ReadBytes(subfamilyNameRecord.Length);
            SubfamilyName = Encoding.BigEndianUnicode.GetString(bytes);

            var fontNameRecord = englishRecords.First(record => record.NameId == 4);
            stream.Position = header.Offset + offset + fontNameRecord.Offset;
            bytes = reader.ReadBytes(fontNameRecord.Length);
            FontName = Encoding.BigEndianUnicode.GetString(bytes);
        }

        Dictionary<char, int> ParseCmapTable(Stream stream, BinaryReader reader, TableHeader header)
        {
            stream.Position = header.Offset + 2;
            int tableCount = reader.ReadUInt16();

            var tableList = new List<EncodingTable>();
            for (int i = 0; i < tableCount; i++)
            {
                var table = new EncodingTable()
                {
                    PlatformId = reader.ReadUInt16(),
                    EncodingId = reader.ReadUInt16(),
                    Offset = reader.ReadUInt32(),
                };
                tableList.Add(table);
            }

            EncodingTable unicodeTable = tableList.Find(table => table.PlatformId == 3 && table.EncodingId == 1);
            stream.Position = header.Offset + unicodeTable.Offset + 2;

            int length = reader.ReadUInt16();
            stream.Position += 2;

            int segCount = reader.ReadUInt16() / 2;
            stream.Position += 6;

            var endCode = new int[segCount];
            for (int i = 0; i < segCount; i++)
                endCode[i] = reader.ReadUInt16();
            stream.Position += 2;

            var startCode = new int[segCount];
            for (int i = 0; i < segCount; i++)
                startCode[i] = reader.ReadUInt16();

            var idDelta = new int[segCount];
            for (int i = 0; i < segCount; i++)
                idDelta[i] = reader.ReadInt16();

            long offset = stream.Position;
            var idRangeOffset = new int[segCount];
            for (int i = 0; i < segCount; i++)
                idRangeOffset[i] = reader.ReadUInt16();

            var glyphDictionary = new Dictionary<char, int>();
            for (int i = 0; i < segCount - 1; i++)
            {
                if (idRangeOffset[i] == 0)
                {
                    for (int c = startCode[i]; c <= endCode[i]; c++)
                        glyphDictionary.Add((char)c, idDelta[i] + c);
                }
                else
                {
                    for (int c = startCode[i]; c <= endCode[i]; c++)
                    {
                        stream.Position = idRangeOffset[i] + 2 * (c - startCode[i]) + offset + i * 2;
                        glyphDictionary.Add((char)c, idDelta[i] + reader.ReadUInt16());
                    }
                }
            }

            return glyphDictionary;
        }

        int ParseHheaTable(Stream stream, BinaryReader reader, TableHeader header)
        {
            stream.Position = header.Offset + 4;
            Ascend = reader.ReadInt16();
            Descend = reader.ReadInt16();
            LineGap = reader.ReadInt16();
            stream.Position += 24;
            return reader.ReadUInt16();
        }

        HorizontalMetric[] ParseHmtxTable(Stream stream, BinaryReader reader, TableHeader header, int metricCount, int glyphCount)
        {
            stream.Position = header.Offset;

            var metrics = new HorizontalMetric[glyphCount];
            for (int i = 0; i < metricCount; i++)
            {
                metrics[i] = new HorizontalMetric()
                {
                    AdvanceWidth = reader.ReadUInt16(),
                    LeftSideBearing = reader.ReadInt16(),
                };
            }

            for (int i = metricCount; i < glyphCount; i++)
            {
                metrics[i] = new HorizontalMetric()
                {
                    AdvanceWidth = metrics[metricCount - 1].AdvanceWidth,
                    LeftSideBearing = reader.ReadInt16(),
                };
            }

            return metrics;
        }

        Glyph[] ParseGlyfTable(Stream stream, BinaryReader reader, TableHeader glyfHeader, TableHeader locaHeader, int glyphCount, bool useLongIndexFormat)
        {
            stream.Position = locaHeader.Offset;

            var glyphOffsets = new long[glyphCount + 1];
            Func<long> read = useLongIndexFormat
                ? new Func<long>(() => (long)reader.ReadUInt32())
                : new Func<long>(() => reader.ReadUInt16() * 2L);
            for (int i = 0; i < glyphCount + 1; i++)
                glyphOffsets[i] = read();

            var glyphs = new Glyph[glyphCount];
            for (int i = 0; i < glyphCount; i++)
            {
                if (glyphOffsets[i + 1] - glyphOffsets[i] <= 0)
                {
                    glyphs[i] = Glyph.Empty;
                }
                else
                {
                    stream.Position = glyfHeader.Offset + glyphOffsets[i];
                    glyphs[i] = new Glyph(stream, reader);
                }
            }
            return glyphs;
        }
    }
}
