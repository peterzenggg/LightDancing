using LightDancing.Enums;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.MusicAnalysis
{
    public class SampleAggregator : ISampleProvider
    {
        // FFT
        public event EventHandler<FftEventArgs> FFTsCalculated;
        private readonly Dictionary<FFTSampleType, Complex[]> fftBuffers;
        private readonly Dictionary<FFTSampleType, double[]> audioSignals;
        private readonly TimeSpan[] fftTime;
        private readonly FftEventArgs fftArgs;

        private int fftPos;
        private readonly int fftLength;
        private readonly int m;
        private readonly ISampleProvider source;

        private readonly int channels;

        /// <summary>
        /// This constructor is use by load file.
        /// </summary>
        /// <param name="source">Audio load file sourcr</param>
        /// <param name="fftLength">FFT length</param>
        public SampleAggregator(ISampleProvider source, int fftLength = 1024)
        {
            channels = source.WaveFormat.Channels;
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;

            fftBuffers = new Dictionary<FFTSampleType, Complex[]>();
            audioSignals = new Dictionary<FFTSampleType, double[]>();

            foreach (FFTSampleType enumType in Enum.GetValues(typeof(FFTSampleType)))
            {
                fftBuffers.Add(enumType, new Complex[fftLength]);
                audioSignals.Add(enumType, new double[fftLength]);
            }

            fftTime = new TimeSpan[fftLength];
            fftArgs = new FftEventArgs(fftBuffers, fftTime, audioSignals);
            this.source = source;
        }

        /// <summary>
        /// This constructor is use by record system audio
        /// </summary>
        /// <param name="fftLength">FFT length</param>
        public SampleAggregator(int fftLength = 1024)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;

            fftBuffers = new Dictionary<FFTSampleType, Complex[]>();
            audioSignals = new Dictionary<FFTSampleType, double[]>();

            foreach (FFTSampleType enumType in Enum.GetValues(typeof(FFTSampleType)))
            {
                fftBuffers.Add(enumType, new Complex[fftLength]);
                audioSignals.Add(enumType, new double[fftLength]);
            }

            fftTime = new TimeSpan[fftLength];
            fftArgs = new FftEventArgs(fftBuffers, fftTime, audioSignals);
        }

        private bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }


        private readonly object locker = new object();

        /// <summary>
        /// TODO: Calculated and return intensitys in this method.
        /// </summary>
        /// <param name="sampleDatas"></param>
        /// <param name="time"></param>
        public void Add(Dictionary<FFTSampleType, float> sampleDatas, TimeSpan time)
        {
            if (FFTsCalculated != null)
            {
                lock (locker)
                {
                    foreach (var sampleData in sampleDatas)
                    {
                        fftBuffers[sampleData.Key][fftPos].X = (float)(sampleData.Value * FastFourierTransform.HammingWindow(fftPos, fftLength));
                        fftBuffers[sampleData.Key][fftPos].Y = 0;
                        audioSignals[sampleData.Key][fftPos] = sampleData.Value;
                    }

                    fftTime[fftPos] = time;
                    fftPos++;

                    if (fftPos >= fftTime.Length)
                    {
                        fftPos = 0;
                        // 1024 = 2^10

                        foreach (var complexs in fftBuffers.Values)
                        {
                            FastFourierTransform.FFT(true, m, complexs);
                        }

                        FFTsCalculated(this, fftArgs);
                    }
                }
            }
        }


        public WaveFormat WaveFormat => source.WaveFormat;

        /// <summary>
        /// Overwirte the Read method when read with audio file
        /// </summary>
        /// <param name="buffer">Audio Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <returns></returns>
        public int Read(float[] buffer, int offset, int count)
        {
            WaveStream wavrStream = (WaveStream)this.source;
            var currentTime = wavrStream.CurrentTime;

            var samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += channels)
            {
                var leftSample = buffer[n + offset];
                var rightSample = buffer[n + offset + channels - 1];
                var centerSample = (leftSample + rightSample) / 2;

                Dictionary<FFTSampleType, float> datas = new Dictionary<FFTSampleType, float>()
                {
                    { FFTSampleType.Left, leftSample},
                    { FFTSampleType.Right, rightSample},
                    { FFTSampleType.Center, centerSample},
                };

                Add(datas, currentTime);
            }

            return samplesRead;
        }
    }

    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(Dictionary<FFTSampleType, Complex[]> result, TimeSpan[] resultTime, Dictionary<FFTSampleType, double[]> audioSignals)
        {
            Result = result;
            ResultTime = resultTime;
            Signals = audioSignals;
        }
        public Dictionary<FFTSampleType, Complex[]> Result { get; private set; }

        public Dictionary<FFTSampleType, double[]> Signals { get; private set; }

        public TimeSpan[] ResultTime { get; private set; }
    }
}
