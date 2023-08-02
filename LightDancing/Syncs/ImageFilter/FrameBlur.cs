using LightDancing.Colors;
using System;

namespace LightDancing.Syncs.ImageFilter
{
    public class FrameBlur : ImageFilterBase
    {
        private readonly float blur = 0.5f;

        /// <summary>
        /// blur default size set to 25 (50 * 0.5)
        /// </summary>
        public FrameBlur()
        { }

        public FrameBlur(float blur)
        {
            this.blur = blur;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorMatrix"></param>
        /// <returns></returns>
        public override ColorRGB[,] Process(ColorRGB[,] colorMatrix)
        {
            ColorRGB[,] result = (ColorRGB[,])colorMatrix.Clone();
            float BLUR_SIZE = 50 * blur;

            //if BLUR_SIZE >=1, Blur activate
            if (BLUR_SIZE >= 1)
            {
                var frameBlurImg = GetBlurFrameColorMixed(colorMatrix);
                result = FilterEquation.FastBoxBlur(frameBlurImg, (int)BLUR_SIZE);
            }

            return result;
        }

        /// <summary>
        /// 1. Get color enhanced frame color blurred
        /// 2. Mix left and right frame
        /// 3. Mix Top and Bottom frame
        /// 4. Mix left-right and top-bottom img
        /// 5. Adjsut brightness and gamma value
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static ColorRGB[,] GetBlurFrameColorMixed(ColorRGB[,] img)
        {
            int _xAxisCount = ScreenInfo.Instance.ScaleWidth;
            int _yAxisCount = ScreenInfo.Instance.ScaleHeight;
            ColorRGB[,] result = (ColorRGB[,])img.Clone();

            #region Get Color Enhanced Frame Color Blurred
            ColorEffectBase saturationFilter = new ManualAdjustment(0.7f);
            const float FRAME_SIZE = 0.2f;
            const int BLUR_SIZE = 40;

            var top_frame = GetFrame(img, BLUR_SIZE, _xAxisCount, _yAxisCount, Tuple.Create(FRAME_SIZE, 1f), Tuple.Create(0, 0), saturationFilter);
            var bottom_frame = GetFrame(img, BLUR_SIZE, _xAxisCount, _yAxisCount, Tuple.Create(FRAME_SIZE, 1f), Tuple.Create((int)(_xAxisCount * (1 - FRAME_SIZE)), 0), saturationFilter);
            var left_frame = GetFrame(img, BLUR_SIZE, _xAxisCount, _yAxisCount, Tuple.Create(1f, FRAME_SIZE), Tuple.Create(0, 0), saturationFilter);
            var right_frame = GetFrame(img, BLUR_SIZE, _xAxisCount, _yAxisCount, Tuple.Create(1f, FRAME_SIZE), Tuple.Create(0, (int)(_yAxisCount * (1 - FRAME_SIZE))), saturationFilter);

            #endregion Get Color Enhanced Frame Color Blurred

            #region Get Left and Right frame mixed
            for (int j = 0; j < _xAxisCount; j++)
            {
                int x_index = (int)(j * FRAME_SIZE) >= (int)(_xAxisCount * FRAME_SIZE) - 1 ? (int)(_xAxisCount * FRAME_SIZE) - 1 : (int)(j * FRAME_SIZE);
                /*power 2 to make a linear ramp for the side blur mixed*/
                float percentage_R = (float)Math.Pow(j / (float)(_xAxisCount - 1), 2);
                float percentage_L = (float)Math.Pow((_xAxisCount - 1 - j) / (float)(_xAxisCount - 1), 2);

                for (int i = 0; i < _yAxisCount; i++)
                {
                    if (img[i, j] != null)
                    {
                        byte r = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(top_frame[i, x_index].R) * percentage_L + ColorRGB.GetPowOfTwo(bottom_frame[i, x_index].R) * percentage_R);
                        byte g = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(top_frame[i, x_index].G) * percentage_L + ColorRGB.GetPowOfTwo(bottom_frame[i, x_index].G) * percentage_R);
                        byte b = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(top_frame[i, x_index].B) * percentage_L + ColorRGB.GetPowOfTwo(bottom_frame[i, x_index].B) * percentage_R);

                        result[i, j] = new ColorRGB(r, g, b);
                    }
                    else
                        result[i, j] = new ColorRGB(0, 0, 0);
                }
            }
            #endregion Get Left and Right frame mixed

            #region Get Top and Bottom frame mixed
            for (int j = 0; j < _xAxisCount; j++)
            {
                for (int i = 0; i < _yAxisCount; i++)
                {
                    int y_index = (int)(i * FRAME_SIZE) >= (int)(_yAxisCount * FRAME_SIZE) ? (int)(_yAxisCount * FRAME_SIZE) - 1 : (int)(i * FRAME_SIZE);
                    /*power 4 to make a linear ramp for the side blur mixed*/
                    float percentage_R = (float)Math.Pow(i / (float)(_yAxisCount - 1), 4);
                    float percentage_L = (float)Math.Pow((_yAxisCount - 1 - i) / (float)(_yAxisCount - 1), 4);

                    if (img[i, j] != null)
                    {
                        /*Top and Bottom frame mix*/
                        byte r = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(left_frame[y_index, j].R) * percentage_L + ColorRGB.GetPowOfTwo(right_frame[y_index, j].R) * percentage_R);
                        byte g = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(left_frame[y_index, j].G) * percentage_L + ColorRGB.GetPowOfTwo(right_frame[y_index, j].G) * percentage_R);
                        byte b = (byte)Math.Sqrt(ColorRGB.GetPowOfTwo(left_frame[y_index, j].B) * percentage_L + ColorRGB.GetPowOfTwo(right_frame[y_index, j].B) * percentage_R);

                        ColorRGB temp = new ColorRGB(r, g, b);

                        /*Adjust brightness*/
                        result[i, j].BrightnessRange(0.9f);
                        temp.BrightnessRange(0.9f);

                        /*Top-Bottom and Left-Right img mix*/
                        r = (byte)((temp.R + result[i, j].R) > 255 ? 255 : temp.R + result[i, j].R);
                        g = (byte)((temp.G + result[i, j].G) > 255 ? 255 : temp.G + result[i, j].G);
                        b = (byte)((temp.B + result[i, j].B) > 255 ? 255 : temp.B + result[i, j].B);

                        result[i, j] = new ColorRGB(r, g, b);
                        /*Adjust brightness*/
                        result[i, j].BrightnessRange(0.9f);
                        /*Adjust Gmma*/
                        result[i, j].R = (byte)(Math.Pow(result[i, j].R / 255f, 0.9) * 255);
                        result[i, j].G = (byte)(Math.Pow(result[i, j].G / 255f, 0.9) * 255);
                        result[i, j].B = (byte)(Math.Pow(result[i, j].B / 255f, 0.9) * 255);
                    }
                    else
                        result[i, j] = new ColorRGB(0, 0, 0);
                }
            }
            #endregion Get Top and Bottom frame mixed

            return result;
        }

        /// <summary>
        /// 1. Get enhanced color frame blurred
        /// 2. Process second time box blur to get blur more smooth
        /// </summary>
        /// <param name="img"></param>
        /// <param name="radius">Box blur blur size</param>
        /// <param name="width">width of input image</param>
        /// <param name="height">height of input image</param>
        /// <param name="percentage"> Item1 is the frame size percentage of width, Item2 is the frame size percentage of height</param>
        /// <param name="offset">Item1 is the index offset of width to get pixel for the frame, Item2 is the index offset of height</param>
        /// <param name="colorEffectBase">Colorfilter</param>
        /// <returns></returns>
        public static ColorRGB[,] GetFrame(ColorRGB[,] img, int radius, int width, int height, Tuple<float, float> percentage, Tuple<int, int> offset, ColorEffectBase colorEffectBase)
        {
            var firstBoxBlur = ResizedFastBoxBlur(img, radius, width, height, percentage, offset, colorEffectBase);
            var secondBoxBlur = ResizedFastBoxBlur(firstBoxBlur, radius, firstBoxBlur.GetLength(1), firstBoxBlur.GetLength(0), Tuple.Create(1f, 1f), Tuple.Create(0, 0), new Standard());

            return secondBoxBlur;
        }

        public static ColorRGB[,] ResizedFastBoxBlur(ColorRGB[,] img, int radius, int width, int height, Tuple<float, float> percentage, Tuple<int, int> offset, ColorEffectBase colorEffectBase)
        {
            int _xAxisCount = (int)(width * percentage.Item1) + 1 > width ? width : (int)(width * percentage.Item1) + 1;
            int _yAxisCount = (int)(height * percentage.Item2) + 1 > height ? height : (int)(height * percentage.Item2) + 1;

            int kSize = radius;

            if (kSize % 2 == 0 || kSize / 2 == 0) kSize++;

            ColorRGB[,] Hblur = new ColorRGB[_yAxisCount, _xAxisCount];

            float Avg = (float)1 / kSize;

            for (int j = offset.Item1; j < offset.Item1 + _xAxisCount; j++)
            {
                float[] hSum = new float[] { 0, 0, 0, 0 };

                float[] iAvg = new float[] { 0, 0, 0, 0 };

                if (offset.Item2 + kSize / 2 > height)
                {
                    for (int x = offset.Item2; x < height; x++)
                    {
                        ColorRGB tmpColor = colorEffectBase.GetColors(img[x, j].R, img[x, j].G, img[x, j].B);

                        hSum[1] += ColorRGB.GetPowOfTwo(tmpColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmpColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmpColor.B);
                    }
                }
                else
                {
                    for (int x = offset.Item2; x < offset.Item2 + kSize / 2; x++)
                    {
                        ColorRGB tmpColor = colorEffectBase.GetColors(img[x, j].R, img[x, j].G, img[x, j].B);

                        hSum[1] += ColorRGB.GetPowOfTwo(tmpColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmpColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmpColor.B);
                    }
                }

                for (int i = offset.Item2; i < offset.Item2 + _yAxisCount; i++)
                {
                    var i_min = i - kSize / 2 >= offset.Item2 ? i - kSize / 2 : offset.Item2;
                    var i_large = i + kSize / 2 < offset.Item2 + _yAxisCount ? i + kSize / 2 : offset.Item2 + _yAxisCount - 1;

                    if (i - kSize / 2 < offset.Item2 && i + kSize / 2 < offset.Item2 + _yAxisCount)
                    {
                        ColorRGB tmp_nColor = colorEffectBase.GetColors(img[i_large, j].R, img[i_large, j].G, img[i_large, j].B);
                        hSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (i_large + 1));
                    }
                    else if (i - kSize / 2 < offset.Item2 && i + kSize / 2 >= offset.Item2 + _yAxisCount)
                    {
                        Avg = radius < 2 ? 0.5f : ((float)1 / (offset.Item2 + _yAxisCount));
                    }
                    else if (i - kSize / 2 >= offset.Item2 && i + kSize / 2 < offset.Item2 + _yAxisCount)
                    {
                        ColorRGB tmp_pColor = colorEffectBase.GetColors(img[i_min, j].R, img[i_min, j].G, img[i_min, j].B);
                        hSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        hSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        hSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);
                        ColorRGB tmp_nColor = colorEffectBase.GetColors(img[i_large, j].R, img[i_large, j].G, img[i_large, j].B);
                        hSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        hSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        hSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? ((float)1 / kSize) : ((float)1 / (i_large - i_min));
                    }
                    else
                    {
                        ColorRGB tmp_pColor = colorEffectBase.GetColors(img[i_min, j].R, img[i_min, j].G, img[i_min, j].B);
                        hSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        hSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        hSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (i_large - i_min));
                    }

                    iAvg[1] = hSum[1] * Avg;
                    iAvg[2] = hSum[2] * Avg;
                    iAvg[3] = hSum[3] * Avg;

                    Hblur[i - offset.Item2, j - offset.Item1] = new ColorRGB((byte)Math.Sqrt(iAvg[1]), (byte)Math.Sqrt(iAvg[2]), (byte)Math.Sqrt(iAvg[3]));
                }
            }

            ColorRGB[,] total = (ColorRGB[,])Hblur.Clone();
            for (int i = 0; i < _yAxisCount; i++)
            {
                if (i == 34)
                { }
                float[] tSum = new float[] { 0, 0, 0, 0 };
                float[] iAvg = new float[] { 0, 0, 0, 0 };

                for (int y = 0; y < kSize / 2; y++)
                {
                    ColorRGB tmpColor = colorEffectBase.GetColors(Hblur[i, y].R, Hblur[i, y].G, Hblur[i, y].B);
                    tSum[1] += ColorRGB.GetPowOfTwo(tmpColor.R);
                    tSum[2] += ColorRGB.GetPowOfTwo(tmpColor.G);
                    tSum[3] += ColorRGB.GetPowOfTwo(tmpColor.B);
                }

                for (int j = 0; j < _xAxisCount; j++)
                {
                    var j_min = j - kSize / 2 >= 0 ? j - kSize / 2 : 0;
                    var j_large = j + kSize / 2 < _xAxisCount ? j + kSize / 2 : _xAxisCount - 1;

                    if (j - kSize / 2 < 0 && j + kSize / 2 < _xAxisCount)
                    {
                        ColorRGB tmp_nColor = colorEffectBase.GetColors(Hblur[i, j_large].R, Hblur[i, j_large].G, Hblur[i, j_large].B);
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
                        ColorRGB tmp_pColor = colorEffectBase.GetColors(Hblur[i, j_min].R, Hblur[i, j_min].G, Hblur[i, j_min].B);
                        tSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        tSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        tSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);
                        ColorRGB tmp_nColor = colorEffectBase.GetColors(Hblur[i, j_large].R, Hblur[i, j_large].G, Hblur[i, j_large].B);
                        tSum[1] += ColorRGB.GetPowOfTwo(tmp_nColor.R);
                        tSum[2] += ColorRGB.GetPowOfTwo(tmp_nColor.G);
                        tSum[3] += ColorRGB.GetPowOfTwo(tmp_nColor.B);

                        Avg = radius < 2 ? ((float)1 / kSize) : ((float)1 / (j_large - j_min));
                    }
                    else
                    {
                        ColorRGB tmp_pColor = colorEffectBase.GetColors(Hblur[i, j_min].R, Hblur[i, j_min].G, Hblur[i, j_min].B);
                        tSum[1] -= ColorRGB.GetPowOfTwo(tmp_pColor.R);
                        tSum[2] -= ColorRGB.GetPowOfTwo(tmp_pColor.G);
                        tSum[3] -= ColorRGB.GetPowOfTwo(tmp_pColor.B);

                        Avg = radius < 2 ? 0.5f : ((float)1 / (j_large - j_min));
                    }

                    iAvg[1] = (float)Math.Sqrt(tSum[1] * Avg);
                    iAvg[2] = (float)Math.Sqrt(tSum[2] * Avg);
                    iAvg[3] = (float)Math.Sqrt(tSum[3] * Avg);

                    if (iAvg[1] < 255 || iAvg[2] < 255 || iAvg[3] < 255)
                    { }
                    total[i, j] = new ColorRGB((byte)iAvg[1], (byte)iAvg[2], (byte)iAvg[3]);
                }
            }

            return total;
        }
    }
}