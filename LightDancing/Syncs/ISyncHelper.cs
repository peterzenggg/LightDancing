using LightDancing.Colors;
using System;
using System.Drawing;

namespace LightDancing.Syncs
{
    public interface ISyncHelper
    {
        /// <summary>
        /// Get the matrix colors
        /// </summary>
        /// <returns>Matrix colors</returns>
        public ColorRGB[,] GetColors();

        /// <summary>
        /// Get the bitmap for preview screen
        /// Seperate method for preview screen and streaming to avoid memory occupy
        /// </summary>
        /// <returns>bitmap</returns>
        public Bitmap GetBitmapForPreviewImage();

        /// <summary>
        /// Get the bitmap for streaming
        /// Seperate method for preview screen and streaming to avoid memory occupy
        /// </summary>
        /// <returns>bitmap</returns>
        public Bitmap GetBitmapForStreaming();

        /// <summary>
        /// Stop processing sync
        /// </summary>
        public void Stop();

        /// <summary>
        /// For frontend to register for callback when backend image is updated
        /// </summary>
        /// <param name="func"></param>
        public void RegisterPreviewUpdateCallback(Action func);

        /// <summary>
        /// Set streaming mode with blur filter
        /// </summary>
        /// <param name="blur"></param>
        public void SetBlur(ImageFilterBase filter);

        /// <summary>
        /// Get the color matrix in format of base64
        /// </summary>
        /// <returns></returns>
        public string GetBase64StringColor();

        public Bitmap GetBitmapForAlphaVideo();

        /// <summary>
        /// Set Speed Configuration
        /// </summary>
        /// <param name="speed">should be 0(x0.25), 1(x0.5) ... 6(x1.75), 7(x2)</param>
        public void SetSpeedConfig(int speed);

        /// <summary>
        /// Set Blue Reduction
        /// </summary>
        /// <param name="_reduction"></param>
        public void SetBlueFilter(bool _reduction);

        public Action ScreenUpdated { get; set; }
    }
}
