using System.Runtime.InteropServices;

namespace SoftRender.TGA
{
    [StructLayout(LayoutKind.Sequential,Pack=1)]
    public struct TgaHeader
    {
        public byte IdLenght;

        public byte ColorMapType;

        public byte DataTypeCode;

        public short ColorMapOrigin;

        public short ColorMapLength;

        public byte ColorMapDepth;

        public short XOrigin;

        public short YOrigin;

        public short Width;

        public short Height;

        public byte BitSperPixel;

        public byte ImageDescription;
    }
}