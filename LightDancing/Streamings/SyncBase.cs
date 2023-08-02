using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.OpenTKs;
using LightDancing.Syncs;
using System;
using System.Drawing;

namespace LightDancing.Streamings
{
    public class SyncBase : IStreaming
    {
        private readonly CaptureInfos _capture;
        private readonly MatrixLayouts _deviceLayouts;
        private readonly ISyncHelper _syncHelper;

        public SyncBase(CaptureInfos infos, MatrixLayouts layouts, ISyncHelper syncHelper)
        {
            _capture = infos;
            _deviceLayouts = layouts;
            _syncHelper = syncHelper;
        }

        /// <summary>
        /// If syncHelper.GetColors == null means this syncHelper returns color in Bitmap form, please call _syncHelper.GetBitmapForStreaming()
        /// </summary>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public ColorRGB[,] Process(float brightness)
        {
            var fullColors = _syncHelper.GetColors();
            if (fullColors != null)
            {
                return MappingDevice(fullColors, _capture, _deviceLayouts.Width, _deviceLayouts.Height, brightness);
            }
            else
            {
                var bitmap = _syncHelper.GetBitmapForStreaming();
                return MappingBitmapDevice(bitmap, _capture, _deviceLayouts.Width, _deviceLayouts.Height, brightness);
            }
        }

        /// <summary>
        /// Mapping the capture pixcels to devices of LEDs colors
        /// </summary>
        /// <param name="fullColors">The full martix color form OpenTK</param>
        /// <param name="infos">Capture info(start axis and x and y size)</param>
        /// <param name="logic">Capture logic</param>
        /// <param name="deviceWidth">Device X-Axis count</param>
        /// <param name="deviceHeight">Device Y-Axis count</param>
        /// /// <param name="brightness">brightness percentage</param>
        /// /// <param name="blueReduction">bool for blue reduction (set to 75%)</param>
        /// <returns></returns>
        protected ColorRGB[,] MappingDevice(ColorRGB[,] fullColors, CaptureInfos infos, int deviceWidth, int deviceHeight, float brightness)
        {
            ColorRGB[,] result = new ColorRGB[deviceHeight, deviceWidth];

            if (fullColors == null)
            {
                for (int y = 0; y < _deviceLayouts.Height; y++)
                {
                    for (int x = 0; x < _deviceLayouts.Width; x++)
                    {
                        result[y, x] = ColorRGB.Black();
                    }
                }

                return result;
            }

            
            var captureColors = GetCaptureColors(infos, fullColors);
            double captureWidth = captureColors.GetLength(1);
            double captureHeight = captureColors.GetLength(0);

            double widthRange = captureWidth / deviceWidth;
            double heightRange = captureHeight / deviceHeight;

            double halfWidth = widthRange / 2;
            double halfHeight = heightRange / 2;

            for (int y = 0; y < deviceHeight; y++)
            {
                for (int x = 0; x < deviceWidth; x++)
                {
                    int xAxis = (int)((x * widthRange) + halfWidth);
                    int yAxis = (int)((y * heightRange) + halfHeight);

                    var color = captureColors[yAxis, xAxis];
                    result[y, x] = new ColorRGB((byte)(color.R * brightness), (byte)(color.G * brightness), (byte)(color.B * brightness));
                }
            }

            return result;
            
        }

        /// <summary>
        /// According to the capture's info to get the matrix colors from full colors.
        /// </summary>
        /// <param name="infos">Capture infos</param>
        /// <param name="fullColors">OpenTK full window of colors</param>
        /// <returns>Capture of matrix colors</returns>
        private ColorRGB[,] GetCaptureColors(CaptureInfos infos, ColorRGB[,] fullColors)
        {
            ColorRGB[,] result = new ColorRGB[infos.Layouts.Height, infos.Layouts.Width];
            int startXaxis = infos.StartAxis.Item1;
            int startYaxis = infos.StartAxis.Item2;

            int endXaxis = startXaxis + infos.Layouts.Width;
            int endYaxis = startYaxis + infos.Layouts.Height;

            for (int yIndex = startYaxis; yIndex < endYaxis; yIndex++)
            {
                for (int xIndex = startXaxis; xIndex < endXaxis; xIndex++)
                {
                    if (yIndex < fullColors.GetLength(0) && xIndex < fullColors.GetLength(1))
                    {
                        result[yIndex - startYaxis, xIndex - startXaxis] = fullColors[yIndex, xIndex];
                    }
                    else
                    {
                        result[yIndex - startYaxis, xIndex - startXaxis] = ColorRGB.Black();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// According to the capture's info to get the matrix colors from full colors bitmap.
        /// </summary>
        /// <param name="fullColors">fullcolors in bitmap</param>
        /// <param name="infos">Capture infos</param>
        /// <param name="deviceWidth"></param>
        /// <param name="deviceHeight"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        protected ColorRGB[,] MappingBitmapDevice(Bitmap fullColors, CaptureInfos infos, int deviceWidth, int deviceHeight, float brightness)
        {
            ColorRGB[,] result = new ColorRGB[deviceHeight, deviceWidth];

            if (fullColors == null)
            {
                for (int y = 0; y < _deviceLayouts.Height; y++)
                {
                    for (int x = 0; x < _deviceLayouts.Width; x++)
                    {
                        result[y, x] = ColorRGB.Black();
                    }
                }

                return result;
            }

            var captureColors = GetBitmapColors(infos, fullColors);
            double captureWidth = captureColors.GetLength(1);
            double captureHeight = captureColors.GetLength(0);

            double widthRange = captureWidth / deviceWidth;
            double heightRange = captureHeight / deviceHeight;

            double halfWidth = widthRange / 2;
            double halfHeight = heightRange / 2;

            for (int y = 0; y < deviceHeight; y++)
            {
                for (int x = 0; x < deviceWidth; x++)
                {
                    int xAxis = (int)((x * widthRange) + halfWidth);
                    int yAxis = (int)((y * heightRange) + halfHeight);

                    var color = captureColors[yAxis, xAxis];
                    result[y, x] = new ColorRGB((byte)(color.R * brightness), (byte)(color.G * brightness), (byte)(color.B * brightness));
                }
            }

            return result;
        }

        private ColorRGB[,] GetBitmapColors(CaptureInfos infos, Bitmap fullColors)
        {
            ColorRGB[,] result = new ColorRGB[infos.Layouts.Height, infos.Layouts.Width];
            int startXaxis = infos.StartAxis.Item1;
            int startYaxis = infos.StartAxis.Item2;

            int endXaxis = startXaxis + infos.Layouts.Width;
            int endYaxis = startYaxis + infos.Layouts.Height;

            try
            {
                for (int yIndex = startYaxis; yIndex < endYaxis; yIndex++)
                {
                    for (int xIndex = startXaxis; xIndex < endXaxis; xIndex++)
                    {
                        if (yIndex < fullColors.Height && xIndex < fullColors.Width)
                        {
                            Color color = fullColors.GetPixel(xIndex, yIndex);
                            result[yIndex - startYaxis, xIndex - startXaxis] = new ColorRGB(color.R, color.G, color.B);
                        }
                        else
                        {
                            result[yIndex - startYaxis, xIndex - startXaxis] = ColorRGB.Black();
                        }
                    }
                }
            }
            catch { }

            return result;
        }
    }
}
