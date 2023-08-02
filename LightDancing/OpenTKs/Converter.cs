using LightDancing.Colors;
using LightDancing.Common;
using System;

namespace LightDancing.OpenTKs
{
    public class Converter
    {
        private readonly int _xCounts = 0;
        private readonly int _yCounts = 0;
        private readonly int _totalBytes = 0;

        /// <summary>
        /// Convet the color from Opentk
        /// </summary>
        /// <param name="xCounts">The X-Axis size</param>
        /// <param name="yCounts">The Y-Axis size</param>
        /// <param name="logic">The logic of caputre color</param>
        public Converter(int xCounts, int yCounts)
        {
            _xCounts = xCounts;
            _yCounts = yCounts;
            _totalBytes = xCounts * yCounts * ConstValues.RGBA_BYTE; // 4 = RGBA
        }

        /// <summary>
        /// Convert the byte from the Opentk to Led devices
        /// </summary>
        /// <param name="receiveBytes">All colors bytes, each color = 4 bytes(RGBA)</param>
        /// <returns>The colors of matrix (xAxisCount * yAxisCount)</returns>
        public ColorRGB[,] Convert2Colors(byte[] receiveBytes)
        {
            if (receiveBytes.Length == _totalBytes)
            {
                ColorRGB[,] result = new ColorRGB[_yCounts, _xCounts];

                for (int yIndex = 0; yIndex < _yCounts; yIndex++)
                {
                    for (int xIndex = 0; xIndex < _xCounts; xIndex++)
                    {
                        int startIndex = (_xCounts * yIndex) + xIndex;
                        int index = startIndex * ConstValues.RGBA_BYTE;
                        var color = new ColorRGB(receiveBytes[index], receiveBytes[index + 1], receiveBytes[index + 2]);
                        result[_yCounts - 1 - yIndex, xIndex] = color;
                    }
                }

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format("The Argument of length: {0}, LEDs total bytes: {1}",
                    receiveBytes.Length, _totalBytes));
            }
        }
    }

    public class CaptureInfos
    {
        /// <summary>
        /// Item1 = X-Axis, Item2 = Y-Axis
        /// </summary>
        public Tuple<int, int> StartAxis { get; set; }

        /// <summary>
        /// The device of LEDs layout.
        /// </summary>
        public MatrixLayouts Layouts { get; set; }
    }
}
