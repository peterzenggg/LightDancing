using LightDancing.Common;
using NAudio.Dsp;
using System;

namespace LightDancing.MusicAnalysis
{
    public class MusicEndDetector
    {
        private int fromLastEndingCounter = 0;

        private int lowInyensityCounter = 0;

        private bool previousEnd = false;


        /// <summary>
        /// Reset all data
        /// </summary>
        public void Reset()
        {
            fromLastEndingCounter = 0;
            lowInyensityCounter = 0;
        }

        /// <summary>
        ///  Detecting low intensity as the end or beginning of a song
        /// </summary>
        /// <param name="complices">FFT values</param>
        /// <param name="maxIntensity">the max intensity of all bands</param>
        /// <returns></returns>
        public bool SongEndDetection(Complex[] fftResults)
        {
            bool result = DetectEnd(fftResults);

            // That means the FftResult does not update, no need to update continuously.
            if (result && previousEnd)
            {
                return false;
            }
            else
            {
                previousEnd = result;
                return result;
            }
        }

        private bool DetectEnd(Complex[] fftResults)
        {
            double maxIntensity = CommonMethods.GetRangeMaxIntensity(fftResults, new Tuple<int, int>(0, 512));
            bool result = false;
            fromLastEndingCounter++;

            if (maxIntensity < 20)
            {
                Reset();
                result = true;
            }
            else
            {
                if (maxIntensity < 58)
                {
                    lowInyensityCounter++;
                    if ((double)lowInyensityCounter / ConstValues.EACH_HZ_OF_INDEX > 0.75)
                    {
                        if ((double)fromLastEndingCounter / ConstValues.EACH_HZ_OF_INDEX < 30)
                        {
                            if (maxIntensity < 52.25)
                            {
                                fromLastEndingCounter = 0;
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            if (maxIntensity < 42.25)
                            {
                                fromLastEndingCounter = 0;
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                    else
                    {
                        lowInyensityCounter = 0;
                        result = false;
                    }
                }
                if (fromLastEndingCounter / ConstValues.EACH_HZ_OF_INDEX >= 600) //10mins
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
