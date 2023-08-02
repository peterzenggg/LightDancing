using LightDancing.Common;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightDancing.MusicAnalysis
{
    public class FrequencyBandSetup
    {
        private const double MAX_INTENSITY_HIT_RATE = 0.9;
        private const double MAX_NON_HIT_RATE = 0.7;
        private const int NON_HIT_COUNT = 50;
        private const int DEFAULT_THRESHOLD = 30;

        private readonly List<double> nonHitIntensities = new List<double>();
        private readonly Tuple<int, int> rangeIndex;
        private readonly int threshold;

        private double previousIntensity;
        private double maxIntensity;
        private bool isHited;


        public FrequencyBandSetup(int threshold, int eachHzIndex, Tuple<int, int> frequencyBand)
        {
            this.threshold = threshold;
            rangeIndex = Tuple.Create(frequencyBand.Item1 / eachHzIndex, frequencyBand.Item2 / eachHzIndex);
        }

        public FrequencyBandSetup(Tuple<int, int> frequencyBand)
        {
            threshold = DEFAULT_THRESHOLD;
            rangeIndex = Tuple.Create(frequencyBand.Item1 / ConstValues.EACH_HZ_OF_INDEX, frequencyBand.Item2 / ConstValues.EACH_HZ_OF_INDEX);
        }

        /// <summary>
        /// To check the specific frequency band has beat or not.
        /// </summary>
        /// <param name="fftComplex">FFT sample</param>
        /// <returns>boolean for is beat or not</returns>
        public bool IsBeat(Complex[] fftComplex)
        {
            double currentIntensity = CommonMethods.GetRangeMaxIntensity(fftComplex, rangeIndex);
            currentIntensity = currentIntensity < threshold ? 0 : currentIntensity;
            bool result = false;

            if (currentIntensity > maxIntensity)
            {
                maxIntensity = currentIntensity;
                previousIntensity = currentIntensity;
            }

            if (previousIntensity > currentIntensity)
            {
                if (previousIntensity > maxIntensity * MAX_INTENSITY_HIT_RATE && !isHited)
                {
                    isHited = true;
                    result = true;
                    nonHitIntensities.Clear();
                }
                else
                {
                    nonHitIntensities.Add(currentIntensity);
                }
            }
            else
            {
                isHited = false;
                nonHitIntensities.Add(currentIntensity);
            }

            previousIntensity = currentIntensity;

            ResetNonHitIntensities();

            return result;
        }

        private void ResetNonHitIntensities()
        {
            if (nonHitIntensities.Count >= NON_HIT_COUNT)
            {
                double intensitiesAverage = nonHitIntensities.Average();

                if (intensitiesAverage > maxIntensity * MAX_NON_HIT_RATE)
                {
                    maxIntensity = intensitiesAverage;
                    previousIntensity = intensitiesAverage;
                }

                nonHitIntensities.Clear();
            }
        }

        internal void Reset()
        {
            nonHitIntensities.Clear();
            previousIntensity = 0;
            maxIntensity = 0;
            isHited = false;
        }

        public double GetMaxIntensity(Complex[] fftComplex)
        {
            return CommonMethods.GetRangeMaxIntensity(fftComplex, rangeIndex);
        }

        public double GetLogrithmicAverageIntensity_Low(Complex[] fftComplex)
        {
            return CommonMethods.GetLogarithmicAverageIntensity_Low(fftComplex, rangeIndex);
        }

        public double GetLogrithmicAverageIntensity_High(Complex[] fftComplex)
        {
            return CommonMethods.GetLogarithmicAverageIntensity_High(fftComplex, rangeIndex);
        }

        public double GetLogrithmicMaximumIntensity_Low(Complex[] fftComplex)
        {
            return CommonMethods.GetLogarithmicMaximumIntensity_Low(fftComplex, rangeIndex);
        }
    }
}
