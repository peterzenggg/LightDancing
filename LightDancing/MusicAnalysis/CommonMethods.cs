using NAudio.CoreAudioApi;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.MusicAnalysis
{
    public class CommonMethods
    {
        /// <summary>
        /// Get the intensity of the Decibel
        /// https://en.wikipedia.org/wiki/Decibel#Perception
        /// https://stackoverflow.com/questions/3058236/how-to-extract-frequency-information-from-samples-from-portaudio-using-fftw-in-c/3059084
        /// https://www.keysight.com/main/editorial.jspx?cc=SE&lc=swe&ckey=566084&nid=-32496.1150429&id=566084
        /// MOMO:
        /// When minDB = -90: 
        /// (1) Played the 0DB's audio, the max intensity will around with 87.
        /// (2) Played the -6DB's audio, the max intensity will around with 80.
        /// (3) Played the -12DB's audio, the max intensity will around with 73.
        /// 
        /// When minDB = -60: (Filter more frequency)
        /// (1) Played the 0DB's audio, the max intensity will around with 80.
        /// (1) Played the -6DB's audio, the max intensity will around with 70.
        /// (1) Played the -12DB's audio, the max intensity will around with 60.
        /// </summary>
        /// <param name="c">The FFT data</param>
        /// <returns>Decibel intensity</returns>
        public static double GetYPosLog(Complex c)
        {
            var amplitude = Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));
            double intensityDB = 20 * amplitude;
            double minDB = -90;
            if (intensityDB < minDB) intensityDB = minDB;
            double percent = 1 - (intensityDB / minDB);

            double yPos = percent * 100;
            return yPos;
        }

        public static double GetYPosLog_60(Complex c)
        {
            var amplitude = Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));
            double intensityDB = 20 * amplitude;
            double minDB = -60;
            if (intensityDB < minDB) intensityDB = minDB;
            double percent = 1 - (intensityDB / minDB);

            double yPos = percent * 100;
            return yPos;
        }

        /// <summary>
        /// Get a freqnecy band of max value
        /// </summary>
        /// <param name="fftComplex">FFT sample</param>
        /// <param name="ranageIndex">Frequency range index</param>
        /// <returns>Max value of this frequency band</returns>
        public static double GetRangeMaxIntensity(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double maxIntensity = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog(fftComplex[i]);

                if (intensity >= maxIntensity)
                {
                    maxIntensity = intensity;
                }
            }
            return maxIntensity;
        }

        public static Tuple<double, int> GetRangeMaxIndex(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double maxIntensity = 0;
            int maxIndex = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog(fftComplex[i]);

                if (intensity >= maxIntensity)
                {
                    maxIntensity = intensity;
                    maxIndex = i;
                }
            }
            return new Tuple<double, int>(maxIntensity, maxIndex);
        }

        /// <summary>
        /// Calculate a frequency band of average
        /// </summary>
        /// <param name="fftComplex">FFT sample</param>
        /// <param name="ranageIndex">Frequency range index</param>
        /// <returns>Average value of this frequency band </returns>
        public static double GetRangeAverageIntensity(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double sumIntensity = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog(fftComplex[i]);
                sumIntensity += intensity;
            }

            double average = sumIntensity / ((endFrequency - startFrequency) + 1);
            return average;
        }

        public static T RandomEnumFromSelf<T>(T withoutEnum)
        {
            List<T> list = new List<T>();
            foreach (T enumType in Enum.GetValues(typeof(T)))
            {
                if (!enumType.Equals(withoutEnum))
                {
                    list.Add(enumType);
                }
            }
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int index = random.Next(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// Random choose one animation within enum type
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Enum type</returns>
        public static T RandomEnumFromSelf<T>()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            var values = Enum.GetValues(typeof(T));

            return (T)values.GetValue(random.Next(values.Length));
        }

        /// <summary>
        /// Random choose one animation from source and without parameter
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="source">Enum sources</param>
        /// <param name="withoutEnum">Whithout with this enum</param>
        /// <returns>Enum type</returns>
        public static T RandomEnumFromSource<T>(List<T> source, T withoutEnum)
        {
            List<T> list = new List<T>();
            foreach (T enumType in source)
            {
                if (!enumType.Equals(withoutEnum))
                {
                    list.Add(enumType);
                }
            }

            Random random = new Random(Guid.NewGuid().GetHashCode());
            int index = random.Next(0, list.Count);
            return list[index];
        }

        public static T RandomEnumFromSource<T>(List<T> source)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int index = random.Next(0, source.Count);
            return source[index];
        }

        public static double GetLogarithmicAverageIntensity_Low(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double sumIntensity = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            int currentLogIndex = GetLogarithmicIndex((startFrequency + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);
            List<double> totalLogIntensity = new List<double>();
            List<double> currentLogIntensity = new List<double>();

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog_60(fftComplex[i]);
                sumIntensity += intensity;

                var thisLogIndex = GetLogarithmicIndex((i + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);

                if (thisLogIndex == currentLogIndex)
                {
                    currentLogIntensity.Add(intensity);
                }
                else
                {
                    double ave = currentLogIntensity.Average();
                    currentLogIntensity.Clear();
                    totalLogIntensity.Add(ave);
                    currentLogIndex = thisLogIndex;
                    currentLogIntensity.Add(intensity);
                }

                if (i == endFrequency)
                {
                    double ave = currentLogIntensity.Average();
                    currentLogIntensity.Clear();
                    totalLogIntensity.Add(ave);
                }
            }

            double average = totalLogIntensity.Average();
            return average;
        }

        public static double GetLogarithmicAverageIntensity_High(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double sumIntensity = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            int currentLogIndex = GetLogarithmicIndex((startFrequency + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);
            List<double> totalLogIntensity = new List<double>();
            List<double> currentLogIntensity = new List<double>();

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog(fftComplex[i]);
                sumIntensity += intensity;

                var thisLogIndex = GetLogarithmicIndex((i + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);

                if (thisLogIndex == currentLogIndex)
                {
                    currentLogIntensity.Add(intensity);
                }
                else
                {
                    double ave = currentLogIntensity.Average();
                    currentLogIntensity.Clear();
                    totalLogIntensity.Add(ave);
                    currentLogIndex = thisLogIndex;
                    currentLogIntensity.Add(intensity);
                }

                if (i == endFrequency)
                {
                    double ave = currentLogIntensity.Average();
                    currentLogIntensity.Clear();
                    totalLogIntensity.Add(ave);
                }
            }

            double average = totalLogIntensity.Average();
            return average;
        }

        public static double GetLogarithmicMaximumIntensity_Low(Complex[] fftComplex, Tuple<int, int> ranageIndex)
        {
            double sumIntensity = 0;
            int startFrequency = ranageIndex.Item1;
            int endFrequency = ranageIndex.Item2;

            int currentLogIndex = GetLogarithmicIndex((startFrequency + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);
            List<double> currentLogIntensity = new List<double>();
            double maxIntensity = 0;

            for (int i = startFrequency; i <= endFrequency; i++)
            {
                var intensity = GetYPosLog_60(fftComplex[i]);
                sumIntensity += intensity;

                var thisLogIndex = GetLogarithmicIndex((i + 1) * Common.ConstValues.EACH_HZ_OF_INDEX);

                if (thisLogIndex == currentLogIndex)
                {
                    currentLogIntensity.Add(intensity);
                    if (startFrequency == endFrequency)
                    {
                        maxIntensity = maxIntensity < intensity ? intensity : maxIntensity;
                    }
                }
                else
                {
                    double ave = currentLogIntensity.Average();
                    currentLogIntensity.Clear();
                    currentLogIndex = thisLogIndex;
                    currentLogIntensity.Add(intensity);
                    maxIntensity = maxIntensity < ave ? ave : maxIntensity;
                }
            }
            return maxIntensity;
        }

        public static int GetLogarithmicIndex(int frequency)
        {
            return (int)(Math.Log2((double)frequency / 6.1035) * 2.5 + 1);
        }

        public static double GetVolume()
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            return device.AudioMeterInformation.MasterPeakValue;
        }
    }
}
