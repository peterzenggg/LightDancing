using LightDancing.Colors;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Streamings
{
    public class Bounce : IStreaming
    {
        private readonly int rowsCount;
        private readonly int columnsCount;
        private readonly int lastColumnIndex;
        private readonly List<ColorRGB> layouts = new List<ColorRGB>();

        private bool isBackward = false;
        private int moveSteps = 0;

        /// <summary>
        /// The matrix of size rowsCount * colunmCount
        /// </summary>
        /// <param name="rowsCount">The rows count of LEDs</param>
        /// <param name="columnsCount">The columns count of LEDs</param>
        /// <param name="color">Display color of Bounce effect</param>
        public Bounce(int rowsCount, int columnsCount, ColorRGB color)
        {
            this.rowsCount = rowsCount;
            this.columnsCount = columnsCount;
            lastColumnIndex = columnsCount - 1;
            for (int i = 0; i < columnsCount; i++)
            {
                if (i == 0)
                    layouts.Add(color);
                else
                    layouts.Add(ColorRGB.Black());
            }
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

            if (isBackward)
            {
                var firstColor = layouts.First();
                layouts.RemoveAt(0);
                layouts.Add(new ColorRGB(firstColor.R, firstColor.G, firstColor.B));
                moveSteps--;
            }
            else
            {
                var lastColor = layouts.Last();
                layouts.RemoveAt(lastColumnIndex);
                layouts.Insert(0, new ColorRGB(lastColor.R, lastColor.G, lastColor.B));
                moveSteps++;
            }

            if (moveSteps == lastColumnIndex)
            {
                isBackward = true;
            }
            else if (moveSteps == 0)
            {
                isBackward = false;
            }

            return result;
        }
    }
}
