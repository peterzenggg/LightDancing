using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Diagnostics;
using System.Linq;

namespace LightDancing.Hardware.Devices.UniversalDevice.Razer.Tower
{
    internal class RazerBaseStationChromaConfig
    {
        /// <summary>
        /// Razer Send Max Feature Buffer LENGTH
        /// </summary>
        internal const int MAX_FEATURE_LENGTH = 91;
    }

    internal class RazerBaseStationChromaController : ILightingControl
    {
        private const int DEVICE_VID = 0x1532;
        private const int DEVICE_PID = 0x0f08;
        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, RazerBaseStationChromaConfig.MAX_FEATURE_LENGTH, true);
            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    BaseStationChromaDevice towerDevice = new BaseStationChromaDevice(stream);
                    hardwares.Add(towerDevice);
                }

                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class BaseStationChromaDevice : USBDeviceBase
    {
        private bool _isUIControl = false;
        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                USBDeviceType = USBDevices.RazerBaseStationChroma,
                Name = "Razer Base Station Chroma"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            BaseStationChroma tower = new BaseStationChroma(_model);
            hardwares.Add(tower);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                try
                {
                    SetCurrentLedEffectOff();
                    _isUIControl = true;
                }
                catch
                {
                    _isUIControl = false;
                    Trace.WriteLine($"RazerBaseStationChroma SetCurrentLedEffectOff Fail");
                }
            }
            if (_isUIControl)
            {
                if (process && _lightingBase.Count > 0)
                {
                    _lightingBase.FirstOrDefault().ProcessStreaming(false, brightness);
                }
                if (_lightingBase.Count > 0)
                {
                    try
                    {
                        ((HidStream)_deviceStream).SetFeature(_lightingBase.FirstOrDefault().GetDisplayColors().ToArray());
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on RazerBaseStationChroma");
                    }
                }
            }
        }

        public void SetCurrentLedEffectOff()
        {
            byte[] SendCommand = new byte[91];
            SendCommand[2] = 0x1F;
            SendCommand[6] = 0x06;
            SendCommand[7] = 0x0F;
            SendCommand[8] = 0x02;
            SendCommand[11] = 0x08;
            SendCommand[89] = Methods.CalculateRazerAccessByte(SendCommand);
            ((HidStream)_deviceStream).SetFeature(SendCommand);
        }

        public BaseStationChromaDevice(HidStream deviceStream) : base(deviceStream)
        {
        }
    }
    public class BaseStationChroma : LightingBase
    {
        /// <summary>
        /// Canvs Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 1;
        /// <summary>
        /// Canvs X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 16;

        ///// <summary>
        ///// key = Canvs's key, Value = 2D position on Canvs (ex: ESC = (0, 0)), PosistionY = y position, PosistionX = x position
        ///// </summary>
        private readonly List<LayoutModel> KEYS_LAYOUTS = new List<LayoutModel>()
        {
            new LayoutModel(){ LED = Keyboard.LED1, PosistionY = 0, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED2, PosistionY = 0, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED3, PosistionY = 0, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED4, PosistionY = 0, PosistionX = 3 },
            new LayoutModel(){ LED = Keyboard.LED5, PosistionY = 0, PosistionX = 4 },
            new LayoutModel(){ LED = Keyboard.LED6, PosistionY = 0, PosistionX = 5 },
            new LayoutModel(){ LED = Keyboard.LED7, PosistionY = 0, PosistionX = 6 },
            new LayoutModel(){ LED = Keyboard.LED8, PosistionY = 0, PosistionX = 7 },
            new LayoutModel(){ LED = Keyboard.LED9, PosistionY = 0, PosistionX = 8 },
            new LayoutModel(){ LED = Keyboard.LED10, PosistionY = 0, PosistionX = 9 },
            new LayoutModel(){ LED = Keyboard.LED11, PosistionY = 0, PosistionX = 10 },
            new LayoutModel(){ LED = Keyboard.LED12, PosistionY = 0, PosistionX = 11 },
            new LayoutModel(){ LED = Keyboard.LED13, PosistionY = 0, PosistionX = 12 },
            new LayoutModel(){ LED = Keyboard.LED14, PosistionY = 0, PosistionX = 13 },
            new LayoutModel(){ LED = Keyboard.LED15, PosistionY = 0, PosistionX = 14 },
            new LayoutModel(){ LED = Keyboard.LED16, PosistionY = 0, PosistionX = 15 },
        };

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
                Type = LightingDevices.RazerBaseStationChroma,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Razer Base Station Chroma"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        /// <summary>
        /// All Led Convert to 91 bytes Command
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            byte[] commandCollect = new byte[RazerBaseStationChromaConfig.MAX_FEATURE_LENGTH];
            commandCollect[2] = 0x1F;
            commandCollect[6] = 0x32;
            commandCollect[7] = 0x0F;
            commandCollect[8] = 0x03;
            commandCollect[13] = 0x0E;
            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                int ledIndex = (i * 3) + 14;
                var model = KEYS_LAYOUTS[i];
                ColorRGB color = colorMatrix[model.PosistionY, model.PosistionX];
                commandCollect[ledIndex] = color.R;
                commandCollect[ledIndex + 1] = color.G;
                commandCollect[ledIndex + 2] = color.B;
            }
            commandCollect[89] = Methods.CalculateRazerAccessByte(commandCollect);
            _displayColorBytes = commandCollect.ToList();
        }

        public BaseStationChroma(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void TurnOffLed()
        {
        }
    }
}
