using LightDancing.Colors;
using System;

namespace LightDancing.Syncs
{
    public abstract class ImageFilterBase
    {
        public abstract ColorRGB[,] Process(ColorRGB[,] colorMatrix);
    }

    public class NoneFilter : ImageFilterBase
    {
        public override ColorRGB[,] Process(ColorRGB[,] colorMatrix)
        {
            return colorMatrix;
        }
    }

    public class FastBoxBlurFilter : ImageFilterBase
    {
        private readonly float blur = 0.5f;

        /// <summary>
        /// blur default size set to 25 (50 * 0.5)
        /// </summary>
        public FastBoxBlurFilter()
        { }

        public FastBoxBlurFilter(float blur)
        {
            this.blur = blur;
        }

        public override ColorRGB[,] Process(ColorRGB[,] colorMatrix)
        {
            ColorRGB[,] result = (ColorRGB[,])colorMatrix.Clone();
            float BLUR_SIZE = 50 * blur;
            float MOSAIC_SIZE = 6 * blur;
            //if BLUR_SIZE >=1, Blur activate
            if (BLUR_SIZE >= 1)
            {
                var mosaicImg = FilterEquation.GetMosaic(colorMatrix, MOSAIC_SIZE);
                result = FilterEquation.FastBoxBlur(mosaicImg, (int)BLUR_SIZE);
            }

            return result;
        }
    }

    public class GaussianBlurFilter : ImageFilterBase
    {
        private readonly float blur = 0.5f;

        /// <summary>
        /// blur default size set to 25 (50 * 0.5)
        /// </summary>
        public GaussianBlurFilter()
        { }

        public GaussianBlurFilter(float blur)
        {
            this.blur = blur;
        }

        /// <summary>
        /// Process BoxFilter 2 times will get GussianBlur
        /// </summary>
        /// <param name="colorMatrix"></param>
        /// <returns></returns>
        public override ColorRGB[,] Process(ColorRGB[,] colorMatrix)
        {
            ColorRGB[,] result = (ColorRGB[,])colorMatrix.Clone();
            float BLUR_SIZE = 50 * blur;
            float MOSAIC_SIZE = 6 * blur;
            //if BLUR_SIZE >=1, Blur activate
            if (BLUR_SIZE >= 1)
            {
                var mosaicImg = FilterEquation.GetMosaic(colorMatrix, MOSAIC_SIZE);
                var firstBoxBlur = FilterEquation.FastBoxBlur(mosaicImg, (int)BLUR_SIZE);
                result = FilterEquation.FastBoxBlur(firstBoxBlur, (int)BLUR_SIZE);
            }

            return result;
        }
    }

    public class FilterEquation
    {
        public static ColorRGB[,] GetMosaic(ColorRGB[,] img, float MOSAIC_SIZE)
        {
            int _xAxisCount = ScreenInfo.Instance.ScaleWidth;
            int _yAxisCount = ScreenInfo.Instance.ScaleHeight;
            ColorRGB[,] result = (ColorRGB[,])img.Clone();

            if (MOSAIC_SIZE >= 2)
            {
                for (int j = 0; j < _xAxisCount; j++)
                {
                    var x = j / (int)MOSAIC_SIZE * (int)MOSAIC_SIZE;
                    for (int i = 0; i < _yAxisCount; i++)
                    {
                        var y = i / (int)MOSAIC_SIZE * (int)MOSAIC_SIZE;

                        result[i, j] = img[y, x] != null ? new ColorRGB(img[y, x].R, img[y, x].G, img[y, x].B) : new ColorRGB(0, 0, 0);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// https://www.youtube.com/watch?v=LKnqECcg6Gw&ab_channel=minutephysics
        /// </summary>
        /// <param name="img"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static ColorRGB[,] FastBoxBlur(ColorRGB[,] img, int radius)
        {
            int _xAxisCount = ScreenInfo.Instance.ScaleWidth;
            int _yAxisCount = ScreenInfo.Instance.ScaleHeight;

            int kSize = radius;

            if (kSize % 2 == 0 || kSize / 2 == 0) kSize++;

            ColorRGB[,] Hblur = img != null ? (ColorRGB[,])img.Clone() : new ColorRGB[ScreenInfo.Instance.ScaleHeight, ScreenInfo.Instance.ScaleWidth];

            float Avg = (float)1 / kSize;

            for (int j = 0; j < _xAxisCount; j++)
            {
                float[] hSum = new float[] { 0, 0, 0, 0 };

                float[] iAvg = new float[] { 0, 0, 0, 0 };

                for (int x = 0; x < kSize / 2; x++)
                {
                    ColorRGB tmpColor = img[x, j];
                    hSum[1] += ColorRGB.GetPowOfTwo(tmpColor.R);
                    hSum[2] += ColorRGB.GetPowOfTwo(tmpColor.G);
                    hSum[3] += ColorRGB.GetPowOfTwo(tmpColor.B);
                }

                for (int i = 0; i < _yAxisCount; i++)
                {
                    var i_min = i - kSize / 2 >= 0 ? i - kSize / 2 : 0;
                    var i_large = i + kSize / 2 < _yAxisCount ? i + kSize / 2 : _yAxisCount - 1;

                    if (i - kSize / 2 < 0 && i + kSize / 2 < _yAxisCount)
                    {
                        ColorRGB tmp_nColor = img[i_large, j];
                        hSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (i_large + 1));
                    }
                    else if (i - kSize / 2 < 0 && i + kSize / 2 >= _yAxisCount)
                    {
                        Avg = radius < 2 ? 0.5f : ((float)1 / _yAxisCount);
                    }
                    else if (i - kSize / 2 >= 0 && i + kSize / 2 < _yAxisCount)
                    {
                        ColorRGB tmp_pColor = img[i_min, j];
                        hSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        hSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        hSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);
                        ColorRGB tmp_nColor = img[i_large, j];
                        hSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? ((float)1 / kSize) : ((float)1 / (i_large - i_min));
                    }
                    else
                    {
                        ColorRGB tmp_pColor = img[i_min, j];
                        hSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        hSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        hSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (i_large - i_min));
                    }

                    iAvg[1] = hSum[1] * Avg;
                    iAvg[2] = hSum[2] * Avg;
                    iAvg[3] = hSum[3] * Avg;

                    Hblur[i, j] = new ColorRGB((byte)Math.Sqrt(iAvg[1]), (byte)Math.Sqrt(iAvg[2]), (byte)Math.Sqrt(iAvg[3]));
                }
            }

            ColorRGB[,] total = (ColorRGB[,])Hblur.Clone();
            for (int i = 0; i < _yAxisCount; i++)
            {
                float[] tSum = new float[] { 0, 0, 0, 0 };
                float[] iAvg = new float[] { 0, 0, 0, 0 };
                for (int y = 0; y < kSize / 2; y++)
                {
                    ColorRGB tmpColor = Hblur[(i), (y)];
                    tSum[1] += ColorRGB.GetPowOfTwo(tmpColor.R);
                    tSum[2] += ColorRGB.GetPowOfTwo(tmpColor.G);
                    tSum[3] += ColorRGB.GetPowOfTwo(tmpColor.B);
                }

                for (int j = 0; j < _xAxisCount; j++)
                {
                    var j_min = j - kSize / 2 >= 0 ? j - kSize / 2 : 0;
                    var j_large = j + kSize / 2 < _xAxisCount ? j + kSize / 2 : _xAxisCount - 1;

                    if (j - kSize / 2 < 0 && i + kSize / 2 < _xAxisCount)
                    {
                        ColorRGB tmp_nColor = Hblur[i, j_large];
                        tSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        tSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        tSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (j_large + 1));
                    }
                    else if (j - kSize / 2 < 0 && j + kSize / 2 >= _xAxisCount)
                    {
                        Avg = radius < 2 ? 0.5f : ((float)1 / _xAxisCount);
                    }
                    else if (j - kSize / 2 >= 0 && j + kSize / 2 < _xAxisCount)
                    {
                        ColorRGB tmp_pColor = Hblur[i, j_min];
                        tSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        tSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        tSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);
                        ColorRGB tmp_nColor = Hblur[i, j_large];
                        tSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        tSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        tSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? ((float)1 / kSize) : ((float)1 / (j_large - j_min));
                    }
                    else
                    {
                        ColorRGB tmp_pColor = Hblur[i, j_min];
                        tSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        tSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        tSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (j_large - j_min));
                    }

                    iAvg[1] = (float)Math.Sqrt(tSum[1] * Avg);
                    iAvg[2] = (float)Math.Sqrt(tSum[2] * Avg);
                    iAvg[3] = (float)Math.Sqrt(tSum[3] * Avg);

                    total[i, j] = new ColorRGB((byte)iAvg[1], (byte)iAvg[2], (byte)iAvg[3]);
                }
            }

            return total;
        }
    }
}
