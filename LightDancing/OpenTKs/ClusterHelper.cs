using LightDancing.Colors;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.OpenTKs
{
    public class ClusterHelper
    {
        private const byte COLOR_BYTE_THRESH = 127;
        private const int COLOR_INDEX_BLACK = 0;
        private const int COLOR_INDEX_BLUE = 1;
        private const int COLOR_INDEX_GREEN = COLOR_INDEX_BLUE << 1;
        private const int COLOR_INDEX_CYAN = COLOR_INDEX_BLUE | COLOR_INDEX_GREEN;
        private const int COLOR_INDEX_RED = COLOR_INDEX_BLUE << 2;
        private const int COLOR_INDEX_MAGENTA = COLOR_INDEX_RED | COLOR_INDEX_BLUE;
        private const int COLOR_INDEX_YELLOW = COLOR_INDEX_RED | COLOR_INDEX_GREEN;
        private const int COLOR_INDEX_WHITE = COLOR_INDEX_RED | COLOR_INDEX_GREEN | COLOR_INDEX_BLUE;

        private readonly Dictionary<int, List<ColorRGB>> cubical;

        /// <summary>
        /// This helper will collect the color and storage to a similar cubical, in the end, it will get the max cubical then average those colors.
        /// </summary>
        public ClusterHelper()
        {
            cubical = BuildCubicalDictionary();
        }

        /// <summary>
        /// To calculate the color belongs with which main color
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<ColorRGB>> BuildCubicalDictionary()
        {
            return new Dictionary<int, List<ColorRGB>>()
            {
                {COLOR_INDEX_BLACK, new List<ColorRGB>()},
                {COLOR_INDEX_BLUE, new List<ColorRGB>()},
                {COLOR_INDEX_GREEN, new List<ColorRGB>()},
                {COLOR_INDEX_CYAN, new List<ColorRGB>()},
                {COLOR_INDEX_RED, new List<ColorRGB>()},
                {COLOR_INDEX_MAGENTA, new List<ColorRGB>()},
                {COLOR_INDEX_YELLOW, new List<ColorRGB>()},
                {COLOR_INDEX_WHITE, new List<ColorRGB>()},
            };
        }

        /// <summary>
        /// Convert input color into RGB space cubical by RGB binary division
        /// Index  R    G    B // Color
        ///  [0] 000, 000, 000 // Black
        ///  [1] 000, 000, 255 // Blue
        ///  [2] 000, 255, 000 // Green
        ///  [3] 000, 255, 255 // Cyan
        ///  [4] 255, 000, 000 // Red
        ///  [5] 255, 000, 255 // Magenta
        ///  [6] 255, 255, 000 // Yellow
        ///  [7] 255, 255, 255 // White
        /// </summary>
        /// <param name="colorInput">Input color</param>
        /// <returns>Cubical Color Space index</returns>
        private int ConvertColorIndex(ColorRGB colorInput)
        {
            int bResult = 0;

            bResult |= ((colorInput.R > COLOR_BYTE_THRESH) ? COLOR_INDEX_RED : COLOR_INDEX_BLACK);
            bResult |= ((colorInput.G > COLOR_BYTE_THRESH) ? COLOR_INDEX_GREEN : COLOR_INDEX_BLACK);
            bResult |= ((colorInput.B > COLOR_BYTE_THRESH) ? COLOR_INDEX_BLUE : COLOR_INDEX_BLACK);

            return bResult;
        }

        /// <summary>
        /// Add color to cubical
        /// </summary>
        /// <param name="color"></param>
        public void Add(ColorRGB color)
        {
            int cubicalIndex = ConvertColorIndex(color);
            cubical[cubicalIndex].Add(color);
        }

        /// <summary>
        /// Get the most of color cubical, and average that cubical of all colors
        /// </summary>
        /// <returns></returns>
        public ColorRGB AverageMostColor()
        {
            #region Remove nondominate blackwhite

            long countAllHalf = cubical.Sum(x => x.Value.Count) >> 1;

            if (cubical[COLOR_INDEX_BLACK].Count < countAllHalf)
            {
                cubical[COLOR_INDEX_BLACK].Clear();
            }

            if (cubical[COLOR_INDEX_WHITE].Count < countAllHalf)
            {
                cubical[COLOR_INDEX_WHITE].Clear();
            }

            #endregion Remove nondominate blackwhite 

            long coumtMax = cubical.Max(x => x.Value.Count);

            List<ColorRGB> lstColors = cubical.FirstOrDefault(x => x.Value.Count == coumtMax).Value;
            byte r = (byte)(lstColors.Average(x => x.R));
            byte g = (byte)(lstColors.Average(x => x.G));
            byte b = (byte)(lstColors.Average(x => x.B));

            return new ColorRGB(r, g, b);
        }
    }
}
