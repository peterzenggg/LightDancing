using LightDancing.Colors;
using System.Collections.Generic;

namespace LightDancing.Streamings
{
    public class Rainbow : IStreaming
    {
        private const float CIRCUMFERENCE = 360f;

        private readonly int rowsCount;
        private readonly int columnsCount;
        private readonly int lastColumnIndex;
        private readonly List<ColorRGB> layouts = new List<ColorRGB>();

        /// <summary>
        /// The matrix of size rowsCount * colunmCount
        /// </summary>
        /// <param name="rowsCount">The rows count of LEDs</param>
        /// <param name="columnsCount">The columns count of LEDs</param>
        public Rainbow(int rowsCount, int columnsCount)
        {
            this.rowsCount = rowsCount;
            this.columnsCount = columnsCount;
            lastColumnIndex = columnsCount - 1;
            layouts = CreateRainbow(columnsCount);
        }

        public ColorRGB[,] Process(float brightness)
        {
            var result = new ColorRGB[rowsCount, columnsCount];

            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colunmIndex = 0; colunmIndex < columnsCount; colunmIndex++)
                {
                    result[rowIndex, colunmIndex] = layouts[colunmIndex];
                }
            }

            ColorRGB lastColor = layouts[lastColumnIndex];
            layouts.RemoveAt(lastColumnIndex);
            layouts.Insert(0, new ColorRGB(lastColor.R, lastColor.G, lastColor.B));

            return result;
        }

        private static List<ColorRGB> CreateRainbow(int gradientCount)
        {
            float eachLed = CIRCUMFERENCE / gradientCount;
            List<ColorRGB> colors = new List<ColorRGB>();

            for (int i = 0; i < gradientCount; i++)
            {
                float hue = i * eachLed;
                ColorHSL hsl = new ColorHSL(hue, 1, 0.5f);
                ColorRGB rgb = hsl.ConvertToRGB();
                colors.Add(rgb);
            }

            return colors;
        }
    }
}
