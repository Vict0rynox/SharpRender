using System;
using System.Collections.Generic;
using System.Drawing;
using SharpDX.DXGI;
using SoftRender.TGA;
using Vulkan;
using Xunit;

namespace SoftRenderUnitTest.Tga
{
    public class TgaImageUnitTestDataGenerator
    {
        public static IEnumerable<object[]> GetValidTgaImageParams()
        {
            yield return new object[] {new Size(10, 10), TgaImage.Format.Grayscale};
            yield return new object[] {new Size(10, 10), TgaImage.Format.Rgb};
            yield return new object[] {new Size(10, 10), TgaImage.Format.Rgba};
        }

        public static IEnumerable<object[]> GetNotValidSizeTgaImageParams()
        {
            yield return new object[] {new Size(-10, 10), TgaImage.Format.Grayscale};
            yield return new object[] {new Size(10, -10), TgaImage.Format.Rgb};
            yield return new object[] {new Size(0, 0), TgaImage.Format.Rgba};
        }

        public static IEnumerable<object[]> GetNotValidFormatTgaImageParams()
        {
            yield return new object[] {new Size(10, 10), TgaImage.Format.None};
        }

        public static IEnumerable<object[]> GetValidHeaderTgaImage()
        {
            yield return new object[]
                {new TgaHeader(new byte[] {0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150, 0, 150, 0, 32, 0x10})};
            yield return new object[]
                {new TgaHeader(new byte[] {0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 250, 0, 150, 0, 32, 0x10})};
            yield return new object[]
                {new TgaHeader(new byte[] {0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150, 0, 250, 0, 32, 0x10})};
        }

        public static IEnumerable<object[]> GetValidPointSetPointColor()
        {
            yield return new object[] {new Size(10, 10), new Point(5, 5)};
            yield return new object[] {new Size(10, 10), new Point(0, 0)};
            yield return new object[] {new Size(10, 10), new Point(9, 9)};
        }

        public static IEnumerable<object[]> GetUnvalidPointSetPointColor()
        {
            yield return new object[] {new Size(10, 10), new Point(-5, 5)};
            yield return new object[] {new Size(10, 10), new Point(5, -4)};
            yield return new object[] {new Size(10, 10), new Point(12, 12)};
        }

        public static IEnumerable<object[]> GetValidPointGetPointColor()
        {
            yield return new object[] {new Size(10, 10), new Point(0, 0)};
            yield return new object[] {new Size(15, 15), new Point(7, 7)};
            yield return new object[] {new Size(5, 20), new Point(3, 8)};
        }

        public static IEnumerable<object[]> GetUnvalidPointGetPointColor()
        {
            yield return new object[] {new Size(10, 10), new Point(-1, 9)};
            yield return new object[] {new Size(15, 15), new Point(7, -7)};
            yield return new object[] {new Size(5, 20), new Point(7, 8)};
        }

        public static IEnumerable<object[]> GetValidSizeScale()
        {
            yield return new object[] {new Size(10, 10), new Size(15, 15)};
            yield return new object[] {new Size(10, 10), new Size(10, 15)};
            yield return new object[] {new Size(15, 15), new Size(5, 5)};
            yield return new object[] {new Size(5, 20), new Size(10, 10)};
        }

        public static IEnumerable<object[]> GetUnvalidSizeScale()
        {
            yield return new object[] {new Size(10, 10), new Size(10, -10)};
            yield return new object[] {new Size(15, 15), new Size(10, 0)};
            yield return new object[] {new Size(5, 20), new Size(-10, 10)};
        }
    }

    public class TgaImageUnitTest
    {
        private TgaImage _image;

        /// <summary>
        /// Init (RGBA) image with red square; (0x00_ff_00_00)
        /// </summary>
        private void InitImage(Size size)
        {
            _image = new TgaImage(size, TgaImage.Format.Rgba);
            for (int i = 0; i < _image.TgaHeader.Height; i++)
            {
                for (int j = 0; j < _image.TgaHeader.Width; j++)
                {
                    _image.SetPointColor(new Point(j, i), new TgaColor(0x00_ff_00_00));
                }
            }
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetValidTgaImageParams), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void TgaImage_ParamTest_Success(Size size, TgaImage.Format format)
        {
            _image = new TgaImage(size, format);
            Assert.Equal(_image.TgaFormat, format);
            Assert.Equal(_image.TgaHeader.Width, size.Width);
            Assert.Equal(_image.TgaHeader.Height, size.Height);
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetNotValidSizeTgaImageParams), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void TgaImage_NotValidSize_ArgumentException(Size size, TgaImage.Format format)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { _image = new TgaImage(size, format); });
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetNotValidFormatTgaImageParams), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void TgaImage_NotValidFormat_ArgumentException(Size size, TgaImage.Format format)
        {
            Assert.Throws<ArgumentException>(() => { _image = new TgaImage(size, format); });
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetValidHeaderTgaImage), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void TgaImage_ValidHeader_Success(TgaHeader header)
        {
            _image = new TgaImage(header);
            Assert.Equal(_image.TgaHeader, header);
        }

        [Fact]
        public void Clear_DataClear_Success()
        {
            InitImage(new Size(10, 10));
            _image.Clear();
            for (int i = 0; i < _image.TgaHeader.Height; i++)
            {
                for (int j = 0; j < _image.TgaHeader.Width; j++)
                {
                    TgaColor color = _image.GetPointColor(new Point(j, i));
                    Assert.Equal((uint)0, color.Color); //check color is zero(black)
                }
            }
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetValidPointSetPointColor), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void SetPointColor_ValidPoint_Success(Size size, Point point)
        {
            //inti empty image. Init with black square.
            var color = new TgaColor(0x00ff0000);
            _image = new TgaImage(size, TgaImage.Format.Rgba);
            _image.SetPointColor(point, color);
            Assert.Equal(_image.GetPointColor(point).Color, color.Color);
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetUnvalidPointSetPointColor), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void SetPointColor_NotValidPoint_ArgumentOutOfRangeException(Size size, Point point)
        {
            //inti empty image. Init with black square.
            _image = new TgaImage(size, TgaImage.Format.Rgba);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _image.SetPointColor(point, new TgaColor()); });
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetValidPointGetPointColor), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void GetPointColor_ValidPoint_Success(Size size, Point point)
        {
            InitImage(size);
            Assert.Equal(new TgaColor(0x00_ff_00_00).Color, _image.GetPointColor(point).Color);
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetUnvalidPointGetPointColor), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void GetPointColor_NotValidPoint_ArgumentOutOfRangeException(Size size, Point point)
        {
            InitImage(size);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _image.GetPointColor(point); });
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetValidSizeScale), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void Scale_ValidSize_Success(Size oldSize, Size newSize)
        {
            InitImage(oldSize);
            _image.Scale(newSize);
            Assert.Equal(_image.TgaHeader.Width, newSize.Width);
            Assert.Equal(_image.TgaHeader.Height, newSize.Height);
            //TODO: add check scale alhoritm... 
        }

        [Theory]
        [MemberData(nameof(TgaImageUnitTestDataGenerator.GetUnvalidSizeScale), MemberType =
            typeof(TgaImageUnitTestDataGenerator))]
        public void Scale_NotValidSize_ArgumentOutOfRangeException(Size oldSize, Size newSize)
        {
            InitImage(oldSize);
            Assert.Throws<ArgumentOutOfRangeException>(() => { _image.Scale(newSize); });
        }
    }
}