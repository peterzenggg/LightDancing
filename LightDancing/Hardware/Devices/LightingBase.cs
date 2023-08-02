using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.OpenTKs;
using LightDancing.Streamings;
using LightDancing.Syncs;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices
{
    public abstract class LightingBase
    {
        private readonly object locker = new object();
        private readonly int yAxisLedCount;
        private readonly int xAxisLedCount;

        protected LightingModel _model;
        protected HardwareModel _usbModel;

        protected List<byte> _displayColorBytes;
        protected string _httpClientColorData;

        private bool _isTurnOn = true;
        private IStreaming _streaming;

        public Action _LedTurnOff;

        protected Dictionary<Keyboard, ColorRGB> keyColor = new Dictionary<Keyboard, ColorRGB>();

        private List<List<Keyboard>> _keyboardLayout;

        public List<List<Keyboard>> KeyboardLayout
        {
            get
            {
                _keyboardLayout ??= new List<List<Keyboard>>();
                return _keyboardLayout;
            }
            protected set
            {
                _keyboardLayout = value;
            }
        }

        public LightingBase(int yAxis, int xAxis, HardwareModel hardwareModel)
        {
            _usbModel = hardwareModel;
            yAxisLedCount = yAxis;
            xAxisLedCount = xAxis;

            _model = InitModel();
        }

        /// <summary>
        /// Init device data
        /// </summary>
        protected abstract LightingModel InitModel();

        protected abstract void ProcessColor(ColorRGB[,] colorMatrix);

        protected abstract void TurnOffLed();

        protected abstract void SetKeyLayout();

        protected abstract void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors);

        /// <summary>
        /// Set animation coding 
        /// </summary>
        /// <param name="type"></param>
        public void SetStreaming(AnimationCoding type)
        {
            _streaming = type switch
            {
                AnimationCoding.Bounce => new Bounce(yAxisLedCount, xAxisLedCount, new ColorRGB(255, 0, 0)),
                AnimationCoding.Rainbow => new Rainbow(yAxisLedCount, xAxisLedCount),
                _ => throw new ArgumentException(string.Format(ErrorTexts.ENUMS_NOT_HANDLE, type, "SetStreaming()")),
            };
        }

        /// <summary>
        /// Set Screen sync or OpenTK sync of streaming
        /// </summary>
        /// <param name="syncHelper">Synchelper</param>
        /// <param name="captureInfos">Capture Infos</param>
        public void SetStreaming(ISyncHelper syncHelper, CaptureInfos captureInfos)
        {
            _streaming = new SyncBase(captureInfos, _model.Layouts, syncHelper);
        }

        /// <summary>
        /// Get and process streaming
        /// </summary>
        /// <param name="_isMirror">Animation mirror</param>
        /// <param name="brightness">Brightness percentage</param>
        public void ProcessStreaming(bool _isMirror, float brightness)
        {
            lock (locker)
            {
                if (_streaming != null && _isTurnOn)
                {
                    var colorMatrix = _streaming.Process(brightness);
                    if (_isMirror)
                        colorMatrix = ColorMirror(colorMatrix);
                    ProcessColor(colorMatrix);
                }
            }
        }

        /// <summary>
        /// Manually set color for each key
        /// </summary>
        /// <param name="keyColors">Dictionay of each key and its color</param>
        public void ProcessZoneSelect(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            lock (locker)
            {
                if (_isTurnOn)
                {
                    ProcessZoneSelection(keyColors);
                }
            }
        }

        /// <summary>
        /// Flip color matrix upside down
        /// </summary>
        /// <param name="colorMatrix"></param>
        /// <returns></returns>
        private ColorRGB[,] ColorMirror(ColorRGB[,] colorMatrix)
        {
            ColorRGB[,] mirrorMatrix = new ColorRGB[yAxisLedCount, xAxisLedCount];
            for (int j = 0; j < yAxisLedCount; j++)
            {
                for (int i = 0; i < xAxisLedCount; i++)
                {
                    mirrorMatrix[j, i] = colorMatrix[yAxisLedCount - 1 - j, i];
                }
            }

            return mirrorMatrix;
        }

        public void ProcessStaticColors(ColorRGB[,] colorMatrix, float _brightness)
        {
            if (_isTurnOn)
            {
                ColorRGB[,] layoutColors = Methods.Convert2LayoutColors(colorMatrix, _model.Layouts, _brightness);
                ProcessColor(layoutColors);
            }
        }

        public virtual void SetLedTurnOn(bool isTurnOn)
        {
            lock (locker)
            {
                if (!isTurnOn)
                {
                    TurnOffLed();
                    _LedTurnOff?.Invoke();
                }

                _isTurnOn = isTurnOn;
            }
        }

        /// <summary>
        /// Get current lighting device of model
        /// </summary>
        /// <returns></returns>
        public LightingModel GetModel()
        {
            if (_model != null)
            {
                return _model;
            }
            else
            {
                InitModel();
                return _model;
            }
        }

        /// <summary>
        /// Return Color for every keys
        /// </summary>
        /// <returns></returns>
        public Dictionary<Keyboard, ColorRGB> GetKeyColor()
        {
            return keyColor;
        }

        /// <summary>
        /// Get display color of this lighting base
        /// </summary>
        /// <returns></returns>
        public List<byte> GetDisplayColors()
        {
            return _displayColorBytes;
        }

        /// <summary>
        /// Get display color of this lighting base
        /// </summary>
        /// <returns></returns>
        public string GetHttpClientColorData()
        {
            return _httpClientColorData;
        }

        /// <summary>
        /// Get led on/off status
        /// </summary>
        /// <returns>true if led on/ false if led off</returns>
        public bool IsLedOn()
        {
            return _isTurnOn;
        }
    }
}
