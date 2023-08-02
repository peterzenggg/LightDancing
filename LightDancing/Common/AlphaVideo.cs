using LightDancing.Colors;
using LightDancing.Syncs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LightDancing.Common
{
    public class AlphaVideo
    {
        public static AlphaVideo Instance => lazy.Value;
        private static readonly Lazy<AlphaVideo> lazy = new Lazy<AlphaVideo>(() => new AlphaVideo());
        public List<Bitmap> Bitmaps { get; set; } = new List<Bitmap>();
        private int _count = 0;
        private ISyncHelper _currentSyncHelper;
        public Bitmap ResultBitmap { get; set; }

        public Action Updated { get ; set; }

        public AlphaVideo() 
        {
        }

        public void Initialize(string folderPath)
        {
            GetFishyBitmaps(folderPath);
        }

        private void GetFishyBitmaps(string folderPath)
        {
            for (int i = 1; i < 361; i++)
            {
                string filePath = folderPath;
                if (i < 10)
                {
                    filePath = filePath + "00" + i + ".png";
                }
                else if (i < 100)
                {
                    filePath = filePath + "0" + i + ".png";
                }
                else
                {
                    filePath = filePath + i + ".png";
                }
                using Image image = Image.FromFile(filePath);
                Bitmaps.Add(new Bitmap(image, image.Width / 2, image.Height / 2));
            }
        }

        public void SetupSyncWithLCD(ISyncHelper syncHelper)
        {
            _currentSyncHelper = syncHelper;
            _currentSyncHelper.ScreenUpdated += GetPreviewImages;
        }

        private void GetPreviewImages()
        {
            ColorRGB[,] preview = _currentSyncHelper.GetColors();
            if (preview == null)
            {
                using Bitmap bitmap = _currentSyncHelper.GetBitmapForAlphaVideo();
                SetResultBitmap(bitmap);
            }
            else
            {
                using Bitmap bitmap = GetBitmapFromRGB(preview);
                SetResultBitmap(bitmap);
            }
            Updated?.Invoke();
            GC.Collect();
        }

        private void SetResultBitmap(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                _count++;
                if (_count >= Bitmaps.Count)
                {
                    _count = 0;
                }

                using Bitmap resultBitmap = new Bitmap(Bitmaps[_count].Width, Bitmaps[_count].Height);

                using (Graphics g = Graphics.FromImage(resultBitmap))
                {
                    using (var bitmapFirstLayer = new Bitmap(bitmap, Bitmaps[_count].Width, Bitmaps[_count].Height))
                    {
                        // Draw the first bitmap onto the result bitmap
                        g.DrawImage(bitmapFirstLayer, 0, 0);
                    }

                    // Set the alpha blending mode to combine the images
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                    // Draw the second bitmap onto the result bitmap with transparency
                    g.DrawImage(Bitmaps[_count], 0, 0);
                }

                ResultBitmap?.Dispose();
                ResultBitmap = new Bitmap(resultBitmap);
            }
        }

        private Bitmap GetBitmapFromRGB(ColorRGB[,] colors) 
        {
            int width = colors.GetLength(1);
            int height = colors.GetLength(0);

            byte[] buffer = new byte[width * height * 4];

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[index] = colors[y, x].B;
                    buffer[index + 1] = colors[y, x].G;
                    buffer[index + 2] = colors[y, x].R;
                    buffer[index + 3] = 255;
                    index += 4;
                }
            }

            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            Bitmap bitmap = new Bitmap(width, height, width * 4,
                       System.Drawing.Imaging.PixelFormat.Format32bppRgb, ptr);
            return bitmap;
        }

        public string GetBase64Srring()
        {
            return Methods.ColorRgbToBase64(ResultBitmap);
        }
    }
}
