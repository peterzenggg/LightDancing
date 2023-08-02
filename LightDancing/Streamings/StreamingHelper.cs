using LightDancing.Common;
using LightDancing.Hardware;
using LightDancing.Syncs;
using System;

namespace LightDancing.Streamings
{
    /// <summary>
    /// Make sure all of streaming resource is disposed.(OpenTk, FFT, Hid/Serial devices stream)
    /// Note: Before App closing please call the methed which is DisposeAll
    /// </summary>
    public class StreamingHelper
    {
        private static readonly Lazy<StreamingHelper> lazy = new Lazy<StreamingHelper>(() => new StreamingHelper());

        public static StreamingHelper Instance => lazy.Value;

        public DurationTimer FrameRateContolTimer { private set; get; }

        public StreamingHelper()
        {
            FrameRateContolTimer = new DurationTimer(33); //Default 30 fps
            SetScaleRatio(10); //Default ration 1/10 resolution
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        public void DisposeAll()
        {
            OpenTKSyncHelper openTKSyncHelper = OpenTKSyncHelper.Instance;
            openTKSyncHelper.Stop();
            ScreenSyncHelper screenSyncHelperMS = ScreenSyncHelper.Instance;
            screenSyncHelperMS.Stop();

            HardwaresDetector hardwaresDetector = HardwaresDetector.Instance;
            hardwaresDetector.StopStreaming();
            hardwaresDetector.SetFwAnimationON();
            hardwaresDetector.Dispose();
        }

        /// <summary>
        /// Set syncHelper process fps
        /// </summary>
        /// <param name="frameRate">syncHelper fps as 12, 15, 24, 25 etc...</param>
        public void SetFrameRate(int frameRate)
        {
            int duration = 1000 / frameRate;
            FrameRateContolTimer.UpdateDuration(duration);
        }

        /// <summary>
        /// Set syncHelper process resolution
        /// </summary>
        /// <param name="ratio">resolution ratio to monitor size, 1/10 as 10, 1/20 as 20</param>
        public void SetScaleRatio(int ratio)
        {
            ScreenInfo.Instance.SetScaleSize(ratio);
        }
    }
}
