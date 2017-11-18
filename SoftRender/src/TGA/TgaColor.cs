using System;

namespace SoftRender.TGA
{
    public class TgaColor
    {
        public int ByteSpp
        {
            get => ByteSpp;
            set => ByteSpp = value;
        }

        public uint Color
        {
            get => Color;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                Color = value;
            }
        }

        public TgaColor(uint color = 0, int byteSpp = 1)
        {
            ByteSpp = byteSpp;
            Color = color;
        }

        public TgaColor(int byteSpp, byte rChanel, byte gChanel, byte bChanel, byte aChanel)
        {
            ByteSpp = byteSpp;
            //TODO: add color init
        }
 
        public TgaColor(TgaColor color)
        {
            ByteSpp = color.ByteSpp;
            Color = color.Color;
        }

        public void SetRawColor(byte[] rawColor)
        {
            //TODO: realise set RawColor method
        }
       
        public byte[] GetRawColor()
        {
            var rawColor = new byte[4];

            //fil rawColor by chanel;
            rawColor[0] = GetBCanel();
            rawColor[1] = GetGCanel();
            rawColor[2] = GetRCanel();
            rawColor[3] = GetACanel();
            return rawColor;
        }

        public byte GetRCanel()
        {
            var rChanel = (byte)((Color & 0xff_00_00) >> 16);;
            return rChanel;
        }

        public byte GetGCanel()
        {
            var gChanel = (byte)((Color & 0xff_00) >> 8);
            //TODO: get G color chanel;
            return gChanel;
        }

        public byte GetBCanel()
        {
            var bChanel = (byte)((Color & 0xff) >> 0);           
            return bChanel;
        }
        
        public byte GetACanel()
        {
            var aChanel = (byte)((Color & 0xff_00_00_00) >> 32);;
            return aChanel;
        }
 
        public void SetRChanel(byte rChanel)
        {
            Color = ((uint)(Color & (~0xff_00_00)) | (uint)(rChanel << 16));
        }
        
        public void SetGChanel(byte gChanel)
        {
            Color = ((uint)(Color & (~0xff_00)) | (uint)(gChanel << 8));
        }
        
        public void SetBChanel(byte bChanel)
        {
            Color = ((uint)(Color & (~0xff)) | (uint)(bChanel << 0));
        }
        
        public void SetAChanel(byte aChanel)
        {
            Color = ((Color & (~0xff_00_00_00)) | (uint)(aChanel << 24));  
        }
    }
}