using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.MLs;
using LightDancing.MusicAnalysis;
using LightDancing.OpenTKs.Animations;
using LightDancing.Smart.Helper;
using LightDancing.Syncs;
using NAudio.Dsp;
using System;
using System.Collections.Generic;

namespace LightDancing.Smart
{
    public abstract class AnimationGroupBase
    {
        protected readonly FrequencyBandSetup frequencySetup = new FrequencyBandSetup(Tuple.Create(60, 450));
        protected readonly FrequencyBandSetup mid_frequencyBand = new FrequencyBandSetup(Tuple.Create(500, 3000));
        protected readonly FrequencyBandSetup high_frequencyBand = new FrequencyBandSetup(Tuple.Create(3000, 16000));

        private readonly Speed speedHelper = new Speed();
        private readonly MusicEndDetector endDetector = new MusicEndDetector();
        private AnimationGroup animationGroupHistory;
        protected GrenreHelper genreHelper = new GrenreHelper();

        private readonly Accumulative centroidAccumulative = new Accumulative();
        protected readonly Accumulative intensitiesAccumulative = new Accumulative();
        protected Sections currentSection = Sections.Intro;
        protected double maxintensity = 0;
        protected const int DEFAULT_SPEED = 40;

        protected readonly BackgroundDetail background;
        protected readonly ProtagonistDetail protagonist;
        protected readonly PanoramaDetail panorama;

        protected FrequencyBand[] order = new FrequencyBand[3];

        protected abstract void InitAniamtionUnion(int speed);

        public AnimationGroupBase()
        {
            background = new BackgroundDetail() { Counter = 0, Mode = null };
            protagonist = new ProtagonistDetail() { Counter = 0, Mode = null };
            panorama = new PanoramaDetail() { Counter = 0, Mode = null };
        }

        /// <summary>
        /// Process the group animation and get commands
        /// </summary>
        /// <param name="musicFeatures"></param>
        /// <returns></returns>
        public void ChangedShader(MusicFeatures musicFeatures)
        {
            animationGroupHistory = new AnimationGroup(background.GetShader(), panorama.GetShader(), protagonist.GetShader());
            int speed = speedHelper.GetSpeed(musicFeatures.AudioSignals);
            if (endDetector.SongEndDetection(musicFeatures.FftResults[FFTSampleType.Center]))
            {
                Reset();
                InitAniamtionUnion(speed);
            }

            order = GetIntensityOrder(musicFeatures.FftResults[FFTSampleType.Center]);

            double centroid = GetAllOctavesCentroid(musicFeatures.FftResults[FFTSampleType.Center]);
            double intensity = GetAllOctavesAverage(musicFeatures.FftResults[FFTSampleType.Center]);
            centroidAccumulative.Add(centroid);
            intensitiesAccumulative.Add(intensity);

            if (frequencySetup.IsBeat(musicFeatures.FftResults[FFTSampleType.Center]))
            {
                genreHelper.UpdateGenre(musicFeatures.FftResults);
                bool sectionChanged = IsSectionChanged(speed);
                UpdateAnimationsDetail(sectionChanged, speed, genreHelper.GetGenre());

                centroidAccumulative.CalculateValues();
                intensitiesAccumulative.CalculateValues();
            }

            AnimationGroup newAnimationGroup = new AnimationGroup(background.GetShader(), panorama.GetShader(), protagonist.GetShader());

            if (animationGroupHistory.Background != newAnimationGroup.Background || animationGroupHistory.Panorama != newAnimationGroup.Panorama || animationGroupHistory.Protagonist != newAnimationGroup.Protagonist)
            {
                OpenTKSyncHelper.Instance.ChangeShader(newAnimationGroup);
            }
        }

        /// <summary>
        /// Allow AnimationBase to change MLs path
        /// </summary>
        /// <param name="path"></param>
        public void SetMLsFilePath(string path)
        {
            genreHelper.SetMLsFilePath(path);
        }

        private void Reset()
        {
            genreHelper.Reset();
            frequencySetup.Reset();
            speedHelper.Reset();
            endDetector.Reset();
            centroidAccumulative.Reset();
            intensitiesAccumulative.Reset();

            currentSection = Sections.Intro;
            maxintensity = 0;

            background.Reset();
            protagonist.Reset();
            panorama.Reset();

            OpenTKSyncHelper.Instance.ResetFrequencyBands();
        }

        /// <summary>
        /// Get centroid of 10 octaves
        /// </summary>
        /// <param name="fftComplex"></param>
        /// <returns></returns>
        private double GetAllOctavesCentroid(Complex[] fftComplex)
        {
            double totalIntensity = 0, centroid = 0;
            int currentIndex = 0;

            foreach (var octave in DictionaryConst.OCTAVE_RANGES)
            {
                double intensity = CommonMethods.GetRangeAverageIntensity(fftComplex, octave.Value);
                totalIntensity += intensity;
                centroid += currentIndex * intensity;
                currentIndex++;
            }

            if (totalIntensity == 0)
            {
                centroid = 0.5;
            }
            else
            {
                centroid /= 9 * totalIntensity;
            }

            return centroid;
        }

        /// <summary>
        /// Get average intensity of 10 octaves
        /// </summary>
        /// <param name="fftComplex">FFT data</param>
        /// <returns>Average of 10 octaves</returns>
        private double GetAllOctavesAverage(Complex[] fftComplex)
        {
            double totalIntensity = 0;

            foreach (var octave in DictionaryConst.OCTAVE_RANGES)
            {
                double intensity = CommonMethods.GetRangeAverageIntensity(fftComplex, octave.Value);
                totalIntensity += intensity;
            }

            totalIntensity /= DictionaryConst.OCTAVE_RANGES.Count;

            return totalIntensity;
        }

        /// <summary>
        /// Update the current setcion also notify if section changed
        /// </summary>
        /// <param name="speed">The speed from Audio signals</param>
        /// <returns></returns>
        private bool IsSectionChanged(int speed)
        {
            bool changed = true;
            double centroidVariance = Math.Abs(centroidAccumulative.GetVariance());
            double intensityVariance = intensitiesAccumulative.GetVariance();

            double averageIntensity = intensitiesAccumulative.GetCurrentAverage();
            maxintensity = maxintensity < averageIntensity ? averageIntensity : maxintensity;

            if (speed > 75 && averageIntensity < maxintensity * 0.3)
            {
                currentSection = Sections.Intro;
            }
            else if (speed > 50 && averageIntensity < maxintensity * 0.7 && currentSection == Sections.Hook)
            {
                currentSection = Sections.Verse;
            }
            else if (speed < 35 && averageIntensity > maxintensity * 0.8 && currentSection != Sections.Hook)
            {
                currentSection = Sections.Hook;
            }
            else if (Math.Abs(intensityVariance / intensitiesAccumulative.PreviousAverage) > 0.3 && centroidVariance > 0.04 && intensitiesAccumulative.PreviousAverage != 0)
            {
                if ((intensityVariance / intensitiesAccumulative.PreviousAverage) > 0)
                {
                    switch (currentSection)
                    {
                        case Sections.Intro:
                            currentSection = Sections.Verse;
                            break;
                        case Sections.Verse:
                        case Sections.Hook:
                            currentSection = Sections.Hook;
                            break;
                        default:
                            throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, currentSection, "AnimationGroupBase.IsSectionChanged"));
                    }
                }
                else
                {
                    switch (currentSection)
                    {
                        case Sections.Intro:
                        case Sections.Verse:
                            currentSection = Sections.Intro;
                            break;
                        case Sections.Hook:
                            currentSection = Sections.Verse;
                            break;
                        default:
                            throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, currentSection, "AnimationGroupBase.IsSectionChanged"));
                    }
                }
            }
            else
            {
                changed = false;
            }

            return changed;
        }

        /// <summary>
        /// Update background, protagonist and panorama's detail
        /// </summary>
        /// <param name="sectionChanged">Is section changed</param>
        /// <param name="colors">The genre colors</param>
        /// <param name="speed">Current speed</param>
        private void UpdateAnimationsDetail(bool sectionChanged, int speed, Genres genres)
        {
            int panoramaCounter = 64;
            int backgroundCounter = 32;

            switch (genres)
            {
                case Genres.Ambient:
                    backgroundCounter = 480;
                    break;
                case Genres.Ballad:
                case Genres.Jazz:
                case Genres.Classic:
                    panoramaCounter = 96;
                    backgroundCounter = 96;
                    break;
                case Genres.EDM:
                case Genres.Rock:
                case Genres.HipHop:
                    panoramaCounter = 64;
                    backgroundCounter = 32;
                    break;
            }

            if (sectionChanged || (panorama.Mode != null && panorama.Counter >= panoramaCounter))
            {
                ResetAllDetail();
                InitAniamtionUnion(speed);
            }
            else if (background.Mode != null && background.Counter >= backgroundCounter || protagonist.Mode != null && protagonist.Counter >= backgroundCounter)
            {
                background.Reset();
                protagonist.Reset();
                InitAniamtionUnion(speed);
            }
            else
            {
                background.Counter++;
                protagonist.Counter++;
                panorama.Counter++;
            }
        }

        private void ResetAllDetail()
        {
            background.Reset();
            protagonist.Reset();
            panorama.Reset();
        }



        /// <summary>
        /// Expand the color to 10
        /// </summary>
        /// <param name="colors">Two Colors</param>
        /// <returns></returns>
        protected List<ColorRGB> ExpandColors(List<ColorRGB> colors)
        {
            List<ColorRGB> result = new List<ColorRGB>();

            if (colors.Count > 2)
            {
                // Make sure only two color select
                colors = colors.GetRange(0, 2);
            }

            for (int i = 0; i < 5; i++)
            {
                result.AddRange(colors);
            }

            return result;
        }

        private FrequencyBand[] GetIntensityOrder(Complex[] fftResult)
        {
            double lowIntensity = frequencySetup.GetMaxIntensity(fftResult);
            double midIntensity = mid_frequencyBand.GetMaxIntensity(fftResult);
            double highIntensity = high_frequencyBand.GetMaxIntensity(fftResult);
            FrequencyBand[] order = new FrequencyBand[3];

            List<double> intensities = new List<double>()
            {
                lowIntensity,
                midIntensity,
                highIntensity,
            };

            for (int i = 0; i < intensities.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < intensities.Count; j++)
                {
                    if (intensities[i] >= intensities[j] && i != j)
                    {
                        count++;
                    }
                }
                if (order[2 - count] == 0)
                {
                    order[2 - count] = (FrequencyBand)(i + 1);
                }
                else if (order[2 - count + 1] == 0)
                {
                    order[2 - count + 1] = (FrequencyBand)(i + 1);
                }
                else
                {
                    order[2 - count + 2] = (FrequencyBand)(i + 1);
                }
            }

            return order;
        }
    }

    /// <summary>
    /// Auto process the smart algorithm
    /// </summary>
    public class AnimationGroupAuto : AnimationGroupBase
    {
        private readonly Genres? customGenre;
        public AnimationGroupAuto() : base()
        {
            InitAniamtionUnion(DEFAULT_SPEED);
        }

        public AnimationGroupAuto(Genres genre) : base()
        {
            customGenre = genre;
            InitAniamtionUnion(DEFAULT_SPEED);
        }

        private Genres GetCurrentGenre()
        {
            if (customGenre != null)
                return customGenre.Value;
            else
            {
                return genreHelper.GetGenre();
            }
        }

        protected override void InitAniamtionUnion(int speed)
        {
            double averageIntensity = intensitiesAccumulative.GetCurrentAverage();
            Genres currentGenre = GetCurrentGenre();

            switch (currentGenre)
            {
                case Genres.Ambient:
                    background.InitCurrentMode(DictionaryConst.BACKGROUND_KALE);
                    break;

                case Genres.Classic:
                case Genres.Jazz:
                    InitGroupsCurrentMode(averageIntensity, DictionaryConst.BACKGROUND_ALL, DictionaryConst.PANORAMA_ALL, DictionaryConst.PROTAGONIST_ALL);
                    break;

                case Genres.Ballad:
                    InitGroupsCurrentMode(averageIntensity, DictionaryConst.BACKGROUND_CARTOON, DictionaryConst.PANORAMA_CARTOON, DictionaryConst.PROTAGONIST_CARTOON);
                    break;

                case Genres.Rock:
                    InitGroupsCurrentMode(averageIntensity, DictionaryConst.BACKGROUND_POPART, DictionaryConst.PANORAMA_POPART, DictionaryConst.PROTAGONIST_POPART);
                    break;
                case Genres.EDM:
                    InitGroupsCurrentMode(averageIntensity, DictionaryConst.BACKGROUND_GEO, DictionaryConst.PANORAMA_GEO, DictionaryConst.PROTAGONIST_GEO);
                    break;
                case Genres.HipHop:
                    background.InitCurrentMode(DictionaryConst.BACKGROUND_TRIP);
                    break;

                default:
                    throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, currentSection, "AnimationGroupBase.IsSectionChanged"));
            }

            OpenTKSyncHelper.Instance.ChangeColorSet(ColorFromGenre(currentGenre));
            OpenTKSyncHelper.Instance.SetSpeed(speed);
        }

        private void InitGroupsCurrentMode(double averageIntensity, List<BackgroundShaders> backgroundList, List<PanoramaShaders> panoramaList, List<ProtagonistShaders> protagonistList)
        {
            switch (currentSection)
            {
                case Sections.Intro:
                    if (order[0] == FrequencyBand.low)
                    {
                        background.InitCurrentMode(backgroundList);
                    }
                    else if (order[0] == FrequencyBand.mid)
                    {
                        panorama.InitCurrentMode(panoramaList);
                    }
                    else
                    {
                        protagonist.InitCurrentMode(protagonistList);
                    }
                    break;

                case Sections.Verse:
                    if (order[0] == FrequencyBand.low)
                    {
                        background.InitCurrentMode(backgroundList);
                    }
                    else if (order[0] == FrequencyBand.mid)
                    {
                        panorama.InitCurrentMode(panoramaList);
                    }
                    else
                    {
                        protagonist.InitCurrentMode(protagonistList);
                    }

                    if (order[1] == FrequencyBand.low)
                    {
                        background.InitCurrentMode(backgroundList);
                    }
                    else if (order[1] == FrequencyBand.mid)
                    {
                        panorama.InitCurrentMode(panoramaList);
                    }
                    else
                    {
                        protagonist.InitCurrentMode(protagonistList);
                    }
                    break;

                case Sections.Hook:
                    background.InitCurrentMode(backgroundList);
                    panorama.InitCurrentMode(panoramaList);
                    protagonist.InitCurrentMode(protagonistList);
                    break;

                default:
                    throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, currentSection, "AnimationGroupBase.IsSectionChanged"));
            }
        }

        private void InitProtagonist(double averageIntensity)
        {
            if (averageIntensity < maxintensity * 0.2)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
            else if (averageIntensity >= maxintensity * 0.2 && averageIntensity < maxintensity * 0.4)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
            else if (averageIntensity >= maxintensity * 0.4 && averageIntensity < maxintensity * 0.6)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
            else //(averageIntensity >= maxintensity * 0.6)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
        }

        private void InitSlowSongProtagonist(double averageIntensity)
        {
            if (averageIntensity < maxintensity * 0.3)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
            else if (averageIntensity >= maxintensity * 0.3 && averageIntensity < maxintensity * 0.6)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
            else//(averageIntensity >= maxintensity * 0.6)
            {
                protagonist.InitCurrentMode(DictionaryConst.PROTAGONIST_ALL);
            }
        }

        private List<ColorRGB> ColorFromGenre(Genres genre)
        {
            List<ColorRGB> colors = genre switch
            {
                Genres.Ambient => new List<ColorRGB>()
                {
                    new ColorRGB(65, 6, 2),
                    new ColorRGB(30, 3, 1),
                    new ColorRGB(66, 14, 13),
                    new ColorRGB(66, 8, 15),
                    new ColorRGB(66, 10, 40),
                    new ColorRGB(44, 5, 39),
                    new ColorRGB(29, 9, 44),
                    new ColorRGB(16, 0, 44),
                    new ColorRGB(15, 17, 44),
                    new ColorRGB(0, 6, 24),
                },
                Genres.Jazz => new List<ColorRGB>()
                {
                    new ColorRGB(86, 69, 17),
                    new ColorRGB(0, 55, 67),
                    new ColorRGB(102, 66, 60),
                    new ColorRGB(76, 60, 61),
                },
                Genres.Classic => new List<ColorRGB>()
                {
                    new ColorRGB(99, 69, 18),
                    new ColorRGB(111, 70, 37),
                    new ColorRGB(112, 34, 36),
                    new ColorRGB(11, 32, 89),
                },
                Genres.HipHop => new List<ColorRGB>()
                {
                    new ColorRGB(111, 98, 82),
                    new ColorRGB(72, 53, 107),
                    new ColorRGB(126, 98, 24),
                },
                Genres.EDM => new List<ColorRGB>()
                {
                    new ColorRGB(255, 0, 115),
                    new ColorRGB(0, 91, 255),
                    new ColorRGB(11, 231, 255),
                    new ColorRGB(0, 231, 255),
                    new ColorRGB(0, 255, 120),
                    new ColorRGB(0, 255, 8),
                    new ColorRGB(119, 255, 0),
                    new ColorRGB(255, 43, 0),
                },
                Genres.Rock => new List<ColorRGB>()
                {
                    new ColorRGB(255, 0, 0),
                    new ColorRGB(0, 0, 255),
                    new ColorRGB(255, 213, 0),
                    new ColorRGB(255, 255, 255),
                },
                Genres.Ballad => new List<ColorRGB>()
                {
                    new ColorRGB(142, 92, 118),
                    new ColorRGB(87, 129, 128),
                    new ColorRGB(90, 136, 90),
                    new ColorRGB(140, 128, 74),
                },
                _ => new List<ColorRGB>()
                {
                    new ColorRGB(255, 0, 0),
                    new ColorRGB(255, 128, 0),
                    new ColorRGB(255, 255, 0),
                    new ColorRGB(128, 255, 0),
                    new ColorRGB(0, 255, 0),
                    new ColorRGB(0, 255, 128),
                    new ColorRGB(0, 255, 255),
                    new ColorRGB(0, 128, 255),
                    new ColorRGB(0, 0, 255),
                    new ColorRGB(128, 0, 255),
                },
            };

            return colors;
        }
    }
}
