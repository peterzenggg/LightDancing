using Force.DeepCloner;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.MusicAnalysis;
using LightDancing.Smart;
using LightDancing.Syncs;
using NAudio.Dsp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LightDancing.OpenTKs.Animations
{
    internal abstract class AnimationBase
    {
        protected readonly List<Vector3> DISPLAY_COLORS = new List<Vector3>()
        {
            new Vector3(0.995f,0.032f,0.147f),
            new Vector3(0.975f,0.361f,0.008f),
            new Vector3(0.990f,0.953f,0.007f),
            new Vector3(0.397f,1.000f,0.004f),
            new Vector3(0.012f,0.817f,1.000f),
            new Vector3(0.000f,0.006f,1.000f),
            new Vector3(0.939f,0.007f,1.000f),
        };

        protected Vector3 _displayColor;

        public abstract void SendParameterToGL(Shader shader, double stream, AnimationGroup animationGroup);

        public abstract void SetColor(List<ColorRGB> colors);

        public Vector3 ConvertToVector3Color(ColorRGB color)
        {
            return new Vector3((float)color.R / 255, (float)color.G / 255, (float)color.B / 255);
        }
    }

    internal class Wallpaper : AnimationBase
    {
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private bool _minus = false;
        private double _loop = 0;
        private readonly List<String> _wallpaperModes;
        private readonly bool _shuffle;
        private readonly DurationTimer durationTimer = new DurationTimer(3000);
        private int index = 0;


        public Wallpaper(List<String> wallpaperModes, bool shuffle)
        {
            _displayColor = DISPLAY_COLORS[random.Next(DISPLAY_COLORS.Count)];
            _wallpaperModes = wallpaperModes;
            _shuffle = shuffle;
        }

        public void ChangeWallpaper()
        {
            if (durationTimer.IsAvailable())
            {
                OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;

                if (_wallpaperModes != null)
                {
                    _displayColor = DISPLAY_COLORS[random.Next(DISPLAY_COLORS.Count)];
                    if (_shuffle)
                    {
                        Random random = new Random(Guid.NewGuid().GetHashCode());
                        index = random.Next(0, _wallpaperModes.Count);
                        AnimationGroup group = GetAnimationGroup(_wallpaperModes[index]);
                        helper.ChangeShader(group);
                    }
                    else
                    {
                        var mode = _wallpaperModes[index];
                        AnimationGroup group = GetAnimationGroup(mode);
                        helper.ChangeShader(group);
                        index = (index + 1) % _wallpaperModes.Count;
                    }
                }
            }
        }

        private static AnimationGroup GetAnimationGroup(string type)
        {
            return type switch
            {
                "HalfCircle" => new AnimationGroup(BackgroundShaders.HalfCircle, null, null),
                "Fire" => new AnimationGroup(BackgroundShaders.Fire, null, null),
                "FadeCircle" => new AnimationGroup(BackgroundShaders.FadeCircle, null, null),
                "Diamond" => new AnimationGroup(BackgroundShaders.Diamond, null, null),
                "HalfRainbow" => new AnimationGroup(BackgroundShaders.HalfRainbow, null, null),
                "Flowers" => new AnimationGroup(BackgroundShaders.Flowers, null, null),
                "Triangle" => new AnimationGroup(BackgroundShaders.Triangle, null, null),
                "ClappingBall" => new AnimationGroup(BackgroundShaders.ClappingBall, null, null),
                "Rectangle" => new AnimationGroup(BackgroundShaders.Rectangle, null, null),
                "RoundCircle" => new AnimationGroup(BackgroundShaders.RoundCircle, null, null),
                "Arrow" => new AnimationGroup(BackgroundShaders.Arrow, null, null),
                "Grid" => new AnimationGroup(BackgroundShaders.Grid, null, null),
                "StripRotate" => new AnimationGroup(BackgroundShaders.StripRotate, null, null),
                "GridRotate" => new AnimationGroup(BackgroundShaders.GridRotate, null, null),
                "Squares" => new AnimationGroup(BackgroundShaders.Squares, null, null),
                //Cartoon
                "CartoonBG1" => new AnimationGroup(BackgroundShaders.CartoonBG1, null, null),
                "CartoonBG2" => new AnimationGroup(BackgroundShaders.CartoonBG2, null, null),
                "CartoonBG3" => new AnimationGroup(BackgroundShaders.CartoonBG3, null, null),
                "CartoonBG4" => new AnimationGroup(BackgroundShaders.CartoonBG4, null, null),
                "CartoonBG5" => new AnimationGroup(BackgroundShaders.CartoonBG5, null, null),
                "CartoonBG6" => new AnimationGroup(BackgroundShaders.CartoonBG6, null, null),
                "CartoonBG7" => new AnimationGroup(BackgroundShaders.CartoonBG7, null, null),
                //PopArt
                "MovingDots" => new AnimationGroup(BackgroundShaders.MovingDots, null, null),
                "PAHorizontalStrip" => new AnimationGroup(BackgroundShaders.PAHorizontalStrip, null, null),
                "VerticalStrip" => new AnimationGroup(BackgroundShaders.VerticalStrip, null, null),
                "MovingSlope" => new AnimationGroup(BackgroundShaders.MovingSlope, null, null),
                "MovingTriangleWave" => new AnimationGroup(BackgroundShaders.MovingTriangleWave, null, null),
                "MovingDotLarge" => new AnimationGroup(BackgroundShaders.MovingDotLarge, null, null),
                //Trip
                "Monster" => new AnimationGroup(BackgroundShaders.Monster, null, null),
                "AlienShip" => new AnimationGroup(BackgroundShaders.AlienShip, null, null),
                //Kale
                "Kale10" => new AnimationGroup(BackgroundShaders.Kale10, null, null),
                //Panorama
                "Heart" => new AnimationGroup(null, PanoramaShaders.Heart, null),
                "HorizontalStrip" => new AnimationGroup(null, PanoramaShaders.HorizontalStrip, null),
                "SpotLights" => new AnimationGroup(null, PanoramaShaders.SpotLights, null),
                "HeadLights" => new AnimationGroup(null, PanoramaShaders.HeadLights, null),
                "DiamondLights" => new AnimationGroup(null, PanoramaShaders.DiamondLights, null),
                "CircleSquare" => new AnimationGroup(null, PanoramaShaders.CircleSquare, null),
                "Wave" => new AnimationGroup(null, PanoramaShaders.Wave, null),
                "Ripple" => new AnimationGroup(null, PanoramaShaders.Ripple, null),
                "RotateCircle" => new AnimationGroup(null, PanoramaShaders.RotateCircle, null),
                "RotateDiamond" => new AnimationGroup(null, PanoramaShaders.RotateDiamond, null),
                "LineRotate" => new AnimationGroup(null, PanoramaShaders.LineRotate, null),
                "HorizontalLines" => new AnimationGroup(null, PanoramaShaders.HorizontalLines, null),
                "CrossLines" => new AnimationGroup(null, PanoramaShaders.CrossLines, null),
                "TriangleLine" => new AnimationGroup(null, PanoramaShaders.TriangleLine, null),
                "Swirl" => new AnimationGroup(null, PanoramaShaders.Swirl, null),
                //Cartoon
                "CartoonPanorama1" => new AnimationGroup(null, PanoramaShaders.CartoonPanorama1, null),
                "CartoonPanorama2" => new AnimationGroup(null, PanoramaShaders.CartoonPanorama2, null),
                "CartoonPanorama3" => new AnimationGroup(null, PanoramaShaders.CartoonPanorama3, null),
                "CartoonPanorama4" => new AnimationGroup(null, PanoramaShaders.CartoonPanorama4, null),
                "CartoonPanorama5" => new AnimationGroup(null, PanoramaShaders.CartoonPanorama5, null),
                //PopArt
                "FourSquare" => new AnimationGroup(null, PanoramaShaders.FourSquare, null),
                "DotSquare" => new AnimationGroup(null, PanoramaShaders.DotSquare, null),
                "CornerDot" => new AnimationGroup(null, PanoramaShaders.CornerDot, null),
                "CornerTriangleWave" => new AnimationGroup(null, PanoramaShaders.CornerTriangleWave, null),
                //Protagonist
                "BouncingBall" => new AnimationGroup(null, null, ProtagonistShaders.BouncingBall),
                "RadiusLaser" => new AnimationGroup(null, null, ProtagonistShaders.RadiusLaser),
                "Laser" => new AnimationGroup(null, null, ProtagonistShaders.Laser),
                "RandomString" => new AnimationGroup(null, null, ProtagonistShaders.RandomString),
                "Rain" => new AnimationGroup(null, null, ProtagonistShaders.Rain),
                "StillBalls" => new AnimationGroup(null, null, ProtagonistShaders.StillBalls),
                "RandomCross" => new AnimationGroup(null, null, ProtagonistShaders.RandomCross),
                "RandomHeart" => new AnimationGroup(null, null, ProtagonistShaders.RandomHeart),
                "RandomTarget" => new AnimationGroup(null, null, ProtagonistShaders.RandomTarget),
                "RandomTree" => new AnimationGroup(null, null, ProtagonistShaders.RandomTree),
                "SquareWave" => new AnimationGroup(null, null, ProtagonistShaders.SquareWave),
                //Cartoon
                "CartoonProtagonist1" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist1),
                "CartoonProtagonist2" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist2),
                "CartoonProtagonist3" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist3),
                "CartoonProtagonist4" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist4),
                "CartoonProtagonist5" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist5),
                "CartoonProtagonist6" => new AnimationGroup(null, null, ProtagonistShaders.CartoonProtagonist6),
                //PopArt
                "TSquare" => new AnimationGroup(null, null, ProtagonistShaders.TSquare),
                "OSquare" => new AnimationGroup(null, null, ProtagonistShaders.OSquare),
                "RandomCurve" => new AnimationGroup(null, null, ProtagonistShaders.RandomCurve),
                "TwoSideCircle" => new AnimationGroup(null, null, ProtagonistShaders.TwoSideCircle),
                "RandomCircle" => new AnimationGroup(null, null, ProtagonistShaders.RandomCircle),
                "RandomRect" => new AnimationGroup(null, null, ProtagonistShaders.RandomRect),
                _ => new AnimationGroup(BackgroundShaders.MovingDots, PanoramaShaders.CrossLines, null),
            };
        }

        public override void SendParameterToGL(Shader shader, double stream, AnimationGroup animationGroup)
        {
            if (_loop > 1 || _loop < 0)
            {
                _minus = !_minus;
            }
            _loop = _minus ? _loop - 0.01 : _loop + 0.01;

            if (animationGroup.Background != null)
            {
                int lowstreamLocation = GL.GetUniformLocation(shader.Handle, "low_u_time");
                GL.Uniform1(lowstreamLocation, (float)(stream / 100));

                int lowIntensityLocation = GL.GetUniformLocation(shader.Handle, "low_intensity");
                GL.Uniform1(lowIntensityLocation, (float)(_loop));

                int lowDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "low_displayColor");
                GL.Uniform3(lowDisplayColorLocation, _displayColor);
            }

            if (animationGroup.Panorama != null)
            {
                int midstreamLocation = GL.GetUniformLocation(shader.Handle, "mid_u_time");
                GL.Uniform1(midstreamLocation, (float)(stream / 100));

                int intensityLocation = GL.GetUniformLocation(shader.Handle, "mid_intensity");
                GL.Uniform1(intensityLocation, (float)(_loop));

                int displayColorLocation = GL.GetUniformLocation(shader.Handle, "mid_displayColor");
                GL.Uniform3(displayColorLocation, _displayColor);
            }

            if (animationGroup.Protagonist != null)
            {
                int highstreamLocation = GL.GetUniformLocation(shader.Handle, "high_u_time");
                GL.Uniform1(highstreamLocation, (float)(stream / 100));

                int highIntensityLocation = GL.GetUniformLocation(shader.Handle, "high_intensity");
                GL.Uniform1(highIntensityLocation, (float)(_loop));

                int highDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "high_displayColor");
                GL.Uniform3(highDisplayColorLocation, _displayColor);
            }
        }

        public override void SetColor(List<ColorRGB> colors)
        {
            if (colors != null && colors.Count > 0)
            {
                var color = colors[random.Next(colors.Count)];
                _displayColor = ConvertToVector3Color(color);
            }
        }
    }

    internal class Animate : AnimationBase
    {
        private List<Vector3> _display_colors = new List<Vector3>();
        private string _animateMode;
        private bool _animationChanged = false;

        public Animate(string animateMode)
        {
            if (_display_colors.Count == 0)
                _display_colors = DISPLAY_COLORS.DeepClone();
            int count = _display_colors.Count;
            for (int i = 0; i < 8 - count; i++)
            {
                _display_colors.Add(_display_colors[i]);
            }
            _animateMode = animateMode;
            /*Make sure the animation change when init animation base*/
            UpdateWallpaper();
        }

        /// <summary>
        /// Allow fron end to change the string of animation
        /// </summary>
        /// <param name="newAnimate"></param>
        public void ChangeWallpaper(string newAnimate)
        {
            _animateMode = newAnimate;
            _animationChanged = false;
        }

        /// <summary>
        /// KaleSample will call this when update is availiable, if the animation string is changed by the front end, this function will call helper to change the animation
        /// </summary>
        public void UpdateWallpaper()
        {
            if (!_animationChanged)
            {
                AnimationGroup shader = GetAnimationGroup(_animateMode);
                OpenTKSyncHelper.Instance.ChangeShader(shader);
                Console.WriteLine("Animate Change Shader to {0}", _animateMode);
                _animationChanged = true;
            }
        }

        private static AnimationGroup GetAnimationGroup(string type)
        {
            return type switch
            {
                "ColorShift" => new AnimationGroup(WallpaperAnimateShaders.ColorShift),
                "ColorSwirl" => new AnimationGroup(WallpaperAnimateShaders.ColorSwirl),
                "ColorBounceBall" => new AnimationGroup(WallpaperAnimateShaders.ColorBounceBall),
                "HalfColorCircle" => new AnimationGroup(WallpaperAnimateShaders.HalfColorCircle),
                "MixColor" => new AnimationGroup(WallpaperAnimateShaders.MixColor),
                "ColorStrip" => new AnimationGroup(WallpaperAnimateShaders.ColorStrip),
                "FluidGrid" => new AnimationGroup(WallpaperAnimateShaders.FluidGrid),
                "MeltingSmileFace" => new AnimationGroup(WallpaperAnimateShaders.MeltingSmileFace),
                "ColorRings" => new AnimationGroup(WallpaperAnimateShaders.ColorRings),
                "CheckboardFluid" => new AnimationGroup(WallpaperAnimateShaders.CheckboardFluid),
                "FixColorRotate" => new AnimationGroup(WallpaperAnimateShaders.FixColorRotate),
                _ => new AnimationGroup(BackgroundShaders.MovingDots, PanoramaShaders.CrossLines, null),
            };
        }

        /// <summary>
        /// Wallpaper Animation only use parameters below
        /// 1. u_time
        /// 2. 8 colors (color_1, color_2...)
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="stream"></param>
        /// <param name="animationGroup"></param>
        public override void SendParameterToGL(Shader shader, double stream, AnimationGroup animationGroup)
        {
            if (animationGroup.WallpaperAnimate != null)
            {
                int color1Location = GL.GetUniformLocation(shader.Handle, "color_1");
                GL.Uniform3(color1Location, _display_colors[0]);

                int color2Location = GL.GetUniformLocation(shader.Handle, "color_2");
                GL.Uniform3(color2Location, _display_colors[1]);

                int color3Location = GL.GetUniformLocation(shader.Handle, "color_3");
                GL.Uniform3(color3Location, _display_colors[2]);

                int color4Location = GL.GetUniformLocation(shader.Handle, "color_4");
                GL.Uniform3(color4Location, _display_colors[3]);

                int color5Location = GL.GetUniformLocation(shader.Handle, "color_5");
                GL.Uniform3(color5Location, _display_colors[4]);

                int color6Location = GL.GetUniformLocation(shader.Handle, "color_6");
                GL.Uniform3(color6Location, _display_colors[5]);

                int color7Location = GL.GetUniformLocation(shader.Handle, "color_7");
                GL.Uniform3(color7Location, _display_colors[6]);

                int color8Location = GL.GetUniformLocation(shader.Handle, "color_8");
                GL.Uniform3(color8Location, _display_colors[7]);
            }
        }

        public override void SetColor(List<ColorRGB> colors)
        {
            if (colors != null && colors.Count > 0)
            {
                List<Vector3> display_colors = new List<Vector3>();
                foreach (var color in colors)
                {
                    var vecColor = ConvertToVector3Color(color);
                    display_colors.Add(vecColor);
                }
                int count = display_colors.Count;
                for (int i = 0; i < 8 - count; i++)
                {
                    display_colors.Add(display_colors[i]);
                }

                _display_colors = display_colors.DeepClone();
            }
        }
    }

    internal class SmartMusic : AnimationBase, IDisposable
    {
        private readonly FftCatcher _fftCatcher = new FftCatcher();
        private readonly AnimationGroupAuto autoSwitchGroup;

        private DurationTimer durationTimer = new DurationTimer(20);
        private MusicData lowFrequency;
        private MusicData midFrequency;
        private MusicData highFrequency;

        public SmartMusic(bool isAuto, Genres? customGenres)
        {
            _fftCatcher.Start();
            lowFrequency = new MusicData(Tuple.Create(60, 250), DISPLAY_COLORS);
            midFrequency = new MusicData(Tuple.Create(630, 3000), DISPLAY_COLORS);
            highFrequency = new MusicData(Tuple.Create(3000, 16000), DISPLAY_COLORS);

            if (isAuto)
            {
                autoSwitchGroup = customGenres == null ? new AnimationGroupAuto() : new AnimationGroupAuto(customGenres.Value);
            }
        }

        /// <summary>
        /// Allows KaleSample to change MLs path
        /// </summary>
        /// <param name="path"></param>
        public void SetMLsFilePath(string path)
        {
            if (autoSwitchGroup != null)
                autoSwitchGroup.SetMLsFilePath(path);
        }

        public void GetMusicBeat(AnimationGroup animationGroup)
        {
            if (durationTimer.IsAvailable())
            {
                MusicFeatures musicFeatures = _fftCatcher.GetMusicFeatures();
                if (musicFeatures != null)
                {
                    Dictionary<FFTSampleType, Complex[]> FftResults = musicFeatures.FftResults;
                    var fftResult = FftResults[FFTSampleType.Center];

                    if (animationGroup.Background != null)
                    {
                        lowFrequency.ProcessMusic(fftResult);
                    }

                    if (animationGroup.Panorama != null)
                    {
                        midFrequency.ProcessFadeMusic(fftResult);
                    }

                    if (animationGroup.Protagonist != null)
                    {
                        highFrequency.ProcessFadeMusic(fftResult);
                    }
                }
            }
        }

        public override void SendParameterToGL(Shader shader, double stream, AnimationGroup animationGroup)
        {
            if (autoSwitchGroup != null)
            {
                var features = _fftCatcher.GetMusicFeatures();
                if (features != null)
                {
                    try
                    {
                        autoSwitchGroup.ChangedShader(features);
                    }
                    catch (Exception exception)
                    {
                        _fftCatcher.Stop();
                        _fftCatcher.Dispose();
                        throw exception;
                    }
                }
            }

            GetMusicBeat(animationGroup);

            if (animationGroup.Background != null)
            {
                int lowstreamLocation = GL.GetUniformLocation(shader.Handle, "low_u_time");
                GL.Uniform1(lowstreamLocation, (float)(lowFrequency.BeatCount / 100));

                int lowIntensityLocation = GL.GetUniformLocation(shader.Handle, "low_intensity");
                GL.Uniform1(lowIntensityLocation, (float)(lowFrequency.Intensity / lowFrequency.MaxIntensity));

                int lowDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "low_displayColor");
                GL.Uniform3(lowDisplayColorLocation, lowFrequency.DisplayColor);
            }

            if (animationGroup.Panorama != null)
            {
                int midstreamLocation = GL.GetUniformLocation(shader.Handle, "mid_u_time");
                GL.Uniform1(midstreamLocation, (float)(midFrequency.BeatCount / 100));

                int intensityLocation = GL.GetUniformLocation(shader.Handle, "mid_intensity");
                GL.Uniform1(intensityLocation, (float)(midFrequency.Intensity / midFrequency.MaxIntensity));

                int displayColorLocation = GL.GetUniformLocation(shader.Handle, "mid_displayColor");
                GL.Uniform3(displayColorLocation, midFrequency.DisplayColor);
            }

            if (animationGroup.Protagonist != null)
            {
                int highstreamLocation = GL.GetUniformLocation(shader.Handle, "high_u_time");
                GL.Uniform1(highstreamLocation, (float)(highFrequency.BeatCount / 100));

                int highIntensityLocation = GL.GetUniformLocation(shader.Handle, "high_intensity");
                GL.Uniform1(highIntensityLocation, (float)(highFrequency.Intensity / highFrequency.MaxIntensity));

                int highDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "high_displayColor");
                GL.Uniform3(highDisplayColorLocation, highFrequency.DisplayColor);
            }
        }

        internal void SetSpeed(int ms)
        {
            durationTimer = new DurationTimer(ms);
        }

        internal void ResetFrequencyBand()
        {
            lowFrequency = new MusicData(Tuple.Create(60, 250), DISPLAY_COLORS);
            midFrequency = new MusicData(Tuple.Create(630, 3000), DISPLAY_COLORS);
            highFrequency = new MusicData(Tuple.Create(3000, 16000), DISPLAY_COLORS);
        }

        public override void SetColor(List<ColorRGB> colors)
        {
            if (colors != null && colors.Count > 0)
            {
                List<Vector3> vectorColors = new List<Vector3>();

                foreach (var color in colors)
                {
                    var vecColor = ConvertToVector3Color(color);
                    vectorColors.Add(vecColor);
                }

                if (lowFrequency != null)
                {
                    lowFrequency.ChangeColorSet(vectorColors);
                }

                if (midFrequency != null)
                {
                    midFrequency.ChangeColorSet(vectorColors);
                }

                if (highFrequency != null)
                {
                    highFrequency.ChangeColorSet(vectorColors);
                }
            }
        }


        /// <summary>
        /// Dispose FFT catcher
        /// </summary>
        public void Dispose()
        {
            _fftCatcher.Stop();
            _fftCatcher.Dispose();
        }
    }

    public class MusicData
    {
        private readonly FrequencyBandSetup frequencyBandSetup;
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private List<Vector3> displayColors = new List<Vector3>();
        public double Intensity { set; get; }

        public double MaxIntensity { set; get; }

        public double BeatCount { set; get; }

        public Vector3 DisplayColor { set; get; }

        public bool _isDetected = false;

        public double beats = 0;

        public MusicData(Tuple<int, int> frequencyBand, List<Vector3> displayColors)
        {
            frequencyBandSetup = new FrequencyBandSetup(frequencyBand);
            this.displayColors = displayColors;
            DisplayColor = displayColors[random.Next(displayColors.Count)];
            MaxIntensity = 1;
        }

        public void ProcessMusic(Complex[] fftResult)
        {
            if (frequencyBandSetup.IsBeat(fftResult))
            {
                BeatCount = BeatCount + 100 > Double.MaxValue ? BeatCount - Double.MaxValue : BeatCount + 100;
                DisplayColor = displayColors[random.Next(displayColors.Count)];
                Intensity = frequencyBandSetup.GetMaxIntensity(fftResult);
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
            }
            else
            {
                Intensity = Intensity - 1 < 0 ? 0 : Intensity - 1;
            }
        }

        public void ProcessFadeMusic(Complex[] fftResult)
        {
            if (frequencyBandSetup.IsBeat(fftResult))
            {
                BeatCount = BeatCount + 100 > 4000 ? 4000 : BeatCount + 100;
                DisplayColor = displayColors[random.Next(displayColors.Count)];
                Intensity = frequencyBandSetup.GetMaxIntensity(fftResult);
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
            }
            else
            {
                BeatCount = BeatCount - 3 < 0 ? 0 : BeatCount - 3;
                Intensity = Intensity - 1 < 0 ? 0 : Intensity - 1;
                BeatCount = Intensity == 0 ? BeatCount / 2 : BeatCount;
            }
        }

        public void ChangeColorSet(List<Vector3> colors)
        {
            displayColors = colors;
        }

        /*MEMO: For New music analysis*/
        public void ProcessBass(Complex[] fftResult)
        {
            var temp = frequencyBandSetup.GetLogrithmicAverageIntensity_Low(fftResult);
            if (temp >= 10)
            {
                Intensity = temp;
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
                beats = beats + (20 * Math.Pow((double)temp / MaxIntensity, 2.0)) > double.MaxValue ? 0 : beats + (20 * Math.Pow((double)temp / MaxIntensity, 2.0));
            }
            else
            {
                Intensity = 0;
            }
        }

        public void ProcessHighs(Complex[] fftResult)
        {
            var temp = frequencyBandSetup.GetLogrithmicMaximumIntensity_Low(fftResult); //use to use high
            if (temp >= 10)
            {
                Intensity = temp;
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
                beats = beats + (1 * Math.Pow((double)temp / MaxIntensity, 2.0)) > double.MaxValue ? 0 : beats + (1 * Math.Pow((double)temp / MaxIntensity, 2.0));
            }
            else
            {
                Intensity = 0;
            }
        }

        public void ProcessFullSpectrum(Complex[] fftResult)
        {
            var temp = frequencyBandSetup.GetLogrithmicMaximumIntensity_Low(fftResult);
            if (temp > 3)
            {
                Intensity = temp;
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
                beats = beats + (10 * Math.Pow((double)temp / MaxIntensity, 2.0)) > double.MaxValue ? 0 : beats + (10 * Math.Pow((double)temp / MaxIntensity, 2.0));
            }
            else
            {
                Intensity = 0;
            }
        }

        public void ProcessSnare(Complex[] fftResult)
        {
            var temp = frequencyBandSetup.GetLogrithmicMaximumIntensity_Low(fftResult); //use to use high
            if (temp > 20)
            {
                Intensity = temp;
                MaxIntensity = Intensity > MaxIntensity ? Intensity : MaxIntensity;
                beats = beats + (1 * Math.Pow((double)temp / MaxIntensity, 2.0)) > double.MaxValue ? 0 : beats + (1 * Math.Pow((double)temp / MaxIntensity, 2.0));
            }
            else
            {
                Intensity = 0;
            }
        }
    }

    internal class BeatAnalysis : AnimationBase, IDisposable
    {
        private readonly FftCatcher _fftCatcher = new FftCatcher();
        private readonly AnimationGroupAuto autoSwitchGroup;

        private MusicData lowFrequency;
        private MusicData midFrequency;
        private MusicData highFrequency;
        private MusicData fullSpectrum;
        private DateTime dateTime = DateTime.Now;
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());

        private int _beatStream = 0;
        public double beats = 0;
        private List<Vector3> _display_colors = new List<Vector3>();

        public BeatAnalysis(bool isAuto, Genres? customGenres)
        {
            if (_display_colors.Count == 0)
                _display_colors = DISPLAY_COLORS.DeepClone();
            int count = _display_colors.Count;
            for (int i = 0; i < 8 - count; i++)
            {
                _display_colors.Add(_display_colors[i]);
            }

            _fftCatcher.Start();
            lowFrequency = new MusicData(Tuple.Create(0, 120), DISPLAY_COLORS);
            midFrequency = new MusicData(Tuple.Create(120, 900), DISPLAY_COLORS);
            highFrequency = new MusicData(Tuple.Create(900, 25000), DISPLAY_COLORS);
            fullSpectrum = new MusicData(Tuple.Create(0, 25000), DISPLAY_COLORS);
            dateTime = DateTime.Now;

            if (isAuto)
            {
                autoSwitchGroup = customGenres == null ? new AnimationGroupAuto() : new AnimationGroupAuto(customGenres.Value);
            }
        }

        public void SetMLsFilePath(string path)
        {
            if (autoSwitchGroup != null)
                autoSwitchGroup.SetMLsFilePath(path);
        }

        public void GetMusicBeat(AnimationGroup animationGroup)
        {
            MusicFeatures musicFeatures = _fftCatcher.GetMusicFeatures();
            if (musicFeatures != null)
            {
                Dictionary<FFTSampleType, Complex[]> FftResults = musicFeatures.FftResults;
                var fftResult = FftResults[FFTSampleType.Center];

                lowFrequency.ProcessBass(fftResult);
                fullSpectrum.ProcessFullSpectrum(fftResult);
                midFrequency.ProcessSnare(fftResult);
                highFrequency.ProcessHighs(fftResult);

                beats = lowFrequency.beats - midFrequency.beats + fullSpectrum.beats - highFrequency.beats;
                beats = beats > Double.MaxValue ? (beats - Double.MaxValue) : beats;
            }
        }

        public override void SendParameterToGL(Shader shader, double stream, AnimationGroup animationGroup)
        {
            if (autoSwitchGroup != null)
            {
                var features = _fftCatcher.GetMusicFeatures();
                if (features != null)
                {
                    try
                    {
                        autoSwitchGroup.ChangedShader(features);
                    }
                    catch (Exception exception)
                    {
                        _fftCatcher.Stop();
                        _fftCatcher.Dispose();
                        throw exception;
                    }
                }
            }

            GetMusicBeat(animationGroup);

            //if (animationGroup.Background != null)
            {
                int lowstreamLocation = GL.GetUniformLocation(shader.Handle, "low_u_time");
                GL.Uniform1(lowstreamLocation, (float)(lowFrequency.BeatCount / 100));

                int lowIntensityLocation = GL.GetUniformLocation(shader.Handle, "low_intensity");
                GL.Uniform1(lowIntensityLocation, (float)(lowFrequency.Intensity / lowFrequency.MaxIntensity));

                int lowDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "low_displayColor");
                GL.Uniform3(lowDisplayColorLocation, lowFrequency.DisplayColor);
            }

            //if (animationGroup.Panorama != null)
            {
                int midstreamLocation = GL.GetUniformLocation(shader.Handle, "mid_u_time");
                GL.Uniform1(midstreamLocation, (float)(midFrequency.BeatCount / 100));

                int intensityLocation = GL.GetUniformLocation(shader.Handle, "mid_intensity");
                GL.Uniform1(intensityLocation, (float)(midFrequency.Intensity / midFrequency.MaxIntensity));

                int displayColorLocation = GL.GetUniformLocation(shader.Handle, "mid_displayColor");
                GL.Uniform3(displayColorLocation, midFrequency.DisplayColor);
            }

            //if (animationGroup.Protagonist != null)
            {
                int highstreamLocation = GL.GetUniformLocation(shader.Handle, "high_u_time");
                GL.Uniform1(highstreamLocation, (float)(highFrequency.BeatCount / 100));

                int highIntensityLocation = GL.GetUniformLocation(shader.Handle, "high_intensity");
                GL.Uniform1(highIntensityLocation, (float)(highFrequency.Intensity / highFrequency.MaxIntensity));

                int highDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "high_displayColor");
                GL.Uniform3(highDisplayColorLocation, highFrequency.DisplayColor);
            }

            //if (animationGroup.Kick != null)
            {
                int highstreamLocation = GL.GetUniformLocation(shader.Handle, "kick_u_time");
                GL.Uniform1(highstreamLocation, (float)(fullSpectrum.BeatCount / 100));

                int highIntensityLocation = GL.GetUniformLocation(shader.Handle, "kick_intensity");
                GL.Uniform1(highIntensityLocation, (float)(fullSpectrum.Intensity / fullSpectrum.MaxIntensity));

                int highDisplayColorLocation = GL.GetUniformLocation(shader.Handle, "kick_displayColor");
                GL.Uniform3(highDisplayColorLocation, fullSpectrum.DisplayColor);
            }

            //Colors
            int color1Location = GL.GetUniformLocation(shader.Handle, "color_1");
            GL.Uniform3(color1Location, _display_colors[0]);

            int color2Location = GL.GetUniformLocation(shader.Handle, "color_2");
            GL.Uniform3(color2Location, _display_colors[1]);

            int color3Location = GL.GetUniformLocation(shader.Handle, "color_3");
            GL.Uniform3(color3Location, _display_colors[2]);

            int color4Location = GL.GetUniformLocation(shader.Handle, "color_4");
            GL.Uniform3(color4Location, _display_colors[3]);

            int color5Location = GL.GetUniformLocation(shader.Handle, "color_5");
            GL.Uniform3(color5Location, _display_colors[4]);

            int color6Location = GL.GetUniformLocation(shader.Handle, "color_6");
            GL.Uniform3(color6Location, _display_colors[5]);

            int color7Location = GL.GetUniformLocation(shader.Handle, "color_7");
            GL.Uniform3(color7Location, _display_colors[6]);

            int color8Location = GL.GetUniformLocation(shader.Handle, "color_8");
            GL.Uniform3(color8Location, _display_colors[7]);


            int streamLocation_1 = GL.GetUniformLocation(shader.Handle, "beat_u_time");
            GL.Uniform1(streamLocation_1, (float)((float)_beatStream / 10000));

            int beatsstream = GL.GetUniformLocation(shader.Handle, "u_time_beat");
            GL.Uniform1(beatsstream, (float)(beats / 100.0));

            var volume = CommonMethods.GetVolume();
            volume = Math.Pow(volume, 0.15) * 1.003 > 1.0 ? 1.0 : Math.Pow(volume, 0.15) * 1.003;

            int volumeLocation = GL.GetUniformLocation(shader.Handle, "volume");
            GL.Uniform1(volumeLocation, (float)volume);
        }

        internal void SetSpeed(int ms)
        {
        }

        internal void ResetFrequencyBand()
        {
            lowFrequency = new MusicData(Tuple.Create(0, 120), DISPLAY_COLORS);
            midFrequency = new MusicData(Tuple.Create(120, 900), DISPLAY_COLORS);
            highFrequency = new MusicData(Tuple.Create(900, 25000), DISPLAY_COLORS);
            fullSpectrum = new MusicData(Tuple.Create(0, 25000), DISPLAY_COLORS);
        }

        public override void SetColor(List<ColorRGB> colors)
        {
            if (colors != null && colors.Count > 0)
            {
                List<Vector3> display_colors = new List<Vector3>();
                foreach (var color in colors)
                {
                    var vecColor = ConvertToVector3Color(color);
                    display_colors.Add(vecColor);
                }
                int count = display_colors.Count;
                for (int i = 0; i < 8 - count; i++)
                {
                    display_colors.Add(display_colors[i]);
                }

                _display_colors = display_colors.DeepClone();
            }
        }

        /// <summary>
        /// Dispose FFT catcher
        /// </summary>
        public void Dispose()
        {
            _fftCatcher.Stop();
            _fftCatcher.Dispose();
        }
    }
}
