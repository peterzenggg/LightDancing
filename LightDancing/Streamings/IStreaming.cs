using LightDancing.Colors;

namespace LightDancing.Streamings
{
    public interface IStreaming
    {
        /// <summary>
        /// Process animation
        /// </summary>
        /// <returns>Matrix layout of colors</returns>
        public ColorRGB[,] Process(float brightness);
    }
}
