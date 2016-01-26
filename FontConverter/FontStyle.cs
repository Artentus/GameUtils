using System;

namespace FontConverter
{
    [Flags]
    enum FontStyle : short
    {
        Bold = 0x0001,
        Italic = 0x0002,
        Underline = 0x0004,
        Outline = 0x0008,
        Shadow = 0x0010,
        Condensed = 0x0020,
        Extended = 0x0040,
    }
}
