using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SoftRender.TGA
{
    public class TgaImage
    {
        private byte[] _data;

        private Size _size;

        private int _byteSpp;

        internal void LoadRleData(Stream iStream)
        {
            int pixelCount = _size.Height * _size.Width;
            int currentPixel = 0;
            int currentByte = 0;
            do
            {
                int chunkHeader = iStream.ReadByte();
                if (chunkHeader == -1)
                {
                    throw new EndOfStreamException("Stream is end.");
                }
                if (chunkHeader < 128)
                {
                    chunkHeader++;
                    for (int i = 0; i < chunkHeader; i++)
                    {
                        iStream.Read(_data, currentByte, _byteSpp);
                        currentByte += _byteSpp;
                        currentPixel++;
                        if (currentPixel > pixelCount)
                        {
                            throw new OutOfMemoryException("To meany pixel read.");
                        }
                    }
                }
                else
                {
                    chunkHeader -= 127;
                    var colorBuffer = new byte[_byteSpp];
                    iStream.Read(colorBuffer, 0, _byteSpp);
                    for (int i = 0; i < chunkHeader; i++)
                    {
                        Array.ConstrainedCopy(colorBuffer, 0, _data, currentByte, _byteSpp);
                        currentByte += _byteSpp;
                        currentPixel++;
                        if (currentPixel > pixelCount)
                        {
                            throw new OutOfMemoryException("To meany pixel read.");
                        }
                    }
                }
            } while (currentPixel > pixelCount);
        }

        internal void UnloadRleData(Stream oStream)
        {
            //TODO: add check stream
            int maxChankSize = 128;
            int pixelCount = _size.Height * _size.Width;
            int currentPixel = 0;
            while (currentPixel < pixelCount)
            {
                int chunkStart = currentPixel * _byteSpp;
                int currentByte = currentPixel * _byteSpp;
                byte runLenght = 1;
                bool raw = true;
                while (currentPixel + runLenght < pixelCount && runLenght < maxChankSize)
                {
                    bool succEq = true;
                    for (int i = 0; i < _byteSpp && succEq; i++)
                    {
                        succEq = _data[currentByte + i] == _data[currentByte + i + _byteSpp];
                    }
                    currentByte += _byteSpp;
                    if (runLenght == 1)
                    {
                        raw = !succEq;
                    }
                    if (raw && succEq)
                    {
                        runLenght--;
                        break;
                    }
                    if (!raw && !succEq)
                    {
                        break;
                    }
                    runLenght++;
                }
                currentPixel += runLenght;
                oStream.WriteByte((byte) (raw ? runLenght - 1 : runLenght + 127));
                oStream.Write(_data, chunkStart, (raw ? runLenght * _byteSpp : _byteSpp));
            }
        }

        public enum Format
        {
            None = 0,
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
            _data.Initialize();
            //TODO: check if _data init by 0.
        }

        internal bool IsFormatValid(Format format)
        {
            return (format != Format.Grayscale || format != Format.Rgb || format != Format.Rgba);
        }

        public TgaImage(string fileName)
        {
            _data = null; //reset data
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
                if (!fileStream.CanRead)
                {
                    //fileStream.Close();
                    throw new NotSupportedException("Not support read." + fileName);
                }
                //read tga image header
                TgaHeader tgaHeader;

                int headerSize = Marshal.SizeOf<TgaHeader>();
                var headerData = new byte[headerSize];
                fileStream.Read(headerData, 0, headerSize);
                IntPtr i = Marshal.AllocHGlobal(headerSize); //allocate memmory and get pointer
                Marshal.Copy(headerData, 0, i, headerSize); // copy data to memmory 
                tgaHeader = Marshal.PtrToStructure<TgaHeader>(i); //move memmory to structure
                Marshal.FreeHGlobal(i); //free memory

                _size.Height = tgaHeader.Height;
                _size.Width = tgaHeader.Width;
                _byteSpp = tgaHeader.BitSperPixel >> 3;

                if (_size.Width <= 0 || _size.Height <= 0 || !IsFormatValid((Format) _byteSpp))
                {
                    //fileStream.Close();
                    throw new FileLoadException("Invalid file. Header is not valid. ", fileName);
                }

                //read tga image data
                int rgaImgSize = _size.Width * _size.Height * _byteSpp;
                _data = new byte[rgaImgSize]; //init data
                if (tgaHeader.DataTypeCode == 2 || tgaHeader.DataTypeCode == 3)
                {
                    fileStream.Read(_data, 0, rgaImgSize);
                }
                else if (tgaHeader.DataTypeCode == 10 || tgaHeader.DataTypeCode == 11)
                {
                    LoadRleData(fileStream);
                }
                else
                {
                    //fileStream.Close();
                    throw new FileLoadException("Invalid file format", fileName);
                }
                if ((tgaHeader.ImageDescription & 0x20) == 0)
                {
                    FlipVertically();
                }
                if ((tgaHeader.ImageDescription & 0x10) != 0)
                {
                    FlipHorizontally();
                }
            }
            finally
            {
                fileStream.Close();
            }
        }

        public void WriteTgaFile(string fileName, bool rle = true)
        {
            byte[] developerAreaRef = {0, 0, 0, 0};
            byte[] extensionAreaRef = {0, 0, 0, 0};
            char[] footer = {'T', 'R', 'U', 'E', 'V', 'I', 'S', 'I', 'O', 'N', '-', 'X', 'F', 'I', 'L', 'E', '.', '\0'};

            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            try
            {
                if (!fileStream.CanWrite)
                {
                    throw new NotSupportedException("Not support wrtite " + fileName);
                }
                //Write header
                TgaHeader tgaHeader = new TgaHeader();

                tgaHeader.BitSperPixel = (byte) (_byteSpp << 3);
                tgaHeader.Width = (short) _size.Width;
                tgaHeader.Height = (short) _size.Height;
                tgaHeader.DataTypeCode =
                    (byte) ((Format) _byteSpp == Format.Grayscale ? (rle ? 11 : 3) : (rle ? 10 : 2));
                tgaHeader.ImageDescription = 0x20; //top-left origin

                int headerSize = Marshal.SizeOf<TgaHeader>();
                byte[] headerData = new byte[headerSize];
                IntPtr dataPrt = Marshal.AllocHGlobal(headerSize);
                Marshal.StructureToPtr(tgaHeader, dataPrt, true);
                Marshal.Copy(dataPrt, headerData, 0, headerSize);
                Marshal.FreeHGlobal(dataPrt);

                fileStream.Write(headerData, 0, headerSize);
                if (rle)
                {
                    UnloadRleData(fileStream);
                }
                else
                {
                    fileStream.Write(_data, 0, _size.Height * _size.Width * _byteSpp);
                }
                fileStream.Write(developerAreaRef, 0, developerAreaRef.Length);
                fileStream.Write(extensionAreaRef, 0, extensionAreaRef.Length);
                Encoding u8 = Encoding.UTF8;
                fileStream.Write(u8.GetBytes(footer), 0, u8.GetBytes(footer).Length);
            }
            finally
            {
                fileStream.Close();
            }
        }

        internal void CheckDataValid()
        {
            if (_data.Length == 0)
            {
                throw new OutOfMemoryException("Data is empty");
            }
        }

        public void FlipHorizontally()
        {
            CheckDataValid();
            int half = _size.Width >> 1;
            for (int i = 0; i < half; i++)
            {
                for (int j = 0; j < _size.Height; j++)
                {
                    Point p1 = new Point(i, j);
                    Point p2 = new Point(_size.Width - 1 - i, j);
                    TgaColor color1 = GetColorInPoint(p1);
                    TgaColor color2 = GetColorInPoint(p2);
                    SetPoint(p1, color1);
                    SetPoint(p2, color2);
                }
            }
        }

        public void FlipVertically()
        {
            CheckDataValid();
            int bytesPerLine = (_size.Width * _byteSpp);
            var line = new byte[bytesPerLine];
            int half = _size.Height >> 1;
            for (int i = 0; i < half; i++)
            {
                int l1 = bytesPerLine * i;
                int l2 = bytesPerLine * (_size.Height - 1 - i);
                Array.ConstrainedCopy(_data, l1, line, 0, bytesPerLine);
                Array.ConstrainedCopy(_data, l2, _data, l1, bytesPerLine);
                Array.ConstrainedCopy(line, 0, _data, l2, bytesPerLine);
            }
        }

        public void Scale(Size size)
        {
            
        }

        public void SetPoint(Point point, TgaColor color)
        {
            if (_data.IsReadOnly || _data.Length == 0 ||
                point.IsEmpty || point.X < 0 || point.Y < 0 ||
                point.X >= _size.Width || point.Y >= _size.Height)
            {
                throw new ArgumentOutOfRangeException("point " + point.ToString());
            }
            Array.ConstrainedCopy(color.GetRawColor(), 0, _data, point.X + point.Y * _size.Width, _byteSpp);
        }

        public TgaColor GetColorInPoint(Point point)
        {
            TgaColor color = new TgaColor();

            if (_data.IsReadOnly || _data.Length == 0 ||
                point.IsEmpty || point.X < 0 || point.Y < 0 ||
                point.X >= _size.Width || point.Y >= _size.Height)
            {
                throw new ArgumentOutOfRangeException("point " + point.ToString());
            }
            var bytes = new byte[_byteSpp];
            color.ByteSpp = _byteSpp;
            Array.ConstrainedCopy(_data, point.X + point.Y * _size.Width, bytes, 0, _byteSpp);
            color.SetRawColor(bytes);
            return color;
        }

        public void Clear()
        {
            _data.Initialize();
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