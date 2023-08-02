using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.MusicAnalysis;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.MLs
{
    public class GrenreHelper
    {
        private const int CALCULATED_TIME = 2000;
        private DurationTimer calculatedTimer;

        private readonly List<List<double>> allIntensities = new List<List<double>>();
        private readonly List<Tuple<int, int>> frequencyFeatures = new List<Tuple<int, int>>()
        {
            { Tuple.Create(0, 0) },     // 0~46 Hz
            { Tuple.Create(1, 1) },     // 46~92 Hz
            { Tuple.Create(2, 2) },     // 92~138 Hz
            { Tuple.Create(3, 4) },     // 138~184 Hz
            { Tuple.Create(5, 10) },    // 184~506 Hz
            { Tuple.Create(11, 21) },   // 506~1012 Hz
            { Tuple.Create(22, 42) },   //1021~1978 Hz
            { Tuple.Create(43, 85) },   //1978~3956 Hz
            { Tuple.Create(85, 171) },  //3956~7912 Hz
            { Tuple.Create(171, 348) }, //7912~16008 Hz
        };
        private readonly Dictionary<Genres, int> allPredictData = new Dictionary<Genres, int>();

        private DateTime startTime;

        /// <summary>
        /// MEMO: This ML still testing, need to wait new dataset.
        /// </summary>
        public GrenreHelper()
        {

        }

        /// <summary>
        /// Get most count of genre
        /// </summary>
        /// <returns></returns>
        public Genres GetGenre()
        {
            if (allPredictData.Count == 0)
            {
                return Genres.Ambient;
            }
            else
            {
                var gerne = allPredictData.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                System.Diagnostics.Debug.WriteLine(gerne);
                System.Diagnostics.Debug.WriteLine("=====================" + string.Join(",", allPredictData) + "=====================");
                return gerne;
            }
        }

        /// <summary>
        /// Collent and predict the genre data.
        /// </summary>
        /// <param name="fftResults">FFT data</param>
        public void UpdateGenre(Dictionary<FFTSampleType, Complex[]> fftResults)
        {
            if (fftResults.ContainsKey(FFTSampleType.Center))
            {
                if (calculatedTimer == null)
                {
                    calculatedTimer = new DurationTimer(CALCULATED_TIME);
                    startTime = DateTime.Now;
                }

                var fftValues = fftResults[FFTSampleType.Center];
                List<double> intensities = new List<double>();
                foreach (var features in frequencyFeatures)
                {
                    var max = CommonMethods.GetRangeMaxIntensity(fftValues, features);
                    intensities.Add(max);
                }

                if (intensities.All(x => x == 0))
                {
                    return;
                }

                allIntensities.Add(intensities);

                if (calculatedTimer.IsAvailable())
                {
                    var averagedData = allIntensities[0].Select((v, c) => allIntensities.Average(r => r[c])).ToList();
                    Type examType = typeof(GenreData);
                    var properties = examType.GetProperties();
                    GenreData sampleData = new GenreData();

                    for (int i = 0; i < averagedData.Count; i++)
                    {
                        properties[i + 2].SetValue(sampleData, (float)averagedData[i]);
                    }

                    var predictionResult = ConsumeModel.Predict(sampleData);
                    var genre = Convert(predictionResult.Prediction);

                    if (allPredictData.ContainsKey(genre))
                    {
                        allPredictData[genre]++;
                    }
                    else
                    {
                        allPredictData.Add(genre, 1);
                    }
                    AdjustCollectSpeed();
                }
            }
        }

        /// <summary>
        /// Allow AnimationGroupBase to change MLs path
        /// </summary>
        /// <param name="path"></param>
        public void SetMLsFilePath(string path)
        {
            ConsumeModel.SetMLsFilePath(path);
        }

        /// <summary>
        /// 0~8 second will collect and average the data by 2 seconds
        /// 9~16 second will collect and average the data by 1 seconds
        /// 17~24 second will collect and average the data by 0.5 seconds
        /// 25~74 second will collect and average the data by 0.1 seconds
        /// 75~ second will reset the collect to 2 seconds again
        /// </summary>
        private void AdjustCollectSpeed()
        {
            var spendTime = (DateTime.Now - startTime).TotalSeconds;
            var currentDuration = calculatedTimer.GetDuration();
            if (spendTime > 8 && spendTime <= 16 && currentDuration != 1000)
            {
                calculatedTimer.UpdateDuration(1000);
            }
            else if (spendTime > 16 && spendTime <= 20 && currentDuration != 500)
            {
                calculatedTimer.UpdateDuration(500);
            }
            else if (spendTime > 20)
            {
                if (spendTime < 75)
                {
                    if (currentDuration != 100)
                    {
                        calculatedTimer.UpdateDuration(100);
                    }
                }
                else
                {
                    if (currentDuration != 2000)
                    {
                        calculatedTimer.UpdateDuration(2000);
                    }
                }
            }
        }

        private Genres Convert(string genre)
        {
            return genre switch
            {
                "Ambient" => Genres.Ambient,
                "Ballad" => Genres.Ballad,
                "Classic" => Genres.Classic,
                "EDM" => Genres.EDM,
                "Hiphop" => Genres.HipHop,
                "Jazz+Blues" => Genres.Jazz,
                "Jazz" => Genres.Jazz,
                "Rock+Metal+Punk" => Genres.Rock,
                "Rock" => Genres.Rock,
                _ => throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, genre, "GrenreHelper.Convert")),
            };
        }

        internal void Reset()
        {
            calculatedTimer = null;
            allIntensities.Clear();
            allPredictData.Clear();
        }
    }
}
