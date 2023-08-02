using System;
using System.Diagnostics;

namespace LightDancing.Common
{
    public class DurationTimer
    {
        private TimeSpan duration;
        private Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// To check if next action is available to execute. 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public DurationTimer(int millisecondsTimeout)
        {
            duration = TimeSpan.FromMilliseconds(millisecondsTimeout);
        }
        /// <summary>
        /// Is next action ready.
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            if (stopwatch.Elapsed >= duration)
            {
                stopwatch = Stopwatch.StartNew();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update the duration
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public void UpdateDuration(int millisecondsTimeout)
        {
            duration = TimeSpan.FromMilliseconds(millisecondsTimeout);
        }

        public double GetDuration()
        {
            return duration.TotalMilliseconds;
        }
    }
}
