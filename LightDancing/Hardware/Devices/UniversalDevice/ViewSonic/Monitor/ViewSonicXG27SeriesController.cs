using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Linq;
using System;
using LightDancing.Models.ViewSonic;

namespace LightDancing.Hardware.Devices.UniversalDevice.ViewSonic.Monitor
{
    public class ViewSonicXG27SeriesDevice: USBDeviceBase
    {
        private readonly ViewSonicXG27SeriesConfigModels _config;

        public ViewSonicXG27SeriesDevice(HidStream deviceStream, ViewSonicXG27SeriesConfigModels config) : base(deviceStream)
        {
            _config = config;
            this._model.USBDeviceType = _config.USBDeviceType; 
            SetLightingBase();
        }

        protected override HardwareModel InitModel()
        {
            return new LightingModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return null;
        }

        private void SetLightingBase()
        {
            var hardwares = new List<LightingBase>();
            var monitor = new ViewSonicXG27SeriesLightingBase(_model, _config);
            hardwares.Add(monitor);

            _lightingBase = hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            try
            {
                if (_lightingBase.Count > 0)
                {
                    var lightingBase = _lightingBase.FirstOrDefault();
                    if (process)
                    {
                        lightingBase.ProcessStreaming(false, brightness);
                    }

                    ((HidStream)_deviceStream).SetFeature(lightingBase.GetDisplayColors().ToArray());
                }
            }
            catch
            {
            }
        }
    }

    public class ViewSonicXG27SeriesLightingBase: LightingBase
    {
        /// <summary>
        /// Canvs Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 2;

        /// <summary>
        /// Canvs X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 1;

        ///// <summary>
        ///// key = Canvs's key, Value = 2D position on Canvs (ex: ESC = (0, 0)), PosistionY = y position, PosistionX = x position
        ///// </summary>
        private readonly List<LayoutModel> KEYS_LAYOUTS = new List<LayoutModel>()
        {
            new LayoutModel(){ LED = Keyboard.LED1, PosistionY = 1, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED2, PosistionY = 0, PosistionX = 0 },
        };

        private readonly ViewSonicXG27SeriesConfigModels _config;

        public ViewSonicXG27SeriesLightingBase(HardwareModel hardwareModel, ViewSonicXG27SeriesConfigModels config) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
            _config = config;
            this._model.Type = _config.LightingDevicesType;
            this._model.Name = _config.Name;
        }

        /// <summary>
        /// Init the model
        /// MEMO: Device ID should be unique, the better way is get it from the firmware.
        /// </summary>
        /// <returns></returns>
        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                USBDeviceType = _usbModel.USBDeviceType,
            };
        }

        /// <summary>
        /// Need To 
        /// </summary>
        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = CreateCommand((i, command) =>
            {
                LayoutModel model = KEYS_LAYOUTS[i];
                ColorRGB positionColor = colorMatrix[model.PosistionY, model.PosistionX];
                command[(i * 7) + 2] = positionColor.R;
                command[(i * 7) + 3] = positionColor.G;
                command[(i * 7) + 4] = positionColor.B;
            }).ToList();
        }

        protected override void TurnOffLed()
        {
            _displayColorBytes = CreateCommand((i, command) =>
            {
                command[(i * 7) + 2] = 0x00;
                command[(i * 7) + 3] = 0x00;
                command[(i * 7) + 4] = 0x00;
            }).ToList();
        }

        private byte[] CreateCommand(Action<int, byte[]> action)
        {
            byte[] command = new byte[_config.MaxFeatureLength];
            command[0] = 0x02;
            command[1] = 0x01; // Downward LEDs Mode
            command[6] = 0x0A;
            command[8] = 0x01; // Back LEDs Mode
            command[13] = 0x0A;

            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                action(i, command);
            }

            return command;
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
