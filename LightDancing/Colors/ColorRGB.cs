using System;
using LightDancing.Common;

namespace LightDancing.Colors
{
    /// <summary>
    /// This is for each LEDs of display RGB colors
    /// </summary>
    public class ColorRGB
    {
        private const float STRENGTHEN_PERCENTAGE = 0.66f;
        public static ColorRGB Black()
        {
            return new ColorRGB(0, 0, 0);
        }

        public ColorRGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public ColorRGB(byte r, byte g, byte b, double a)
        {
            R = (byte)(r * a);
            G = (byte)(g * a);
            B = (byte)(b * a);
        }

        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        /// <summary>
        /// Convert current color rgb to hsl
        /// </summary>
        /// <returns>ColorHSL</returns>
        public ColorHSL ConvertToHSL()
        {
            System.Drawing.Color systemColor = System.Drawing.Color.FromArgb(R, G, B);
            float h = (float)Math.Round(systemColor.GetHue(), 2);
            float s = (float)Math.Round(systemColor.GetSaturation(), 2);
            float l = (float)Math.Round(systemColor.GetBrightness(), 2);

            return new ColorHSL(h, s, l);
        }

        /// <summary>
        /// Convert the Color to HSL and strengthen the saturation or reduce the lightness
        /// reference 1: https://zh.wikipedia.org/wiki/HSL%E5%92%8CHSV%E8%89%B2%E5%BD%A9%E7%A9%BA%E9%97%B4
        /// reference 2: http://james-ramsden.com/convert-from-hsl-to-rgb-colour-codes-in-c/
        /// </summary>
        public void ColorStrengthen()
        {
            #region RGB to HSL

            ColorHSL colorHSL = ConvertToHSL();

            if (colorHSL.S < 1)
            {
                colorHSL.S = ((1 - colorHSL.S) * STRENGTHEN_PERCENTAGE) + colorHSL.S;
            }

            if (colorHSL.L > 0.5)
            {
                colorHSL.L = (float)(colorHSL.L - ((colorHSL.L - 0.5) * STRENGTHEN_PERCENTAGE));
            }

            #endregion RGB to HSL

            #region HSL to RGB

            ColorRGB colorRGB = colorHSL.ConvertToRGB();
            R = colorRGB.R;
            G = colorRGB.G;
            B = colorRGB.B;

            #endregion HSL to RGB
        }

        public void SetManual(float saturation, float contrast)
        {
            #region RGB to HSL

            ColorHSL colorHSL = ConvertToHSL();

            //Compute saturation from 0~1 to -1~1
            saturation = Normalize(saturation);
            colorHSL.S = saturation < 0 ? (colorHSL.S * saturation) + colorHSL.S : (1 - colorHSL.S) * saturation + colorHSL.S;

            contrast = Normalize(contrast);
            //high contrast
            if (contrast > 0)
            {
                if (colorHSL.L > 0.5)
                {
                    colorHSL.L = (float)(colorHSL.L + ((1 - colorHSL.L) * contrast * 0.7));

                }
                else
                {
                    colorHSL.L -= colorHSL.L * contrast * 0.7f;
                }
            }
            //low contrast
            else if (contrast < 0)
            {
                if (colorHSL.L > 0.5)
                {
                    colorHSL.L = (float)(colorHSL.L - ((1 - colorHSL.L) * contrast * 0.5));
                }
                else
                {
                    colorHSL.L = (float)(colorHSL.L - (1 - colorHSL.L) * contrast * 0.5);
                }
            }
            colorHSL.L = colorHSL.L > 1 ? 1 : colorHSL.L;
            colorHSL.L = colorHSL.L < 0 ? 0 : colorHSL.L;

            if (colorHSL.L < 0)
            { }
            #endregion RGB to HSL

            #region HSL to RGB

            ColorRGB colorRGB = colorHSL.ConvertToRGB();
            R = colorRGB.R;
            G = colorRGB.G;
            B = colorRGB.B;

            #endregion HSL to RGB
        }

        public static float GetPowOfTwo(byte x)
        {
            return (float)Math.Pow(x, 2);
        }

        private static float Normalize(float i)
        {
            return i * 2 - 1f;
        }

        public void BrightnessRange(float max)
        {
            #region RGB to HSL

            ColorHSL colorHSL = ConvertToHSL();

            colorHSL.L *= max;

            #endregion RGB to HSL

            #region HSL to RGB

            ColorRGB colorRGB = colorHSL.ConvertToRGB();
            R = colorRGB.R;
            G = colorRGB.G;
            B = colorRGB.B;

            #endregion HSL to RGB
        }

        /// <summary>
        /// Input range from 0 to 1 will be remapped to 0 - 360
        /// </summary>
        /// <param name="hue">range from 0 - 1</param>
        public void SetHue(float hue)
        {
            #region RGB to HSL

            ColorHSL colorHSL = ConvertToHSL();

            //Compute hue from 0~1 to 0~360
            hue *= 360;
            colorHSL.H = (int)(colorHSL.H + hue) % 360;
            #endregion RGB to HSL

            #region HSL to RGB

            ColorRGB colorRGB = colorHSL.ConvertToRGB();
            R = colorRGB.R;
            G = colorRGB.G;
            B = colorRGB.B;

            #endregion HSL to RGB
        }

        /// <summary>
        /// Input range from 0 to 1, 0.5 means normal saturation
        /// </summary>
        /// <param name="sat"></param>
        public void SetSat(float sat)
        {
            #region RGB to HSL

            ColorHSL colorHSL = ConvertToHSL();

            //Compute saturation from 0~1 to -1~1
            sat = Normalize(sat);
            colorHSL.S = sat < 0 ? (colorHSL.S * sat) + colorHSL.S : (1 - colorHSL.S) * sat + colorHSL.S;
            #endregion RGB to HSL

            #region HSL to RGB

            ColorRGB colorRGB = colorHSL.ConvertToRGB();
            R = colorRGB.R;
            G = colorRGB.G;
            B = colorRGB.B;

            #endregion HSL to RGB
        }

        /// <summary>
        /// The passthrough color percentage, 1 mean show all color, 0 means show no color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public void SetColorFilter(float r, float g, float b)
        {
            R = (byte)(R * r);
            G = (byte)(G * g);
            B = (byte)(B * b);
        }

        /// <summary>
        /// To prevent CNVS current go over 1.8A (All white goes to 2.5A now)
        /// 1.8/2.5 = 0.72
        /// </summary>
        public void CNVSBrightnessAdjustment()
        {
            double brightness = (double)(R + G + B) / 765;
            if (brightness > Math.Pow(brightness, 0.55f) * 0.72)
            {
                brightness = Math.Pow(brightness, 0.55) * 0.72;

                R = (byte)(R * brightness);
                G = (byte)(G * brightness);
                B = (byte)(B * brightness);
            }
        }

        public string GetRGBString()
        {
            return $"{Methods.ByteToHexString(R)}{Methods.ByteToHexString(G)}{Methods.ByteToHexString(B)}";
        }

        public string GetRGBStringStartWith0x()
        {
            return $"0x{GetRGBString()}";
        }
    }
}
