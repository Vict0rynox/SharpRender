using System;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;

namespace SoftRender.TGA
{
    public class TgaImage
    {
        private byte[] _data;

        private Size _size;

        private int _byteSpp;

        bool LoadRleData(Stream iStream)
        {
            //TODO: realize method;
            return false;
        }

        bool UnloadRleData(Stream oStream)
        {
            //TODO: realize method;
            return false;
        }

        public enum Format
        {
            Grayscale = 1,
            Rgb = 3,
            Rgba = 4
        };

        public TgaImage(Size size, int byteSpp)
        {
            _size = size;
            _byteSpp = byteSpp;
            var nBytes = (ulong) (size.Width * size.Height * byteSpp);
            _data = new byte[nBytes];
            //TODO: check if _data init by 0.
        }

        bool ReadTgaFile(string fileName)
        {
            //TODO: read tga file
            return false;
        }

        bool WriteTgaFile(string fileName, bool rle = true)
        {
            //TODO: write tga file
            return false;
        }

        bool FlipHorizontally()
        {
            //TODO: flip horizontally
            return false;
        }

        bool FlipVertically()
        {
            //TODO: flip vertically; 
            return false;
        }

        bool Scale(Size size)
        {
            //TOOD: scale to new size
            return false;
        }

        void SetPoint(Point point, TgaColor color)
        {
            if (_data.IsReadOnly || _data.Length == 0 ||
                point.IsEmpty || point.X < 0 || point.Y < 0 ||
                point.X >= _size.Width || point.Y >= _size.Height)
            {
                throw new ArgumentOutOfRangeException("point " + point.ToString());
            }
            for (int i = 0; i < _byteSpp; i++)
            {
                _data[i + point.X + point.Y * _size.Width] = color.GetRawColor()[i];
            }
        }

        TgaColor GetColorInPoint(Point point)
        {
            TgaColor color = new TgaColor();

            if (_data.IsReadOnly || _data.Length == 0 ||
                point.IsEmpty || point.X < 0 || point.Y < 0 ||
                point.X >= _size.Width || point.Y >= _size.Height)
            {
                throw new ArgumentOutOfRangeException("point " + point.ToString());
            }
            var bites = new byte[_byteSpp];
            color.ByteSpp = _byteSpp;
            for (int i = 0; i < _byteSpp; i++)
            {
                bites[i] = (_data[i + point.X + point.Y * _size.Width]);
            }
            color.SetRawColor(bites);
            return color;
        }

        void Clear()
        {
            //TODO: clear image
        }

        public ref byte[] GetData()
        {
            return ref _data;
        }

        public Size GetSize()
        {
            return _size;
        }

        public int GetByteSpp()
        {
            return _byteSpp;
        }
    }
}