using System;

namespace SoftRender.TGA
{
    /// <summary>
    /// TODO: add compare operator overload. 
    /// TODO: add basic color contant|enum  
    /// </summary>
    public class TgaColor
    {        
        private uint _color;

        public uint Color
        {
            get => _color;
            set => _color = value;
        }

        public TgaColor(uint color = 0)
        {
            _color = color;
        }

        public TgaColor(byte[] rawColor)
        {
            if (rawColor.Length > 4)
            {
                throw new ArgumentOutOfRangeException();
            }
            SetRawColor(rawColor);
        }

        public TgaColor(byte rChanel, byte gChanel, byte bChanel, byte aChanel)
        {
            var rawColor = new byte[4]{aChanel,rChanel,gChanel,bChanel};
            SetRawColor(rawColor);
        }
 
        public TgaColor(TgaColor color)
        {
            _color = color._color;
        }

        public void SetRawColor(byte[] rawColor)
        {
            if (rawColor.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(rawColor));
            }
            _color = BitConverter.ToUInt32(rawColor, 0);
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

        public byte GetACanel()
        {
            var aChanel = (byte)((_color & 0xff_00_00_00) >> 24);;
            return aChanel;
        }
        
        public byte GetRCanel()
        {
            var rChanel = (byte)((_color & 0xff_00_00) >> 16);;
            return rChanel;
        }

        public byte GetGCanel()
        {
            var gChanel = (byte)((_color & 0xff_00) >> 8);
            //TODO: get G color chanel;
            return gChanel;
        }

        public byte GetBCanel()
        {
            var bChanel = (byte)((_color & 0xff) >> 0);           
            return bChanel;
        }
 
        public void SetAChanel(byte aChanel)
        {
            _color = ((uint)(_color & (~0xff_00_00_00)) | (uint)(aChanel << 24));  
        }
        
        public void SetRChanel(byte rChanel)
        {
            _color = ((uint)(_color & (~0xff_00_00)) | (uint)(rChanel << 16));
        }
        
        public void SetGChanel(byte gChanel)
        {
            _color = ((uint)(_color & (~0xff_00)) | (uint)(gChanel << 8));
        }
        
        public void SetBChanel(byte bChanel)
        {
            _color = ((uint)(_color & (~0xff)) | (uint)(bChanel << 0));
        }       
    }
}