using System;
using SoftRender.TGA;
using Xunit;

namespace SoftRenderUnitTest.Tga
{
    public class TgaColorUnitTest
    {
        protected TgaColor Color;

        [Theory]
        [InlineData(0xfa_ff_aa_dd)]
        [InlineData(0xff_43_44_11)]
        public void GetRawColor_MatchingWithСolor_Success(uint color)
        {
            Color = new TgaColor(color);
            var rawColor = Color.GetRawColor();
            Xunit.Assert.Equal(rawColor, BitConverter.GetBytes(color));
        }


        [Theory]
        [InlineData(new byte[] {1, 2, 3, 4})]
        [InlineData(new byte[] {4, 65, 123, 42})]
        [InlineData(new byte[] {255, 23, 63, 76})]
        public void SetRawColor_MatchingWithСolor_Success(byte[] rawColor)
        {
            Color = new TgaColor();
            Color.SetRawColor(rawColor);
            Xunit.Assert.Equal(rawColor, Color.GetRawColor());
        }

        [Theory]
        [InlineData(new byte[] {1, 2, 3, 4, 14, 22, 32})]
        [InlineData(new byte[] {4, 65, 123, 42, 56, 2, 12})]
        [InlineData(new byte[] {4, 65})]
        [InlineData(new byte[] {255, 23, 63, 76, 32, 12, 5})]
        public void SetRawColor_OutOfRange_Exception(byte[] rawColor)
        {
            Color = new TgaColor();
            Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => { Color.SetRawColor(rawColor); });
        }

        [Theory]
        [InlineData(0xf0_ff_aa_3c)]
        [InlineData(0x1f_43_42_24)]
        [InlineData(0x00_43_c2_6e)]
        public void GetAChanel_MatchingWithСolor_Success(uint color)
        {
            Color = new TgaColor(color);
            byte colorChanel = (BitConverter.GetBytes(color)[3]);
            Xunit.Assert.Equal(colorChanel, Color.GetACanel());
        }

        [Theory]
        [InlineData(0xf0_ff_a1_c0)]
        [InlineData(0x1f_43_44_23)]
        [InlineData(0x00_43_12_6d)]
        public void GetRChanel_MatchingWithСolor_Success(uint color)
        {
            Color = new TgaColor(color);
            byte colorChanel = (BitConverter.GetBytes(color)[2]);
            Xunit.Assert.Equal(colorChanel, Color.GetRCanel());
        }

        [Theory]
        [InlineData(0xf0_ff_0a_1c)]
        [InlineData(0x1f_37_48_c7)]
        [InlineData(0x00_43_c2_ad)]
        public void GetGChanel_MatchingWithСolor_Success(uint color)
        {
            Color = new TgaColor(color);
            byte colorChanel = (BitConverter.GetBytes(color)[1]);
            Xunit.Assert.Equal(colorChanel, Color.GetGCanel());
        }

        [Theory]
        [InlineData(0xf0_ff_a4_3c)]
        [InlineData(0x1f_ac_4f_2e)]
        [InlineData(0x00_43_c2_6d)]
        public void GetBChanel_MatchingWithСolor_Success(uint color)
        {
            Color = new TgaColor(color);
            byte colorChanel = (BitConverter.GetBytes(color)[0]);
            Xunit.Assert.Equal(colorChanel, Color.GetBCanel());
        }


        [Theory]
        [InlineData(124)]
        [InlineData(37)]
        [InlineData(18)]
        [InlineData(84)]
        public void SetAChanel_MatchingWithСolorChanel_Success(byte colorChanel)
        {
            Color = new TgaColor();
            Color.SetAChanel(colorChanel);

            Xunit.Assert.Equal(colorChanel, Color.GetACanel());
        }

        [Theory]
        [InlineData(124)]
        [InlineData(254)]
        [InlineData(200)]
        [InlineData(201)]
        public void SetRChanel_MatchingWithСolorChanel_Success(byte colorChanel)
        {
            Color = new TgaColor();
            Color.SetRChanel(colorChanel);

            Xunit.Assert.Equal(colorChanel, Color.GetRCanel());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(7)]
        [InlineData(211)]
        [InlineData(18)]
        public void SetGChanel_MatchingWithСolorChanel_Success(byte colorChanel)
        {
            Color = new TgaColor();
            Color.SetGChanel(colorChanel);

            Xunit.Assert.Equal(colorChanel, Color.GetGCanel());
        }

        [Theory]
        [InlineData(23)]
        [InlineData(76)]
        [InlineData(91)]
        [InlineData(26)]
        public void SetBChanel_MatchingWithСolorChanel_Success(byte colorChanel)
        {
            Color = new TgaColor();
            Color.SetBChanel(colorChanel);

            Xunit.Assert.Equal(colorChanel, Color.GetBCanel());
        }
    }
}