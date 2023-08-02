using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class RedragonMouseController : ILightingControl
    {
        private const int DEVICE_VID = 0x3402; //Original 0x258A
        private const int DEVICE_PID = 0x0200; //Original 0x0122
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
                    RedragonMouseDevice mouse = new RedragonMouseDevice(stream);
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

    public class RedragonMouseDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 33;

        private bool _isUIControl = false;

        public RedragonMouseDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            RedragonMouse mouse = new RedragonMouse(_model);
            hardwares.Add(mouse);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                TurnOnUIControl();
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
                        Trace.WriteLine($"False to streaming on Redragon Mouse");
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
                USBDeviceType = USBDevices.RedragonMouse,
                Name = "iBP KM7 Mouse"
            };
        }

        /// <summary>
        /// Feature report ID = 0x08
        /// </summary>
        public void TurnOnUIControl()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x08; //report id
            commands[1] = 0x2d;
            commands[2] = 0x00;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on Redragon Mouse");
            }
        }

        /// <summary>
        /// Feature report ID = 0x08
        /// </summary>
        public void TurnOffUIControl()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x08; //report id
            commands[1] = 0x2d;
            commands[2] = 0x01;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
                _isUIControl = false;
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on Redragon Mouse");
            }
        }

        public override void TurnFwAnimationOn()
        {
            TurnOffUIControl();
        }
    }

    public class RedragonMouse : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 5;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 4;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 33;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            //Row 1
            { Keyboard.LED1,      Tuple.Create(0, 1) },
            { Keyboard.LED2,      Tuple.Create(2, 0) },
            { Keyboard.LED3,      Tuple.Create(4, 0) },
            { Keyboard.LED4,      Tuple.Create(4, 3) },
            { Keyboard.LED5,      Tuple.Create(2, 3) },
            { Keyboard.LED6,      Tuple.Create(0, 2) },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6 } },
        };

        public RedragonMouse(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        /// <summary>
        /// Init the model
        /// MEMO: Device ID should be unique, the better way is get it from the firmware.
        /// Add Turn On Scroll Wheels
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
                Type = LightingDevices.RedragonMouse,
                Name = "iBP KM7 Mouse"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6 },
            };
        }

        /// <summary>
        /// Each command has 33 bytes, and total is 1 commands
        /// Each commnad should contain 6 keys (or less)
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[0] = 0x08;
                result[1] = 0x1d;
                result[2] = (byte)rowMappings.Key;

                foreach (Keyboard cloumn in rowMappings.Value)
                {
                    if (cloumn != Keyboard.NA)
                    {
                        var index = KEYS_LAYOUTS[cloumn];
                        var color = colorMatrix[index.Item1, index.Item2];
                        result[count * 3 + 3] = color.R;
                        result[count * 3 + 4] = color.G;
                        result[count * 3 + 5] = color.B;
                        keyColor.Add(cloumn, color);
                    }
                    count++;
                }

                if (count <= 6)
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

            for (byte i = 1; i <= 1; i++)
            {
                collectBytes[0] = 0x08;
                collectBytes[1] = 0x1d;
                collectBytes[2] = i;

                _displayColorBytes.AddRange(collectBytes);
            }
        }
    }
}
