using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

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
        
        public TgaHeader(byte[] data) : this()
        {
            var headSizeOf = Marshal.SizeOf(this);
            if (data.Length != headSizeOf)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }
            IntPtr memPtr = Marshal.AllocHGlobal(headSizeOf);
            Marshal.Copy(data, 0, memPtr, headSizeOf);
            this = Marshal.PtrToStructure<TgaHeader>(memPtr);
            Marshal.FreeHGlobal(memPtr);
        }

        public byte[] SerialuzeBytes()
        {
            var headSizeOf = Marshal.SizeOf(this);
            byte[] headerData = new byte[headSizeOf];
            headerData.Initialize(); //init data with zero 

            IntPtr memPtr = Marshal.AllocHGlobal(headSizeOf);
            Marshal.StructureToPtr(this, memPtr, true);
            Marshal.Copy(memPtr, headerData, 0, headSizeOf);
            Marshal.FreeHGlobal(memPtr);
            return headerData;
        }
    }
}