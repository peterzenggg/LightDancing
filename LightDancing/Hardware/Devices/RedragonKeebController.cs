using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class RedragonKeebController : ILightingControl
    {
        private const int DEVICE_VID = 0x3402; //Original 0x258A
        private const int DEVICE_PID = 0x0301; //Original 0x0121
        private const int MAX_FEATURE_LENGTH = 33;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    RedragonKeyboardDevice keyboard = new RedragonKeyboardDevice(stream);
                    hardwares.Add(keyboard);
                }

                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class RedragonKeyboardDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 33;

        private bool _isUIControl = false;

        public RedragonKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            RedragonKeeb keyboard = new RedragonKeeb(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                SetCurrentLedEffectOff();
                _isUIControl = true;
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"Redragon KM7 device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }
                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                for (int i = 0; i < displayColors.Count / MAX_REPORT_LENGTH; i++)
                {
                    byte[] result = displayColors.GetRange(MAX_REPORT_LENGTH * i, MAX_REPORT_LENGTH).ToArray();
                    try
                    {
                        ((HidStream)_deviceStream).SetFeature(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on Redragon Keyboard");
                    }
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;

            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.RedragonKeeb,
                Name = "iBP KM7 Keyboard"
            };
        }

        /// <summary>
        /// Feature report ID = 0x08
        /// </summary>
        public void SetCurrentLedEffectOff()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x08; //report id
            commands[1] = 0x09;
            commands[2] = 0xf9;
            commands[3] = 0x00;

            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on Redragon Keyboard");
            }
        }

        /// <summary>
        /// Feature report ID = 0x08
        /// </summary>
        public void SetCurrentLedEffectOn()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x08; //report id
            commands[1] = 0x09;
            commands[2] = 0xf9;
            commands[3] = 0x01;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
                _isUIControl = false;
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on Redragon Keyboard");
            }
        }

        public override void TurnFwAnimationOn()
        {
            SetCurrentLedEffectOn();
        }
    }

    public class RedragonKeeb : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 11;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 33;

        private bool _isUIControl = false;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.LED1,      Tuple.Create(5, 9) },
            { Keyboard.LED2,      Tuple.Create(5, 8) },
            { Keyboard.LED3,      Tuple.Create(5, 7) },
            { Keyboard.LED4,      Tuple.Create(5, 6) },
            { Keyboard.LED5,      Tuple.Create(5, 5) },
            { Keyboard.LED6,      Tuple.Create(5, 4) },
            { Keyboard.LED7,      Tuple.Create(5, 3) },
            { Keyboard.LED8,      Tuple.Create(5, 2) },
            { Keyboard.LED9,      Tuple.Create(5, 1) },
            { Keyboard.LED10,     Tuple.Create(4, 0) },
            { Keyboard.LED11,     Tuple.Create(3, 0) },
            { Keyboard.LED12,     Tuple.Create(2, 0) },
            { Keyboard.LED13,     Tuple.Create(1, 0) },
            { Keyboard.LED14,     Tuple.Create(0, 1) },
            { Keyboard.LED15,     Tuple.Create(0, 2) },
            { Keyboard.LED16,     Tuple.Create(0, 3) },
            { Keyboard.LED17,     Tuple.Create(0, 4) },
            { Keyboard.LED18,     Tuple.Create(0, 5) },
            { Keyboard.LED19,     Tuple.Create(0, 6) },
            { Keyboard.LED20,     Tuple.Create(0, 7) },
            { Keyboard.LED21,     Tuple.Create(1, 8) },
            { Keyboard.LED22,     Tuple.Create(1, 9) },
            { Keyboard.LED23,     Tuple.Create(3, 10) },
            { Keyboard.LED24,     Tuple.Create(5, 10) },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){   Keyboard.LED1,       Keyboard.LED2,       Keyboard.LED3,       Keyboard.LED4,       Keyboard.LED5,
                                        Keyboard.LED6,       Keyboard.LED7,       Keyboard.LED8,       Keyboard.LED9,       } },

            {2, new List<Keyboard>(){   Keyboard.LED10,      Keyboard.LED11,      Keyboard.LED12,      Keyboard.LED13,      Keyboard.LED14,
                                        Keyboard.LED15,      Keyboard.LED16,      Keyboard.LED17,      Keyboard.LED18,      } },

            {3, new List<Keyboard>(){   Keyboard.LED19,      Keyboard.LED20,      Keyboard.LED21,      Keyboard.LED22,      Keyboard.LED23,
                                        Keyboard.LED24,      } },
        };

        public RedragonKeeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.RedragonKeeb,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "iBP KM7 Keyboard"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){   Keyboard.LED1,       Keyboard.LED2,       Keyboard.LED3,       Keyboard.LED4,       Keyboard.LED5,
                                        Keyboard.LED6,       Keyboard.LED7,       Keyboard.LED8,       Keyboard.LED9,       Keyboard.LED10,
                                        Keyboard.LED11,      Keyboard.LED12,      Keyboard.LED13,      Keyboard.LED14,      Keyboard.LED15,
                                        Keyboard.LED16,      Keyboard.LED17,      Keyboard.LED18,      Keyboard.LED19,      Keyboard.LED20,
                                        Keyboard.LED21,      Keyboard.LED22,      Keyboard.LED23,      Keyboard.LED24 },
            };
        }

        /// <summary>
        /// Each command has 33 bytes, and total is 3 commands
        /// Each commnad should contain 9 keys (or less)
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();

            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[0] = 0x08; //report id
                result[1] = 0x08;
                result[2] = 0xf8;
                result[3] = (byte)rowMappings.Key;

                foreach (Keyboard cloumn in rowMappings.Value)
                {
                    if (cloumn != Keyboard.NA)
                    {
                        var index = KEYS_LAYOUTS[cloumn];
                        var color = colorMatrix[index.Item1, index.Item2];
                        result[count * 3 + 6] = color.R;
                        result[count * 3 + 7] = color.G;
                        result[count * 3 + 8] = color.B;

                        keyColor.Add(cloumn, color);
                    }
                    count++;
                }

                if (count <= 9)
                {
                    _displayColorBytes.AddRange(result);
                }
                else
                {
                    Trace.WriteLine($"Led count error on Redragon Keyboard");
                }
            }
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            _displayColorBytes = new List<byte>();
            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];

            for (byte i = 1; i <= 3; i++)
            {
                collectBytes[0] = 0x08; //report id
                collectBytes[1] = 0x08;
                collectBytes[2] = 0xf8;
                collectBytes[3] = i;

                _displayColorBytes.AddRange(collectBytes);
            }
        }
    }
}
