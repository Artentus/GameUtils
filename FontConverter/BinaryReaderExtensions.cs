using System.IO;

namespace FontConverter
{
    static class BinaryReaderExtensions
    {
        public static decimal ReadFixed16(this BinaryReader reader)
        {
            int raw = reader.ReadInt16();
            int fraction = raw & 0x3fff;
            int mantissa = (raw >> 14) & 0x8001;

            return mantissa + (decimal)fraction / 0x3fff;
        }

        public static decimal ReadFixed32(this BinaryReader reader)
        {
            decimal result = reader.ReadInt16();
            result += ((decimal)reader.ReadUInt16() / (decimal)ushort.MaxValue);
            return result;
        }
    }
}
