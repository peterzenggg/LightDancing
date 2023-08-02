using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Smart.Helper
{
    public class Speed
    {
        private double delayTime = 40;

        private readonly List<double> bpmCounter = new List<double>();

        private readonly List<double> signalSamples = new List<double>();

        private bool previousIsBeat;

        private const double THRESHOLD = 0.4;

        private const int TIME = 250; //250s

        /// <summary>
        /// Calculate speed of a song
        /// </summary>
        /// <param name="signals"> wave signal of audio </param>
        /// <returns></returns>
        public int GetSpeed(Dictionary<FFTSampleType, double[]> signals)
        {
            double[] signal = signals[FFTSampleType.Center];
            int speed = CalculateSpeed(signal);

            return speed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signals"> audio signal </param>
        /// <returns></returns>
        private int CalculateSpeed(double[] signals)
        {
            AbsSig(signals);
            double ave = signals.Average();

            int newAccent = IsAccent(ave);

            if (bpmCounter.Count >= TIME)
            {
                bpmCounter.RemoveAt(0);
            }
            bpmCounter.Add(newAccent);

            double bpm = bpmCounter.Sum() * 12;

            int speed = CheckSpeed(bpm);

            if (delayTime > speed)
            {
                delayTime -= 0.1;
                if (newAccent == 1)
                {
                    delayTime -= 2.4;
                }
            }
            else
            {
                delayTime += 0.1;
            }

            if (delayTime > 90)
            {
                delayTime = 90;
            }
            if (delayTime < 0)
            {
                delayTime = 0;
            }

            return (int)delayTime;
        }

        /// <summary>
        /// Get the abs signals
        /// </summary>
        /// <param name="signals"></param>
        private void AbsSig(double[] signals)
        {
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = Math.Abs(signals[i]);
            }
        }

        /// <summary>
        /// If the waveform is a beat
        /// </summary>
        /// <param name="ave"></param>
        /// <returns></returns>
        private int IsAccent(double ave)
        {
            //Get averageLine every 4 samples (0.08s)
            if (signalSamples.Count >= 4)
            {
                signalSamples.RemoveAt(0);
            }
            signalSamples.Add(ave);

            double lineTemp = signalSamples.Average();

            bool isLargerTemp;
            int result = 0;

            if ((ave - lineTemp) / ave > THRESHOLD)
            {
                isLargerTemp = true;

                if (!previousIsBeat)
                {
                    result = 1;
                }
            }
            else
            {
                isLargerTemp = false;
            }

            previousIsBeat = isLargerTemp;

            return result;
        }

        /// <summary>
        /// BPM to speed (ms)
        /// </summary>
        /// <param name="bpm"></param>
        /// <returns></returns>
        private int CheckSpeed(double bpm)
        {
            int speed = 40;

            if (bpm >= 0 && bpm < 10)
            {
                speed = 90;
            }
            else if (bpm >= 10 && bpm < 20)
            {
                speed = 80;
            }
            else if (bpm >= 20 && bpm < 30)
            {
                speed = 70;
            }
            else if (bpm >= 30 && bpm < 50)
            {
                speed = 60;
            }
            else if (bpm >= 50 && bpm < 80)
            {
                speed = 50;
            }
            else if (bpm >= 80 && bpm < 110)
            {
                speed = 40;
            }
            else if (bpm >= 110 && bpm < 200)
            {
                speed = 30;
            }
            else if (bpm >= 200 && bpm < 250)
            {
                speed = 20;
            }
            else if (bpm >= 250 && bpm < 300)
            {
                speed = 10;
            }
            else if (bpm >= 300)
            {
                speed = 0;
            }

            return speed;
        }

        internal void Reset()
        {
            delayTime = 40;
            bpmCounter.Clear();
            signalSamples.Clear();
            previousIsBeat = false;
        }
    }
}
