using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SoftRender.TGA
{
    /// <summary>
    /// TODO: add color handle relative by Format (Rgb|Rgba|Grayscale) 
    /// </summary>
    public class TgaImage
    {
        public enum Format : uint
        {
            None = 0,
            Grayscale = 1,
            Rgb = 3,
            Rgba = 4
        } //maybe can be byte 

        private byte[] _data;

        private TgaHeader _header;

        public Format TgaFormat { get; }

        public TgaHeader TgaHeader
        {
            get => _header;
        }

        protected void ValidateSize(Size size)
        {
            if (size.Height <= 0 || size.Height > UInt16.MaxValue ||
                size.Width <= 0 || size.Width > UInt16.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public TgaImage(Size size, Format format)
        {
            ValidateSize(size);
            if (format == Format.None)
            {
                throw new ArgumentException();
            }
            TgaFormat = format;
            _header = new TgaHeader();
            _header.Width = (short) size.Width;
            _header.Height = (short) size.Height;
            _header.BitSperPixel = (byte) ((uint) format << 3);
            _header.ImageDescription = (byte) (format == Format.Grayscale ? 2 : 3); //withoute compression by deffault
            _header.DataTypeCode = 0x20;
            _data = new byte[_header.Width * _header.Height * (uint) TgaFormat];
            _data.Initialize();
        }

        public TgaImage(TgaHeader header)
        {
            _header = header;
            TgaFormat = (Format) (header.BitSperPixel >> 3);

            _data = new byte[_header.Width * _header.Height * (uint) TgaFormat];
            _data.Initialize();
        }

        public void Clear()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = 0;
            }
        }

        public void SetPointColor(Point point, TgaColor color)
        {
            if (point.X < 0 || point.X >= _header.Width ||
                point.Y < 0 || point.Y >= _header.Height)
            {
                throw new ArgumentOutOfRangeException();
            }
            Array.ConstrainedCopy(color.GetRawColor(), 0, _data,
                (point.X + (point.Y * _header.Width)) * (int) TgaFormat,
                (int) TgaFormat);
        }

        public TgaColor GetPointColor(Point point)
        {
            if (point.X < 0 || point.X >= _header.Width ||
                point.Y < 0 || point.Y >= _header.Height)
            {
                throw new ArgumentOutOfRangeException();
            }
            var rawColor = new byte[(int) TgaFormat];

            Array.ConstrainedCopy(_data, (point.X + (point.Y * _header.Width)) * (int) TgaFormat, rawColor, 0,
                (int) TgaFormat);
            return new TgaColor(rawColor);
        }

        public void Scale(Size size)
        {
            ValidateSize(size);
            var newData = new byte[size.Height * size.Width * (int) TgaFormat];

            int newScanLine = 0;
            int oldScanLine = 0;
            int errY = 0;
            int newLineBytes = size.Width * (int) TgaFormat;
            int oldLineBytes = _header.Width * (int) TgaFormat;
            for (int i = 0; i < _header.Height; i++)
            {
                int errX = _header.Width - size.Width;
                int newX = -(int) TgaFormat;
                int oldX = -(int) TgaFormat;
                for (int j = 0; j < _header.Width; j++)
                {
                    oldX += (int) TgaFormat;
                    errX += size.Width;
                    while (errX >= _header.Width)
                    {
                        errX -= _header.Width;
                        newX += (int) TgaFormat;
                        Array.ConstrainedCopy(_data, oldScanLine + oldX, newData, newScanLine + newX, (int) TgaFormat);
                    }
                }
                errY += size.Height;
                oldScanLine += oldLineBytes;
                while (errY >= _header.Height)
                {
                    if (errY >= (int) _header.Height << 1)
                    {
                        Array.ConstrainedCopy(newData, newScanLine, newData, newLineBytes + newScanLine, newLineBytes);
                    }
                    errY -= _header.Height;
                    newScanLine += newLineBytes;
                }
            }
            _data = newData;
            _header.Width = (short) size.Width;
            _header.Height = (short) size.Height;
        }
    }

    [System.Obsolete("Class is deprecated. Use TgaImage.", true)]
    public class TgaImage1
    {
        private byte[] _data;

        private Size _size;

        private int _byteSpp;

        protected void LoadRleData(Stream iStream)
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

        protected void UnloadRleData(Stream oStream)
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

        public enum Format : uint
        {
            None = 0,
            Grayscale = 1,
            Rgb = 3,
            Rgba = 4
        };

        protected static bool IsFormatValid(Format format)
        {
            return (format != Format.Grayscale || format != Format.Rgb || format != Format.Rgba);
        }

        //TODO: move to another class
        public TgaImage1(string fileName)
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

        //TODO: move to another class
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

        protected void CheckDataValid()
        {
            if (_data.Length == 0)
            {
                throw new OutOfMemoryException("Data is empty");
            }
        }

        protected void FlipHorizontally()
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

        protected void FlipVertically()
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

        protected ref byte[] GetData()
        {
            return ref _data;
        }

        public TgaImage1(Size size, int byteSpp)
        {
            _size = size;
            _byteSpp = byteSpp;
            var nBytes = (ulong) (size.Width * size.Height * byteSpp);
            _data = new byte[nBytes];
            _data.Initialize();
        }

        public void Scale(Size size)
        {
            if (_size.Width <= 0 || _size.Height <= 0 || _data.Length == 0)
            {
                throw new OutOfMemoryException("data is empty");
            }
            var data = new byte[size.Height * size.Width * _byteSpp];

            int newScanLine = 0;
            int oldScanLine = 0;
            int errY = 0;
            int newLineBytes = size.Width * _byteSpp;
            int oldLineBytes = _size.Width * _byteSpp;
            for (int i = 0; i < _size.Height; i++)
            {
                int errX = _size.Width - size.Width;
                int newX = -_byteSpp;
                int oldX = -_byteSpp;
                for (int j = 0; j < _size.Width; j++)
                {
                    oldX += _byteSpp;
                    errX += size.Width;
                    while (errX >= _size.Width)
                    {
                        errX -= _size.Width;
                        newX += _byteSpp;
                        Array.ConstrainedCopy(_data, oldScanLine + oldX, data, newScanLine + newX, _byteSpp);
                    }
                }
                errY += size.Height;
                oldScanLine += oldLineBytes;
                while (errY >= _size.Height)
                {
                    if (errY >= (int) _size.Height << 1)
                    {
                        Array.ConstrainedCopy(data, newScanLine, data, newLineBytes + newScanLine, newLineBytes);
                    }
                    errY -= _size.Height;
                    newScanLine += newLineBytes;
                }
            }
            _data = data;
            _size = size;
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
            Array.ConstrainedCopy(_data, point.X + point.Y * _size.Width, bytes, 0, _byteSpp);
            color.SetRawColor(bytes);
            return color;
        }

        public void Clear()
        {
            _data.Initialize();
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