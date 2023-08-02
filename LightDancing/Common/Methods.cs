using LightDancing.Colors;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using LightDancing.Enums;

namespace LightDancing.Common
{
    public class Methods
    {
        /// <summary>
        /// Convert the full colorMatrix to fits layouts's colors
        /// </summary>
        /// <param name="colorMatrix">Full colors</param>
        /// <param name="layouts">Device layouts</param>
        /// <returns>Layout's colors</returns>
        public static ColorRGB[,] Convert2LayoutColors(ColorRGB[,] colorMatrix, MatrixLayouts layouts, float _brightness)
        {
            ColorRGB[,] result = new ColorRGB[layouts.Height, layouts.Width];
            int yInterval = colorMatrix.GetLength(0) / layouts.Height;
            int xInterval = colorMatrix.GetLength(1) / layouts.Width;

            for (int y = 0; y < layouts.Height; y++)
            {
                for (int x = 0; x < layouts.Width; x++)
                {
                    int yIndex = yInterval * y;
                    int xIndex = xInterval * x;

                    ColorRGB thisColor = colorMatrix[yIndex, xIndex];
                    result[y, x] = new ColorRGB((byte)(thisColor.R * _brightness), (byte)(thisColor.G * _brightness), (byte)(thisColor.B * _brightness));
                }
            }

            return result;
        }

        /// <summary>
        /// 1. Convert ColorRGB[,] into Bitmap
        /// 2. Convert Bitmap to base64
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static string ColorRgbToBase64(ColorRGB[,] colors)
        {
            if (colors != null)
            {
                int width = colors.GetLength(1);
                int height = colors.GetLength(0);

                byte[] buffer = new byte[width * height * 3];

                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        buffer[index] = colors[y, x].B;
                        buffer[index + 1] = colors[y, x].G;
                        buffer[index + 2] = colors[y, x].R;
                        index += 3;
                    }
                }

                ImageData imageData = new ImageData()
                {
                    Width = width,
                    Height = height,
                    PixelFormat = PixelFormat.Format24bppRgb,
                    PixelData = buffer,
                };

                Bitmap image = (Bitmap)CreateImageFromImageData(imageData);
                string base64ImageString = ToBase64String(image, ImageFormat.Png);
                return base64ImageString;
            }

            return null;
        }

        /// <summary>
        /// Convert bitmap to base64 string
        /// </summary>
        /// <param name="bitmap">Bitmap result from syncHelper</param>
        /// <returns></returns>
        public static string ColorRgbToBase64(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                string base64ImageString = ToBase64String(bitmap, ImageFormat.Png);
                return base64ImageString;
            }

            return null;
        }

        static public Image CreateImageFromImageData(ImageData imageData)
        {
            Bitmap bitmap = new Bitmap(imageData.Width, imageData.Height, imageData.PixelFormat);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var rowLength = imageData.Width * Image.GetPixelFormatSize(imageData.PixelFormat) / 8;
            var ptr = data.Scan0;
            for (var i = 0; i < imageData.Height; i++)
            {
                Marshal.Copy(imageData.PixelData, i * rowLength, ptr, rowLength);
                ptr += data.Stride;
            }
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static string ToBase64String(Bitmap bmp, ImageFormat imageFormat)
        {
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);
            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();
            memoryStream.Close();
            string base64String = Convert.ToBase64String(byteBuffer);

            return base64String;
        }

        public static byte CalculateRazerAccessByte(byte[] result)
        {
            int num = 0;
            for (int i = 3; i < 89; i++)
            {
                num ^= result[i];
            }

            return (byte)num;
        }
        
        public static string ByteToHexString(byte inputByte)
        {
            return inputByte.ToString("X2");
        }

        public static int MaxTwoDigits(int number)
        {
            if (number < 0)
            {
                throw new ArgumentException("Number should be non-negative.");
            }
            string numberString = number.ToString();
            if (numberString.Length <= 2)
            {
                return number;
            }
            else
            {
                string firstTwoDigits = numberString.Substring(0, 2);
                return int.Parse(firstTwoDigits);
            }
        }

        public static (int, int) ReduceToTwoDigits(int number)
        {
            if (number < 0)
            {
                throw new ArgumentException("Number should be non-negative.");
            }

            int divideCount = 0;
            while (number >= 100)
            {
                number /= 10;
                divideCount++;
            }

            return ((int)Math.Pow(10, divideCount), number);
        }

    }

    public class ImageData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public PixelFormat PixelFormat { get; set; }
        public byte[] PixelData { get; set; }
    }

    public class LayoutModel
    {
        public Keyboard LED;
        public int PosistionY;
        public int PosistionX;
    }
}
