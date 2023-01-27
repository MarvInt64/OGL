using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace OGL.Core
{
    public static class ImageHelper
    {
        public static void SavePng(string filePath, Span<byte> data, int width, int height)
        {
            using (Image image = Image.LoadPixelData<Rgb24>(data, width, height))
            {
              image.SaveAsPng(filePath);
            }
        }
    }
}