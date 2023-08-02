using Force.DeepCloner;
using LightDancing.Enums;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.MusicAnalysis
{
    public class MusicFeatures
    {
        public Dictionary<FFTSampleType, Complex[]> FftResults { get; set; }

        public Dictionary<FFTSampleType, double[]> AudioSignals { get; set; }
    }

    public class FftCatcher : IDisposable
    {
        private readonly SampleAggregator sampleAggregator;

        private WasapiLoopbackCapture capture;
        private MusicFeatures musicFeatures;
        private DateTime recordTime = DateTime.MinValue;


        /// <summary>
        /// Real time to get the system of auido data and convert to FFT data.
        /// </summary>
        public FftCatcher()
        {
            sampleAggregator = new SampleAggregator();
            sampleAggregator.FFTsCalculated += SampleAggregator_FFTsCalculated;
        }

        /// <summary>
        /// Start recording the audio from system and calculate to FFT data.
        /// </summary>
        public void Start()
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += Capture_RecordingStopped;
            capture.StartRecording();
        }

        /// <summary>
        /// Stop recroding the auido
        /// </summary>
        public void Stop()
        {
            if (capture != null)
            {
                capture.StopRecording();
                capture.Dispose();
            }
        }

        /// <summary>
        /// Get music features, FFT data and wave data
        /// </summary>
        /// <returns></returns>
        public MusicFeatures GetMusicFeatures()
        {
            return musicFeatures;
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            capture.Dispose();
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recordTime == DateTime.MinValue)
            {
                recordTime = DateTime.Now;
            }

            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            int bufferIncrement = capture.WaveFormat.BlockAlign;
            int rightChannelIndex = bufferIncrement / 2;

            if (buffer.All(x => x == 0))
                return;

            for (int index = 0; index < bytesRecorded; index += bufferIncrement)
            {
                float leftSample = BitConverter.ToSingle(buffer, index);
                float rightSample = BitConverter.ToSingle(buffer, index + rightChannelIndex);
                float centerSample = (leftSample + rightSample) / 2;
                float leftBackSample = leftSample * 20;  // strengthen the sample
                float rightBackSample = rightSample * 20; // strengthen the sample

                Dictionary<FFTSampleType, float> datas = new Dictionary<FFTSampleType, float>()
                {
                    { FFTSampleType.Left, leftSample},
                    { FFTSampleType.Right, rightSample},
                    { FFTSampleType.Center, centerSample},
                    { FFTSampleType.LeftBack, leftBackSample},
                    { FFTSampleType.RightBack, rightBackSample},
                };

                TimeSpan timeSpan = DateTime.Now - recordTime;
                sampleAggregator.Add(datas, timeSpan);
            }
        }

        private void SampleAggregator_FFTsCalculated(object sender, FftEventArgs e)
        {
            musicFeatures = new MusicFeatures()
            {
                FftResults = e.Result.DeepClone(),
                AudioSignals = e.Signals.DeepClone(),
            };
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
