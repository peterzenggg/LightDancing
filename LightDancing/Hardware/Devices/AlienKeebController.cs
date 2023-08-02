using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class AlienKeebController : ILightingControl
    {
        private const int DEVICE_VID = 0x04F2;
        private const int DEVICE_PID = 0x1830;
        private const int MAX_FEATURE_LENGTH = 65;
        private const int MAX_OUTPUT_LENGTH = 65;
        private const int MAX_INPUT_LENGTH = 65;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH, MAX_OUTPUT_LENGTH, MAX_INPUT_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    AlienKeyboardDevice keyboard = new AlienKeyboardDevice(stream);
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

    public class AlienKeyboardDevice : USBDeviceBase
    {
        public AlienKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            AlienKeyboard keyboard = new AlienKeyboard(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (process)
            {
                foreach (var device in _lightingBase)
                {
                    device.ProcessStreaming(false, brightness);
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = deviceID,
                //Type = LightingDevices.AlienKeyboard,
                Name = "Alienware Keeb",
            };
        }

        private string GetFirmwareVersion()
        {
            return "N/A";
        }
    }

    public class AlienKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 15;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 65;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            //Row 1
            { Keyboard.ESC, Tuple.Create(0, 0)},
            { Keyboard.F1, Tuple.Create(0, 3)},
            { Keyboard.F2, Tuple.Create(0, 4)},
            { Keyboard.F3, Tuple.Create(0, 5)},
            { Keyboard.F4, Tuple.Create(0, 6)},
            { Keyboard.F5, Tuple.Create(0, 7)},
            { Keyboard.F6, Tuple.Create(0, 8)},
            { Keyboard.F7, Tuple.Create(0, 9)},
            { Keyboard.F8, Tuple.Create(0, 10)},
            { Keyboard.F9, Tuple.Create(0, 11)},
            { Keyboard.F10, Tuple.Create(0, 12)},
            { Keyboard.F11, Tuple.Create(0, 13)},
            { Keyboard.F12, Tuple.Create(0, 14)},
            //Row 2
            { Keyboard.Tilde, Tuple.Create(1, 0)},
            { Keyboard.One, Tuple.Create(1, 2) },
            { Keyboard.Two, Tuple.Create(1, 3) },
            { Keyboard.Three, Tuple.Create(1, 4) },
            { Keyboard.Four, Tuple.Create(1, 5) },
            { Keyboard.Five, Tuple.Create(1, 6) },
            { Keyboard.Six, Tuple.Create(1, 7) },
            { Keyboard.Seven, Tuple.Create(1, 8) },
            { Keyboard.Eight, Tuple.Create(1, 9) },
            { Keyboard.Nine, Tuple.Create(1, 10) },
            { Keyboard.Zero, Tuple.Create(1, 11) },
            { Keyboard.Hyphen, Tuple.Create(1, 12) },
            { Keyboard.Plus, Tuple.Create(1, 13) },
            { Keyboard.BackSpace, Tuple.Create(1, 14)},
            //Row 3
            { Keyboard.Tab, Tuple.Create(2, 0)},
            { Keyboard.Q, Tuple.Create(2, 2) },
            { Keyboard.W, Tuple.Create(2, 3) },
            { Keyboard.E, Tuple.Create(2, 4) },
            { Keyboard.R, Tuple.Create(2, 5) },
            { Keyboard.T, Tuple.Create(2, 6) },
            { Keyboard.Y, Tuple.Create(2, 7) },
            { Keyboard.U, Tuple.Create(2, 8) },
            { Keyboard.I, Tuple.Create(2, 9) },
            { Keyboard.O, Tuple.Create(2, 10) },
            { Keyboard.P, Tuple.Create(2, 11) },
            { Keyboard.LeftBracket, Tuple.Create(2, 12) },
            { Keyboard.RightBracket, Tuple.Create(2, 13) },
            { Keyboard.BackSlash, Tuple.Create(2, 14)},
            //Row 4
            { Keyboard.CapLock, Tuple.Create(3, 0)},
            { Keyboard.A, Tuple.Create(3, 2) },
            { Keyboard.S, Tuple.Create(3, 3) },
            { Keyboard.D, Tuple.Create(3, 4) },
            { Keyboard.F, Tuple.Create(3, 5) },
            { Keyboard.G, Tuple.Create(3, 6) },
            { Keyboard.H, Tuple.Create(3, 7) },
            { Keyboard.J, Tuple.Create(3, 8) },
            { Keyboard.K, Tuple.Create(3, 9) },
            { Keyboard.L, Tuple.Create(3, 10) },
            { Keyboard.Semicolon, Tuple.Create(3, 11) },
            { Keyboard.SingleQuote, Tuple.Create(3, 12) },
            { Keyboard.Enter, Tuple.Create(3, 14)},
            //Row 5
            { Keyboard.LeftShift, Tuple.Create(4, 0)},
            { Keyboard.Z, Tuple.Create(4, 2) },
            { Keyboard.X, Tuple.Create(4, 3) },
            { Keyboard.C, Tuple.Create(4, 4) },
            { Keyboard.V, Tuple.Create(4, 5) },
            { Keyboard.B, Tuple.Create(4, 6) },
            { Keyboard.N, Tuple.Create(4, 7) },
            { Keyboard.M, Tuple.Create(4, 8) },
            { Keyboard.Comma, Tuple.Create(4, 9) },
            { Keyboard.Period, Tuple.Create(4, 10) },
            { Keyboard.Slash, Tuple.Create(4, 11) },
            { Keyboard.RightShift, Tuple.Create(4, 14)},
            //Row 6
            { Keyboard.CTRL_L, Tuple.Create(5, 0)},
            { Keyboard.WIN_L, Tuple.Create(5, 2) },
            { Keyboard.ALT_L, Tuple.Create(5, 3) },
            { Keyboard.ALT_R, Tuple.Create(5, 10) },
            { Keyboard.FN, Tuple.Create(5, 11) },
            { Keyboard.MOUSE, Tuple.Create(5, 12) },
            { Keyboard.CTRL_R, Tuple.Create(5, 14)}
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){ Keyboard.ESC, Keyboard.F1, Keyboard.F2, Keyboard.F3, Keyboard.Tilde, Keyboard.One, Keyboard.Two, Keyboard.Three, Keyboard.Tab, Keyboard.Q, Keyboard.W, Keyboard.E, Keyboard.CapLock, Keyboard.A, Keyboard.S, Keyboard.D, Keyboard.LeftShift, Keyboard.Z, Keyboard.X} },
            {2, new List<Keyboard>(){ Keyboard.CTRL_L, Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.NA, Keyboard.F12, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.Plus, Keyboard.NA, Keyboard.BackSpace, Keyboard.NA, Keyboard.F8, Keyboard.F9, Keyboard.F10, Keyboard.F11, Keyboard.Eight, Keyboard.Nine, Keyboard.Zero, Keyboard.Hyphen } },
            {3, new List<Keyboard>(){ Keyboard.I, Keyboard.O, Keyboard.P, Keyboard.LeftBracket, Keyboard.K, Keyboard.L, Keyboard.Semicolon, Keyboard.SingleQuote, Keyboard.F4, Keyboard.F5, Keyboard.F6, Keyboard.F7, Keyboard.Four, Keyboard.Five, Keyboard.Six, Keyboard.Seven, Keyboard.R, Keyboard.T, Keyboard.Y, Keyboard.U} },
            {4, new List<Keyboard>(){ Keyboard.F, Keyboard.G, Keyboard.H, Keyboard.J, Keyboard.C, Keyboard.V, Keyboard.B, Keyboard.N, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.ALT_R, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.RightBracket, Keyboard.NA, Keyboard.NA, Keyboard.RightShift, Keyboard.CTRL_R} },
            {5, new List<Keyboard>(){ Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.NA, Keyboard.M, Keyboard.Comma, Keyboard.Period, Keyboard.Slash } },
            {6, new List<Keyboard>(){ Keyboard.FN, Keyboard.MOUSE, Keyboard.NA, Keyboard.NA, Keyboard.BackSlash, Keyboard.NA, Keyboard.Enter} },
            {7, new List<Keyboard>(){ Keyboard.NA} },
            {8, new List<Keyboard>(){ Keyboard.NA} },
            {9, new List<Keyboard>(){ Keyboard.NA} },
        };

        public AlienKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Name = "Alienware Keeb",
                //Type = LightingDevices.AlienKeyboard,
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
                //Row 1
                new List<Keyboard>(){ Keyboard.ESC, Keyboard.F1, Keyboard.F2, Keyboard.F3, Keyboard.F4, Keyboard.F5, Keyboard.F6, Keyboard.F7, Keyboard.F8, Keyboard.F9, 
                                      Keyboard.F10, Keyboard.F11, Keyboard.F12 },
                //Row 2
                new List<Keyboard>(){ Keyboard.Tilde, Keyboard.One, Keyboard.Two, Keyboard.Three, Keyboard.Four,
                                      Keyboard.Five, Keyboard.Six, Keyboard.Seven, Keyboard.Eight, Keyboard.Nine, Keyboard.Zero, Keyboard.Hyphen, Keyboard.Plus,
                                      Keyboard.BackSpace},
                //Row 3
                new List<Keyboard>(){ Keyboard.Tab, Keyboard.Q, Keyboard.W, Keyboard.E, Keyboard.R, Keyboard.T,
                                      Keyboard.Y, Keyboard.U, Keyboard.I, Keyboard.O, Keyboard.P, Keyboard.LeftBracket, Keyboard.RightBracket, Keyboard.BackSlash },
                //Row 4
                new List<Keyboard>(){ Keyboard.CapLock, Keyboard.A, Keyboard.S, Keyboard.D, Keyboard.F, Keyboard.G, Keyboard.H,
                                      Keyboard.J, Keyboard.K, Keyboard.L, Keyboard.Semicolon, Keyboard.SingleQuote, Keyboard.Enter },
                //Row 5
                new List<Keyboard>(){ Keyboard.LeftShift,Keyboard.Z, Keyboard.X, Keyboard.C, Keyboard.V,Keyboard.B,
                                      Keyboard.N, Keyboard.M, Keyboard.Comma, Keyboard.Period, Keyboard.Slash,Keyboard.RightShift },
                //Row 6
                new List<Keyboard>(){ Keyboard.CTRL_L,Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.ALT_R, Keyboard.FN, Keyboard.MOUSE, Keyboard.CTRL_R}
            };
        }

        /// <summary>
        /// 2023/02/18
        /// Currently not using this device, just backup for SetFeature usage
        /// Might have error with USBDeviceBase
        /// Each command has 65 bytes, and total is 9 commands
        /// Each commnad should contain 20 keys (or less)
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[1] = 0x11;
                result[2] = 0xf0;
                result[3] = (byte)rowMappings.Key;
                try
                {
                    //((HidStream)_deviceStream).SetFeature(result);
                }
                catch
                {
                    Trace.WriteLine($"False to set feature on AlienKeyboard");
                }

                foreach (Keyboard cloumn in rowMappings.Value)
                {
                    if (cloumn != Keyboard.NA)
                    {
                        var index = KEYS_LAYOUTS[cloumn];
                        var color = colorMatrix[index.Item1, index.Item2];
                        result[count * 3 + 5] = color.R;
                        result[count * 3 + 6] = color.G;
                        result[count * 3 + 7] = color.B;
                        keyColor.Add(cloumn, color);
                    }
                    count++;
                }

                if (count <= 20)
                {
                    try
                    {
                        //_deviceStream.Write(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on AlienKeyboard");
                    }
                }
            }
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[1] = 0x11;
                result[2] = 0xf0;
                result[3] = (byte)rowMappings.Key;
                try
                {
                    //((HidStream)_deviceStream).SetFeature(result);
                }
                catch
                {
                    Trace.WriteLine($"False to set feature on AlienKeyboard");
                }

                foreach (Keyboard cloumn in rowMappings.Value)
                {
                    if (cloumn != Keyboard.NA)
                    {
                        var color = keyColors[cloumn];
                        result[count * 3 + 5] = color.R;
                        result[count * 3 + 6] = color.G;
                        result[count * 3 + 7] = color.B;
                    }
                    count++;
                }

                if (count <= 20)
                {
                    try
                    {
                        //_deviceStream.Write(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on AlienKeyboard");
                    }
                }
            }
        }


        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];
            for (byte i = 1; i <= 9; i++)
            {
                collectBytes[1] = 0x11;
                collectBytes[2] = 0xf0;
                collectBytes[3] = i;
                try
                {
                    //((HidStream)_deviceStream).SetFeature(collectBytes);

                    //_deviceStream.Write(collectBytes);
                }
                catch
                {
                    Trace.WriteLine($"False to turm off on AlienKeyboard");
                }
            }
        }
    }
}
