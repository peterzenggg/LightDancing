using LightDancing.Colors;

namespace LightDancing.Syncs
{
    public abstract class ColorEffectBase
    {
        public abstract ColorRGB GetColors(byte r, byte g, byte b);
    }

    public class GrayScale : ColorEffectBase
    {
        public override ColorRGB GetColors(byte r, byte g, byte b)
        {
            byte average = (byte)((r + g + b) / 3);
            return new ColorRGB(average, average, average);
        }
    }

    public class Standard : ColorEffectBase
    {
        public override ColorRGB GetColors(byte r, byte g, byte b)
        {
            return new ColorRGB(r, g, b);
        }
    }

    public class Enhancement : ColorEffectBase
    {
        public override ColorRGB GetColors(byte r, byte g, byte b)
        {
            ColorRGB color = new ColorRGB(r, g, b);
            color.ColorStrengthen();
            return color;
        }
    }

    public class Negative : ColorEffectBase
    {
        public override ColorRGB GetColors(byte r, byte g, byte b)
        {
            r = (byte)(255 - r);
            g = (byte)(255 - g);
            b = (byte)(255 - b);
            return new ColorRGB(r, g, b);
        }
    }

    public class ManualAdjustment : ColorEffectBase
    {
        private readonly float saturation = 0.5f;
        private readonly float contrast = 0.5f;

        /// <summary>
        /// Adjust Saturation only
        /// </summary>
        /// <param name="saturation">Saturation range from 0-1, 0.5 means normal</param>
        public ManualAdjustment(float saturation)
        {
            this.saturation = saturation;
        }

        /// <summary>
        /// Adjust saturation and contrast
        /// </summary>
        /// <param name="saturation">Saturation range from 0-1, 0.5 means normal</param>
        /// <param name="contrast">Contrast range from 0-1, 0.5 means normal</param>
        public ManualAdjustment(float saturation, float contrast)
        {
            this.contrast = contrast;
            this.saturation = saturation;
        }

        public override ColorRGB GetColors(byte r, byte g, byte b)
        {
            ColorRGB color = new ColorRGB(r, g, b);
            color.SetManual(saturation, contrast);
            return color;
        }
    }
}
