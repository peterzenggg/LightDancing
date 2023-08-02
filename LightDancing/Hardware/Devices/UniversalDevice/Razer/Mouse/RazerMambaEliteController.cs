using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Linq;
using System;

namespace LightDancing.Hardware.Devices.UniversalDevice.Razer.Mouse
{
    internal class RazerMambaEliteConfig
    {
        /// <summary>
        /// Razer Byte Array Posistion For Access Byte 
        /// </summary>
        internal const int RAZER_ACCESS_BYTE = 89;
        /// <summary>
        /// Razer Send Max Feature Buffer LENGTH
        /// </summary>
        internal const int MAX_FEATURE_LENGTH = 91;
    }

    internal class RazerMambaEliteController : ILightingControl
    {
        private const int DEVICE_VID = 0x1532;
        private const int DEVICE_PID = 0x006C;
        public List<USBDeviceBase> InitDevices()
        {
            List<HidStream> streams = new HidDetector().GetHidStreams(DEVICE_VID, DEVICE_PID, RazerMambaEliteConfig.MAX_FEATURE_LENGTH, true);
            if (streams != null && streams.Any())
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    RazerMambaEliteDevice mouse = new RazerMambaEliteDevice(stream);
                    hardwares.Add(mouse);
                }
                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class RazerMambaEliteDevice : USBDeviceBase
    {
        private bool _isUIControl = false;
        public RazerMambaEliteDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                USBDeviceType = USBDevices.RazerMambaElite,
                Name = "Razer Mamba Elite"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return new List<LightingBase>()
            {
                new RazerMambaElite(_model)
            };
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Fail to SetCurrentLedEffectOff on RazerMambaElite. Exception:{ex}");
                    return;
                }
            }

            if (_lightingBase != null && _lightingBase.Any())
            {
                var lightingBase = _lightingBase.FirstOrDefault();

                if (process)
                {
                    lightingBase.ProcessStreaming(false, brightness);
                }

                SendStream(lightingBase.GetDisplayColors().ToArray());
            }
        }

        private void SendStream(byte[] byteArray)
        {
            try
            {
                ((HidStream)_deviceStream).SetFeature(byteArray);
            }
            catch
            {
                Console.WriteLine($"Fail to streaming on RazerMambaElite");
            }
        }

        public void SetCurrentLedEffectOff()
        {
            byte[] command = new byte[RazerMambaEliteConfig.MAX_FEATURE_LENGTH];
            command[2] = 0x1F;
            command[6] = 0x06;
            command[7] = 0x0F;
            command[8] = 0x02;
            command[11] = 0x08;
            command[12] = 0x01;
            command[13] = 0x01;
            command[RazerMambaEliteConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(command);

            SendStream(command);
        }

        public override void TurnFwAnimationOn()
        {
            byte[] command = new byte[RazerMambaEliteConfig.MAX_FEATURE_LENGTH];
            command[2] = 0x1F;
            command[6] = 0x06;
            command[7] = 0x0F;
            command[8] = 0x02;
            command[9] = 0x01;
            command[11] = 0x03;
            command[13] = 0x028;
            command[RazerMambaEliteConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(command);

            SendStream(command);
        }
    }

    public class RazerMambaElite : LightingBase
    {
        /// <summary>
        /// Canvs Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 9;
        /// <summary>
        /// Canvs X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 3;
        ///// <summary>
        ///// key = Canvs's key, Value = 2D position on Canvs (ex: ESC = (0, 0)), PosistionY = y position, PosistionX = x position
        ///// </summary>
        private readonly List<LayoutModel> KEYS_LAYOUTS = new List<LayoutModel>()
        {
            new LayoutModel(){ LED = Keyboard.LED1, PosistionY = 0, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED2, PosistionY = 7, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED3, PosistionY = 0, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED4, PosistionY = 1, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED5, PosistionY = 2, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED6, PosistionY = 3, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED7, PosistionY = 4, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED8, PosistionY = 5, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED9, PosistionY = 6, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED10, PosistionY = 7, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED11, PosistionY = 8, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED12, PosistionY = 0, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED13, PosistionY = 1, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED14, PosistionY = 2, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED15, PosistionY = 3, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED16, PosistionY = 4, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED17, PosistionY = 5, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED18, PosistionY = 6, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED19, PosistionY = 7, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED20, PosistionY = 8, PosistionX = 2 },
        };

        public RazerMambaElite(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
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
                Type = LightingDevices.RazerMambaElite,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Razer Mamba Elite"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            BuildColorBytes(colorMatrix);
        }

        protected override void TurnOffLed()
        {
            BuildColorBytes();
        }

        private void BuildColorBytes(ColorRGB[,] colorMatrix = null)
        {
            byte[] command = new byte[RazerMambaEliteConfig.MAX_FEATURE_LENGTH];
            command[2] = 0x1F;
            command[6] = 0x41;
            command[7] = 0x0F;
            command[8] = 0x03;
            command[13] = 0x13;
            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                int colorCount = (i * 3 + 14);
                ColorRGB posistionColor;
                if (colorMatrix != null)
                {
                    LayoutModel model = KEYS_LAYOUTS[i];
                    posistionColor = colorMatrix[model.PosistionY, model.PosistionX];
                }
                else
                {
                    posistionColor = ColorRGB.Black();
                }
                command[colorCount] = posistionColor.R;
                command[colorCount + 1] = posistionColor.G;
                command[colorCount + 2] = posistionColor.B;
            }
            command[RazerMambaEliteConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(command);
            _displayColorBytes = command.ToList();
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
