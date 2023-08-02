namespace LightDancing.Colors
{
    public class ColorHSL
    {
        /// <summary>
        /// Color of Hue
        /// </summary>
        public float H { get; set; }

        /// <summary>
        /// Color of Saturation
        /// </summary>
        public float S { get; set; }

        /// <summary>
        /// Color of Lightness
        /// </summary>
        public float L { get; set; }

        public ColorHSL(float h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        /// <summary>
        /// Convert the HSL to RGB
        /// </summary>
        /// <returns>Color RGB</returns>
        public ColorRGB ConvertToRGB()
        {
            float hue = H;
            float saturation = S;
            float lightness = L;

            byte r, g, b;

            if (saturation == 0)
            {
                r = g = b = (byte)(lightness * 255);
            }
            else
            {
                float q = (lightness < 0.5) ? (lightness * (1 + saturation)) : ((lightness + saturation) - (lightness * saturation));
                float p = 2 * lightness - q;
                float hk = hue / 360;

                r = (byte)(255 * HueToRGB(p, q, hk + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(p, q, hk));
                b = (byte)(255 * HueToRGB(p, q, hk - (1.0f / 3)));
            }

            return new ColorRGB(r, g, b);
        }

        /// <summary>
        /// https://zh.wikipedia.org/wiki/HSL%E5%92%8CHSV%E8%89%B2%E5%BD%A9%E7%A9%BA%E9%97%B4
        /// https://axonflux.com/handy-rgb-to-hsl-and-rgb-to-hsv-color-model-c
        /// </summary>
        private float HueToRGB(float p, float q, float tc)
        {
            if (tc < 0)
                tc += 1;

            if (tc > 1)
                tc -= 1;

            if ((6 * tc) < 1)
                return (p + (q - p) * 6 * tc);

            if ((2 * tc) < 1)
                return q;

            if ((3 * tc) < 2)
                return (p + (q - p) * 6 * ((2.0f / 3) - tc));

            return p;
        }
    }
}
