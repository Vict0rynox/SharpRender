namespace SoftRender.TGA
{
    public struct TgaHeader
    {
        public sbyte IdLenght;

        public sbyte ColorMapType;

        public sbyte DataTypeCode;

        public short ColorMapOrigin;

        public short ColorMapLength;

        public sbyte ColorMapDepth;

        public short XOrigin;

        public short YOrigin;

        public short Width;

        public short Height;

        public sbyte BitSperPixel;

        public sbyte ImageDescription;
    }
}