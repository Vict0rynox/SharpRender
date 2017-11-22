using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using SoftRender.TGA;
using Vulkan;
using Xunit;
using Xunit.Sdk;

namespace SoftRenderUnitTest.Tga
{
    public class TgaHeaderUnitTest
    {
        private readonly int _tgaHeaderSize;
        private TgaHeader _header;

        public TgaHeaderUnitTest()
        {
            _tgaHeaderSize = 18;
        }

        [Fact]
        public void SizeOfTest()
        {
            //Structure is actual pack.
            Xunit.Assert.True(Marshal.SizeOf<TgaHeader>() == _tgaHeaderSize);
        }

        /// <summary>
        /// Sent to test byte array has this template
        /// {
        ///    0, //IdLenght
        ///    0, //ColorMapType
        ///    2, //DataTypeCode
        ///    0, 0, //ColorMapOrigin
        ///    0, 0, //ColorMapLength
        ///    0, //ColorMapDepth
        ///    0, 0, //XOrigin
        ///    0, 0, //YOrigin
        ///    150, 0, //Width
        ///    150, 0, //Height
        ///    32, //BitSperPixel
        ///    0x20, //ImageDescription
        ///}
        /// </summary>
        /// <param name="headerBytes"></param>
        [Theory]
        [InlineData(new byte[] {0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150, 0, 150, 0, 32, 0x10,})]
        [InlineData(new byte[] {0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 100, 0, 32, 0x20,})]
        [InlineData(new byte[] {0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 200, 0, 50, 0, 32, 0x20,})]
        [InlineData(new byte[] {0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 250, 0, 10, 0, 32, 0x10,})]
        public void TgaHeader_CreateFromBytes_Success(byte[] headerBytes)
        {
            _header = new TgaHeader(headerBytes);

            Xunit.Assert.True(_header.IdLenght == headerBytes[0]);
            Xunit.Assert.True(_header.ColorMapType == headerBytes[1]);
            Xunit.Assert.True(_header.DataTypeCode == headerBytes[2]);
            Xunit.Assert.True(_header.ColorMapOrigin == BitConverter.ToInt16(headerBytes, 3));
            Xunit.Assert.True(_header.ColorMapLength == BitConverter.ToInt16(headerBytes, 5));
            Xunit.Assert.True(_header.ColorMapDepth == headerBytes[7]);
            Xunit.Assert.True(_header.XOrigin == BitConverter.ToInt16(headerBytes, 8));
            Xunit.Assert.True(_header.YOrigin == BitConverter.ToInt16(headerBytes, 10));
            Xunit.Assert.True(_header.Width == BitConverter.ToInt16(headerBytes, 12));
            Xunit.Assert.True(_header.Height == BitConverter.ToInt16(headerBytes, 14));
            Xunit.Assert.True(_header.BitSperPixel == headerBytes[16]);
            Xunit.Assert.True(_header.ImageDescription == headerBytes[17]);
        }

        [Theory]
        [InlineData(new byte[] {0, 0, 11, 0, 123, 0, 23, 21, 0, 10, 0, 0, 0, 250, 0, 10, 0, 32, 0x10, 012, 23})]
        [InlineData(new byte[] {0, 0, 11, 0, 123, 23, 68, 26, 0, 10, 0, 0, 0, 250, 0, 10, 0, 32, 0x10, 012, 23})]
        [InlineData(new byte[] {0, 0, 11, 0, 123, 73, 53, 28, 0, 10, 0, 0, 0, 20, 0, 1, 0, 32, 0x10, 012, 23})]
        public void TgaHeader_CreateFromBytesMoreThen_Failure(byte[] headerBytes)
        {
            Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => { _header = new TgaHeader(headerBytes); });
        }

        [Theory]
        [InlineData(new byte[] {0, 0, 11, 0, 123, 0, })]
        [InlineData(new byte[] {0, 0, 11, 0, })]
        [InlineData(new byte[] {0, 0,})]
        public void TgaHeader_CreateFromNotEnoughBytes_Failure(byte[] headerBytes)
        {
            Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => { _header = new TgaHeader(headerBytes); });

        }
    }
}