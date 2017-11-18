using System;
using System.Drawing;
using SoftRender.TGA;

namespace SoftRender
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string filePath = "testImage/xing_t32.tga";
            string outFile = "testImage/oxing_t32.tga";
            string outScaleFile = "testImage/oSxing_t32.tga";

            TgaImage image = new TgaImage(filePath);
            Console.WriteLine("File load");
            image.WriteTgaFile(outFile);
            Console.WriteLine("new file unload");
            Console.WriteLine("Scaled");
            image.Scale(new Size(510,510));
            image.WriteTgaFile(outScaleFile);
            Console.WriteLine("new scaled file unload");
        }
    }
}