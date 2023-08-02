using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.MusicAnalysis;
using LightDancing.OpenTKs;
using LightDancing.OpenTKs.Animations;
using LightDancing.Smart.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace LightDancing.Syncs
{
    public class OpentkParameters
    {
        /// <summary>
        /// Is need to display the Opentk winodws
        /// </summary>
        public bool DisplayWindows { get; set; }

        /// <summary>
        /// Opentk mode
        /// </summary>
        public OpenTKMode Mode { get; set; }

        /// <summary>
        /// Is processing smart algorithms
        /// </summary>
        public bool IsAuto { get; set; }

        /// <summary>
        /// Custom genres
        /// </summary>
        public Genres? CustomGenres { get; set; }

        /// <summary>
        /// List of wallpapers for wallpaper mode
        /// </summary>
        public List<string> Wallpapers { get; set; }

        /// <summary>
        /// when wallpaper mode, if wallpaper list if random
        /// </summary>
        public bool Shuffle { get; set; }

        /// <summary>
        /// Animation for Animate mode
        /// </summary>
        public string Animate { get; set; }
    }


    public class OpenTKSyncHelper : ISyncHelper
    {
        private KaleSample _windows;
        private AnimationGroup _defaultAnimationGroup = new AnimationGroup(CommonMethods.RandomEnumFromSource(DictionaryConst.BACKGROUND_ALL), null, null);
        private static readonly Lazy<OpenTKSyncHelper> lazy = new Lazy<OpenTKSyncHelper>(() => new OpenTKSyncHelper());
        private Action m_updateHandler;
        private bool _isProcessing = false;
        private ImageFilterBase _filter = new NoneFilter();
        private string filePath = Environment.CurrentDirectory + "\\MLs\\Source\\GenreModel.zip";
        private List<ColorRGB> newColors = new List<ColorRGB>();
        private List<ColorRGB> musicSyncColors = new List<ColorRGB>()
        {
            new ColorRGB(253, 8, 37),
            new ColorRGB(248, 92, 2),
            new ColorRGB(252, 243, 2),
            new ColorRGB(101, 255, 1),
            new ColorRGB(3, 208, 255),
            new ColorRGB(0, 2, 255),
            new ColorRGB(239, 2, 255),
        };
        private ColorFilter thisFilter = ColorFilter.Normal;
        private float filterPercentage = 0f; //Default filter percentage = 0
        private float SpeedConfig = 1; // Default speed configuration = 1
        private int _currentSpeed = 3; // Default speed = 3 (x1)
        private float _currentNoise = 0; // Default noise = 0
        private float _hue = 0; // Default hue = 0
        private float _sat = 0.5f; // Default saturation = 0.5
        private bool _blueReduction = false;

        private List<ColorRGB> defaultColors = new List<ColorRGB>()
        {
            new ColorRGB(253, 8, 37),
            new ColorRGB(248, 92, 2),
            new ColorRGB(252, 243, 2),
            new ColorRGB(101, 255, 1),
            new ColorRGB(3, 208, 255),
            new ColorRGB(0, 2, 255),
            new ColorRGB(239, 2, 255),
        };

        public static OpenTKSyncHelper Instance => lazy.Value;

        public Action ScreenUpdated { get; set; }

        /*Thread to keep opentk running in the same thread at all times*/
        private Thread _openTkThread;
        private OpentkParameters _opentkParameters;
        private bool _windowRestart = false;

        /// <summary>
        /// Create the OpenTK windows and run in task.
        /// Warning: 
        /// Since the GLFW only can be create with same thread,
        /// please make sure when you re-call this method it should be in same thread.
        /// </summary>
        /// <param name="displayWindows">Dispaly windows or not</param>
        public void Start(OpentkParameters opentkParameters)
        {
            _opentkParameters = opentkParameters;

            if (_openTkThread == null)
            {
                _openTkThread = new Thread(new ThreadStart(StartWindows)) { IsBackground = true };
                _openTkThread.Start();
            }
            _windowRestart = true;
        }

        /// <summary>
        /// After set screen resolution, please call this method to restart the openTK sync
        /// </summary>
        public void Restart()
        {
            if (_opentkParameters != null)
            {
                _windowRestart = true;
            }
        }

        /// <summary>
        /// Get Opentk parameters
        /// </summary>
        /// <returns>Current OpentkParameters</returns>
        public OpentkParameters GetOpentkParameters()
        {
            return _opentkParameters;
        }

        private void StartWindows()
        {
            while (true)
            {
                if (_windowRestart)
                {
                    NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
                    {
                        Size = new Vector2i(ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight),
                        Title = "Animations"
                    };

                    GPUControl.InitializeDedicatedGraphics();

                    if (_opentkParameters.Mode == OpenTKMode.Animate)
                    {
                        _defaultAnimationGroup = _opentkParameters.Animate switch
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

                    _windows = new KaleSample(GameWindowSettings.Default, nativeWindowSettings, _defaultAnimationGroup, _opentkParameters, m_updateHandler)
                    {
                        IsVisible = _opentkParameters != null && _opentkParameters.DisplayWindows,
                    };

                    SetColorFilter(thisFilter, filterPercentage);

                    _windows.VSync = OpenTK.Windowing.Common.VSyncMode.Off;
                    _windows.SetMLsFilePath(filePath);
                    //if (newColors.Count > 0)
                    //{
                    //    SetColorFilter(thisFilter, filterPercentage);
                    //}

                    SetAnimateSpeed(_currentSpeed);
                    SetNoise(_currentNoise);

                    _windowRestart = false;
                    _windows.Run();
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Allows front end to change MLs path
        /// </summary>
        /// <param name="path"></param>
        public void SetMLsFilePath(string path)
        {
            filePath = path;
        }

        /// <summary>
        /// Stop update the OpenTK windows
        /// </summary>
        public void Stop()
        {
            if (_isProcessing)
                _isProcessing = false;

            _windows?.StopFFT();
            _windows?.Close();
            _windows?.Dispose();
        }

        /// <summary>
        /// Chnaged the OpenTK's shader 
        /// </summary>
        /// <param name="animationGroup">Animation Group</param>
        public void ChangeShader(AnimationGroup animationGroup)
        {
            if (animationGroup.MusicAnimate != null)
            {
                thisFilter = ColorFilter.Normal;
            }
            if (_windows == null)
            {
                _defaultAnimationGroup = animationGroup;
            }
            else
            {
                _defaultAnimationGroup = animationGroup;
                _windows.ShaderGroupMode = animationGroup;
            }
        }

        /// <summary>
        /// Get color matrix
        /// </summary>
        /// <returns>Color matrix</returns>
        public ColorRGB[,] GetColors()
        {
            var colorMatix = _windows?.GetColors();
            if (colorMatix != null)
            {
                colorMatix = _filter.Process(colorMatix);
            }
            return colorMatix;
        }

        /// <summary>
        /// Bitmap for preview screen
        /// OpentkHelper only use bitmap for opt
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmapForPreviewImage()
        {
            if (_windows != null)
            {
                return _windows.GetBitmap();
            }
            else
            {
                return new Bitmap(ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight);
            }
        }

        public Bitmap GetBitmapForAlphaVideo()
        {
            if (_windows != null)
            {
                return _windows.GetBitmapForAlphaVideo();
            }
            else
            {
                return new Bitmap(ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight);
            }
        }

        /// <summary>
        /// Bitmap for streaming
        /// OpentkHelper only use bitmap for opt
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmapForStreaming()
        {
            if (_windows != null)
            {
                return _windows.GetBitmapForStreaming();
            }
            else
                return null;
        }

        public void ChangeColorSet(List<ColorRGB> colors)
        {
            newColors = colors;
            SetColorFilter(thisFilter, filterPercentage);
        }

        public void ChangeMusicSyncColor(List<ColorRGB> colors)
        {
            musicSyncColors = colors;

            if (_windows != null)
            {
                _windows.SetColor(musicSyncColors);
            }
        }

        public void ResetFrequencyBands()
        {
            if (_windows != null)
            {
                _windows.ResetFrequencyBand();
            }
        }

        /// <summary>
        /// For MusicSync mode
        /// </summary>
        /// <param name="ms"></param>
        public void SetSpeed(int ms)
        {
            if (_windows != null)
            {
                _windows.SetSpeed(ms);
            }
        }

        public void RegisterPreviewUpdateCallback(Action func)
        {
            m_updateHandler = func;
        }

        public void SetBlur(ImageFilterBase filter)
        {
            _filter = filter;
        }

        public string GetBase64StringColor()
        {
            Bitmap bitmap = GetBitmapForPreviewImage();
            ScreenUpdated?.Invoke();
            return Methods.ColorRgbToBase64(bitmap);
        }

        public void ChangeAnimate(string animate)
        {
            if (_windows != null)
            {
                _windows.ChangeAnimate(animate);
            }
        }

        /// <summary>
        /// Change Hue of the input colors
        /// </summary>
        /// <param name="colors">input colors</param>
        /// <param name="hue">0~1, normal should be 0 means 0 degree, 1 means 360 (the angle of hue is rotated)</param>
        public void ChangeColorHue(List<ColorRGB> colors, float hue)
        {
            _hue = hue;
            List<ColorRGB> thisColorList = new List<ColorRGB>();

            if (colors.Count > 0)
            {
                foreach (var color in colors)
                {
                    ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                    thisColor.SetSat(_sat);
                    thisColor.SetHue(hue);
                    thisColorList.Add(thisColor);
                }
            }
            else
            {
                foreach (var color in defaultColors)
                {
                    ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                    thisColor.SetSat(_sat);
                    thisColor.SetHue(hue);
                    thisColorList.Add(thisColor);
                }
            }
            if (thisColorList.Count != 0)
                newColors = thisColorList;
            SetColorFilter(thisFilter, filterPercentage);
        }

        /// <summary>
        /// Change color set saturation
        /// </summary>
        /// <param name="colors">Input color set</param>
        /// <param name="sat">saturation should be 0 ~ 1, 0.5 is normal, 0 is less saturated, 1 is full saturation</param>
        public void ChangeColorSat(List<ColorRGB> colors, float sat)
        {
            _sat = sat;

            List<ColorRGB> thisColorList = new List<ColorRGB>();
            if (colors.Count > 0)
            {
                foreach (var color in colors)
                {
                    ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                    thisColor.SetSat(sat);
                    thisColor.SetHue(_hue);
                    thisColorList.Add(thisColor);
                }
            }
            else
            {
                foreach (var color in defaultColors)
                {
                    ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                    thisColor.SetSat(sat);
                    thisColor.SetHue(_hue);
                    thisColorList.Add(thisColor);
                }
            }
            newColors = thisColorList;
            SetColorFilter(thisFilter, filterPercentage);
        }

        /// <summary>
        /// https://youtu.be/pIJRRG9jhJI
        /// </summary>
        /// <param name="colorFilter">Choose a color filter</param>
        /// <param name="percentage">Pass through color percentage</param>
        public void SetColorFilter(ColorFilter colorFilter, float percentage)
        {
            try
            {
                thisFilter = colorFilter;
                filterPercentage = percentage;
                List<ColorRGB> thisColorList = new List<ColorRGB>();
                if (newColors.Count > 0)
                {
                    foreach (var color in newColors)
                    {
                        ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                        switch (colorFilter)
                        {
                            case ColorFilter.RedFilter:
                                thisColor.SetColorFilter(1f, 1f - percentage, 1f - percentage);
                                break;
                            case ColorFilter.BlueFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f - percentage, 1f);
                                break;
                            case ColorFilter.GreenFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f, 1f - percentage);
                                break;
                            case ColorFilter.YellowFilter:
                                thisColor.SetColorFilter(1f, 1f, 1f - percentage);
                                break;
                            case ColorFilter.MagentaFilter:
                                thisColor.SetColorFilter(1f, 1f - percentage, 1f);
                                break;
                            case ColorFilter.CyanFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f, 1f);
                                break;
                            default:
                                break;
                        };
                        if (_blueReduction)
                            thisColor.SetColorFilter(1f, 1f, 0.75f);
                        thisColorList.Add(thisColor);
                    }
                }
                else
                {
                    foreach (var color in defaultColors)
                    {
                        ColorRGB thisColor = new ColorRGB(color.R, color.G, color.B);
                        switch (colorFilter)
                        {
                            case ColorFilter.RedFilter:
                                thisColor.SetColorFilter(1f, 1f - percentage, 1f - percentage);
                                break;
                            case ColorFilter.BlueFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f - percentage, 1f);
                                break;
                            case ColorFilter.GreenFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f, 1f - percentage);
                                break;
                            case ColorFilter.YellowFilter:
                                thisColor.SetColorFilter(1f, 1f, 1f - percentage);
                                break;
                            case ColorFilter.MagentaFilter:
                                thisColor.SetColorFilter(1f, 1f - percentage, 1f);
                                break;
                            case ColorFilter.CyanFilter:
                                thisColor.SetColorFilter(1f - percentage, 1f, 1f);
                                break;
                            default:
                                break;
                        };
                        if (_blueReduction)
                            thisColor.SetColorFilter(1f, 1f, 0.75f);
                        thisColorList.Add(thisColor);
                    }
                }


                if (_windows != null)
                {
                    _windows.SetColor(thisColorList);
                }
            }
            catch { }
        }

        /// <summary>
        /// For wallpaper animte and speed config
        /// </summary>
        /// <param name="index">Parameter from 0 to 7, 0 = x0.25, 1 = x0.5... 6 = x1.75, 7 = x2</param>
        public void SetAnimateSpeed(int index)
        {
            _currentSpeed = index;

            float currentSpeed = 1;
            switch (index)
            {
                case 0:
                    currentSpeed = 0.0f;
                    break;
                case 1:
                    currentSpeed = 0.25f;
                    break;
                case 2:
                    currentSpeed = 0.5f;
                    break;
                case 3:
                    currentSpeed = 0.75f;
                    break;
                case 4:
                    currentSpeed = 1;
                    break;
                case 5:
                    currentSpeed = 1.25f;
                    break;
                case 6:
                    currentSpeed = 1.5f;
                    break;
                case 7:
                    currentSpeed = 1.75f;
                    break;
                case 8:
                    currentSpeed = 2;
                    break;
            }

            if (_windows != null)
            {
                _windows.SetSpeed(currentSpeed * SpeedConfig);
            }
        }

        public void SetNoise(float index)
        {
            _currentNoise = index;
            if (_windows != null)
            {
                _windows.SetNoise(_currentNoise * SpeedConfig);
            }
        }

        public void SetSpeedConfig(int speed)
        {
            SpeedConfig = speed;
            switch (speed)
            {
                case 0:
                    SpeedConfig = 0.25f;
                    break;
                case 1:
                    SpeedConfig = 0.5f;
                    break;
                case 2:
                    SpeedConfig = 0.75f;
                    break;
                case 3:
                    SpeedConfig = 1;
                    break;
                case 4:
                    SpeedConfig = 1.25f;
                    if (_currentNoise == 0)
                        _currentNoise = 0.5f;
                    break;
                case 5:
                    SpeedConfig = 1.5f;
                    if (_currentNoise == 0)
                        _currentNoise = 0.5f;
                    break;
                case 6:
                    SpeedConfig = 1.75f;
                    if (_currentNoise == 0)
                        _currentNoise = 0.5f;
                    break;
                case 7:
                    SpeedConfig = 2;
                    if (_currentNoise == 0)
                        _currentNoise = 0.5f;
                    break;
            }

            SetAnimateSpeed(_currentSpeed);
            SetNoise(_currentNoise);
        }

        /// <summary>
        /// Reduce blue light
        /// </summary>
        /// <param name="_reduction"></param>
        public void SetBlueFilter(bool _reduction)
        {
            _blueReduction = _reduction;
            SetColorFilter(thisFilter, filterPercentage);
        }
    }
}
