using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.Corsair.Keyboards
{
    internal class CorsairK60ProLowProfileController : ILightingControl
    {
        private const int DEVICE_VID = 0x1B1C;
        private const int DEVICE_PID = 0x1BAD;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, 0, 65, 65);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    CorsairK60ProLowProfileDevice keyboard = new CorsairK60ProLowProfileDevice(stream);
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

    public class CorsairK60ProLowProfileDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 65;

        private bool _isUIControl = false;

        private const int LED_COUNT = 106;

        public CorsairK60ProLowProfileDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CorsairK60ProLowProfile keyboard = new CorsairK60ProLowProfile(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                SetCurrentLedEffectOff();
            }
            
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"Corsair K60 low profile Keyboard device count error");
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
                        ((HidStream)_deviceStream).Write(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on Corsair K60 low profile Keyboard");
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
                USBDeviceType = USBDevices.CorsairK60ProLowProfile,
                Name = "Corsair K60 Pro Low Profile"
            };
        }

        /// <summary>
        /// Turn firmware animation off
        /// </summary>
        public void SetCurrentLedEffectOff()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x08;
            commands[2] = 0x01;
            commands[3] = 0x03;
            commands[5] = 0x02;
            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch { }

            commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x08;
            commands[2] = 0x0D;
            commands[4] = 0x01;
            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch { }

            _isUIControl = true;
        }

        public override void TurnFwAnimationOn()
        {
            //Corsair will go back to firmware animation automatically
        }
    }

    public class CorsairK60ProLowProfile : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 7;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 22;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            //Command 1 - Row 1
            { Keyboard.ESC,      Tuple.Create(0, 0) },
            { Keyboard.F1,       Tuple.Create(0, 2) },
            { Keyboard.F2,       Tuple.Create(0, 3) },
            { Keyboard.F3,       Tuple.Create(0, 4) },
            { Keyboard.F4,       Tuple.Create(0, 5) },
            { Keyboard.F5,       Tuple.Create(0, 6) },
            { Keyboard.F6,       Tuple.Create(0, 7) },
            { Keyboard.F7,       Tuple.Create(0, 8) },
            { Keyboard.F8,       Tuple.Create(0, 9) },
            { Keyboard.F9,       Tuple.Create(0, 11) },
            { Keyboard.F10,      Tuple.Create(0, 12) },
            { Keyboard.F11,      Tuple.Create(0, 13) },
            { Keyboard.F12,      Tuple.Create(0, 14) },
            { Keyboard.PrnSrc,   Tuple.Create(0, 15) },
            { Keyboard.ScrLk,    Tuple.Create(0, 16) },
            { Keyboard.Pause,    Tuple.Create(0, 17) },
            //Command 2 - Row 2
            { Keyboard.Tilde,        Tuple.Create(1, 0)  },
            { Keyboard.One,          Tuple.Create(1, 1)  },
            { Keyboard.Two,          Tuple.Create(1, 2)  },
            { Keyboard.Three,        Tuple.Create(1, 3)  },
            { Keyboard.Four,         Tuple.Create(1, 4)  },
            { Keyboard.Five,         Tuple.Create(1, 5) },
            { Keyboard.Six,          Tuple.Create(1, 6) },
            { Keyboard.Seven,        Tuple.Create(1, 7) },
            { Keyboard.Eight,        Tuple.Create(1, 8) },
            { Keyboard.Nine,         Tuple.Create(1, 9) },
            { Keyboard.Zero,         Tuple.Create(1, 10) },
            { Keyboard.Hyphen,       Tuple.Create(1, 11) },
            { Keyboard.Plus,         Tuple.Create(1, 12) },
            { Keyboard.BackSpace,    Tuple.Create(1, 14) },
            { Keyboard.Insert,       Tuple.Create(1, 15) },
            { Keyboard.HOME,         Tuple.Create(1, 16) },
            { Keyboard.PAGEUp,       Tuple.Create(1, 17) },
            { Keyboard.NumLock,      Tuple.Create(1, 18) },
            { Keyboard.NumBackSlash, Tuple.Create(1, 19) },
            { Keyboard.NUM_Multiple, Tuple.Create(1, 20) },
            { Keyboard.NUM_Hypen,    Tuple.Create(1, 21) },
            //Command 3 - Row 3
            { Keyboard.Tab,          Tuple.Create(2, 0)  },
            { Keyboard.Q,            Tuple.Create(2, 2)  },
            { Keyboard.W,            Tuple.Create(2, 3)  },
            { Keyboard.E,            Tuple.Create(2, 4)  },
            { Keyboard.R,            Tuple.Create(2, 5) },
            { Keyboard.T,            Tuple.Create(2, 6) },
            { Keyboard.Y,            Tuple.Create(2, 7) },
            { Keyboard.U,            Tuple.Create(2, 8) },
            { Keyboard.I,            Tuple.Create(2, 9) },
            { Keyboard.O,            Tuple.Create(2, 10) },
            { Keyboard.P,            Tuple.Create(2, 11) },
            { Keyboard.LeftBracket,  Tuple.Create(2, 12) },
            { Keyboard.RightBracket, Tuple.Create(2, 13) },
            { Keyboard.Slash,        Tuple.Create(2, 14) },
            { Keyboard.DELETE,       Tuple.Create(2, 15) },
            { Keyboard.END,          Tuple.Create(2, 16) },
            { Keyboard.PAGEDown,     Tuple.Create(2, 17) },
            { Keyboard.NUM_7,        Tuple.Create(2, 18) },
            { Keyboard.NUM_8,        Tuple.Create(2, 19) },
            { Keyboard.NUM_9,        Tuple.Create(2, 20) },
            { Keyboard.NUM_Plus,     Tuple.Create(2, 21) },
            //Commnad 4 - Row 4
            { Keyboard.CapLock,      Tuple.Create(3, 0)  },
            { Keyboard.A,            Tuple.Create(3, 2)  },
            { Keyboard.S,            Tuple.Create(3, 3)  },
            { Keyboard.D,            Tuple.Create(3, 4) },
            { Keyboard.F,            Tuple.Create(3, 5) },
            { Keyboard.G,            Tuple.Create(3, 6) },
            { Keyboard.H,            Tuple.Create(3, 7) },
            { Keyboard.J,            Tuple.Create(3, 8) },
            { Keyboard.K,            Tuple.Create(3, 9) },
            { Keyboard.L,            Tuple.Create(3, 10) },
            { Keyboard.Semicolon,    Tuple.Create(3, 11) },
            { Keyboard.SingleQuote,  Tuple.Create(3, 12) },
            { Keyboard.Enter,        Tuple.Create(3, 14) },
            { Keyboard.NUM_4,        Tuple.Create(3, 18) },
            { Keyboard.NUM_5,        Tuple.Create(3, 19) },
            { Keyboard.NUM_6,        Tuple.Create(3, 20) },
            //Commnad 5 - Row 5
            { Keyboard.LeftShift,    Tuple.Create(4, 1)  },
            { Keyboard.Z,            Tuple.Create(4, 2)  },
            { Keyboard.X,            Tuple.Create(4, 3)  },
            { Keyboard.C,            Tuple.Create(4, 4) },
            { Keyboard.V,            Tuple.Create(4, 5) },
            { Keyboard.B,            Tuple.Create(4, 6) },
            { Keyboard.N,            Tuple.Create(4, 7) },
            { Keyboard.M,            Tuple.Create(4, 8) },
            { Keyboard.Comma,        Tuple.Create(4, 9) },
            { Keyboard.Period,       Tuple.Create(4, 10) },
            { Keyboard.BackSlash,    Tuple.Create(4, 11) },
            { Keyboard.RightShift,   Tuple.Create(4, 13) },
            { Keyboard.UP,           Tuple.Create(4, 16) },
            { Keyboard.NUM_1,        Tuple.Create(4, 18) },
            { Keyboard.NUM_2,        Tuple.Create(4, 19) },
            { Keyboard.NUM_3,        Tuple.Create(4, 20) },
            { Keyboard.NUM_Enter,    Tuple.Create(4, 21) },
            //Commnad 6 - Row 6
            { Keyboard.CTRL_L,       Tuple.Create(5, 0) },
            { Keyboard.WIN_L,        Tuple.Create(5, 1) },
            { Keyboard.ALT_L,        Tuple.Create(5, 2) },
            { Keyboard.SPACE,        Tuple.Create(5, 6)},
            { Keyboard.ALT_R,        Tuple.Create(5, 10)},
            { Keyboard.LOGO,         Tuple.Create(6, 11)},
            { Keyboard.FN,           Tuple.Create(5, 11)},
            { Keyboard.Menu,         Tuple.Create(5, 13)},
            { Keyboard.CTRL_R,       Tuple.Create(5, 14)},
            { Keyboard.LEFT,         Tuple.Create(5, 15)},
            { Keyboard.DOWN,         Tuple.Create(5, 16)},
            { Keyboard.RIGHT,        Tuple.Create(5, 17)},
            { Keyboard.NUM_0,        Tuple.Create(5, 19) },
            { Keyboard.NUM_DEL,      Tuple.Create(5, 20) },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {0, new List<Keyboard>(){   Keyboard.ESC,             Keyboard.F1,       Keyboard.F2,       Keyboard.F3,       Keyboard.F4,
                                        Keyboard.F5,       Keyboard.F6,       Keyboard.F7,       Keyboard.F8,       Keyboard.F9,
                                        Keyboard.F10,      Keyboard.F11,      Keyboard.F12,      Keyboard.PrnSrc,   Keyboard.ScrLk,
                                        Keyboard.Pause,

                                        Keyboard.Tilde,       Keyboard.One,         Keyboard.Two,           Keyboard.Three,             Keyboard.Four,
                                        Keyboard.Five,        Keyboard.Six,         Keyboard.Seven,         Keyboard.Eight,             Keyboard.Nine,              Keyboard.Zero,
                                        Keyboard.Hyphen,      Keyboard.Plus,        Keyboard.BackSpace,     Keyboard.Insert,            Keyboard.HOME,
                                        Keyboard.PAGEUp,      Keyboard.NumLock,     Keyboard.NumBackSlash,  Keyboard.NUM_Multiple,      Keyboard.NUM_Hypen,

                                        Keyboard.Tab,       Keyboard.Q,       Keyboard.W,       Keyboard.E,       Keyboard.R,
                                        Keyboard.T,       Keyboard.Y,       Keyboard.U,       Keyboard.I,       Keyboard.O,
                                        Keyboard.P,      Keyboard.LeftBracket,      Keyboard.RightBracket,      Keyboard.Slash,      Keyboard.DELETE,
                                        Keyboard.END,      Keyboard.PAGEDown,      Keyboard.NUM_7,      Keyboard.NUM_8,      Keyboard.NUM_9,
                                        Keyboard.NUM_Plus,

                                        Keyboard.CapLock,       Keyboard.A,                 Keyboard.S,       Keyboard.D,       Keyboard.F,
                                        Keyboard.G,             Keyboard.H,                 Keyboard.J,       Keyboard.K,       Keyboard.L,
                                        Keyboard.Semicolon,     Keyboard.SingleQuote,            Keyboard.Enter,
                                        Keyboard.NUM_4,   Keyboard.NUM_5,   Keyboard.NUM_6,

                                        Keyboard.LeftShift,             Keyboard.Z,       Keyboard.X,       Keyboard.C,
                                        Keyboard.V,       Keyboard.B,       Keyboard.N,       Keyboard.M,       Keyboard.Comma,
                                        Keyboard.Period,      Keyboard.BackSlash,            Keyboard.RightShift,
                                        Keyboard.UP,            Keyboard.NUM_1,      Keyboard.NUM_2,      Keyboard.NUM_3, Keyboard.NUM_Enter,

                                        Keyboard.CTRL_L,       Keyboard.WIN_L,       Keyboard.ALT_L,
                                        Keyboard.SPACE,           Keyboard.ALT_R,
                                        Keyboard.FN,      Keyboard.Menu,      Keyboard.CTRL_R,      Keyboard.LEFT,
                                        Keyboard.DOWN,      Keyboard.RIGHT,          Keyboard.NUM_0,     Keyboard.NUM_DEL,} }
        };

        private readonly List<int> LED_INDEX = new List<int>()
        {
                    37,   54, 55, 56, 57,     58, 59, 60, 61,     62, 63, 64, 65,     66, 67, 68,    //120, 123, 121, 122,
	        49,   26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 41, 42,     38,     69, 70, 71,    79, 80, 81, 82,
            39,   16, 22, 4, 17, 19, 24, 20, 8, 14, 15, 43,   44,   45,       72, 73, 74,    91, 92, 93, 83,
            53,   0, 18, 3, 5, 6, 7, 9, 10, 11, 47, 48,         36,                        88, 89, 90,
            102,  25, 23, 2, 21, 1, 13, 12, 50, 51, 52,          106,              78,        85, 86, 87, 84,
            101,  104, 103,           40,              107, 108, 97, 105,      76, 77, 75,     94,    95,

	        //ISO
	        96, 46
        };

        public CorsairK60ProLowProfile(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.CorsairK60ProLowProfile,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Corsair K60 Pro Low Profile"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
                new List<Keyboard>(){   Keyboard.ESC,      Keyboard.NA,       Keyboard.F1,       Keyboard.F2,       Keyboard.F3,
                                        Keyboard.F4,       Keyboard.F5,       Keyboard.F6,       Keyboard.F7,       Keyboard.F8,
                                        Keyboard.F9,       Keyboard.F10,      Keyboard.F11,      Keyboard.F12,      Keyboard.PrnSrc,
                                        Keyboard.ScrLk,    Keyboard.Pause,   },

                new List<Keyboard>(){   Keyboard.Tilde,       Keyboard.One,         Keyboard.Two,           Keyboard.Three,             Keyboard.Four,
                                        Keyboard.Five,        Keyboard.Six,         Keyboard.Seven,         Keyboard.Eight,             Keyboard.Nine,
                                        Keyboard.Zero,        Keyboard.Hyphen,      Keyboard.Plus,          Keyboard.BackSpace,         Keyboard.Insert,
                                        Keyboard.HOME,        Keyboard.PAGEUp,      Keyboard.NumLock,       Keyboard.NumBackSlash,      Keyboard.NUM_Multiple,
                                        Keyboard.NUM_Hypen,     },

                new List<Keyboard>(){   Keyboard.Tab,       Keyboard.Q,             Keyboard.W,                 Keyboard.E,         Keyboard.R,
                                        Keyboard.T,         Keyboard.Y,             Keyboard.U,                 Keyboard.I,         Keyboard.O,
                                        Keyboard.P,         Keyboard.LeftBracket,   Keyboard.RightBracket,      Keyboard.Slash,     Keyboard.DELETE,
                                        Keyboard.END,       Keyboard.PAGEDown,      Keyboard.NUM_7,             Keyboard.NUM_8,     Keyboard.NUM_9,
                                        Keyboard.NUM_Plus   },

                new List<Keyboard>(){   Keyboard.CapLock,       Keyboard.A,                 Keyboard.S,       Keyboard.D,       Keyboard.F,
                                        Keyboard.G,             Keyboard.H,                 Keyboard.J,       Keyboard.K,       Keyboard.L,
                                        Keyboard.Semicolon,     Keyboard.SingleQuote,       Keyboard.NA,      Keyboard.Enter,   Keyboard.NA,
                                        Keyboard.NA,            Keyboard.NA,                Keyboard.NUM_4,   Keyboard.NUM_5,   Keyboard.NUM_6  },

                new List<Keyboard>(){   Keyboard.LeftShift,       Keyboard.NA,              Keyboard.Z,         Keyboard.X,             Keyboard.C,
                                        Keyboard.V,               Keyboard.B,               Keyboard.N,         Keyboard.M,             Keyboard.Comma,
                                        Keyboard.Period,          Keyboard.BackSlash,      Keyboard.NA,         Keyboard.RightShift,    Keyboard.NA,
                                        Keyboard.UP,              Keyboard.NA,              Keyboard.NUM_1,     Keyboard.NUM_2,         Keyboard.NUM_3,
                                        Keyboard.NUM_Enter  },

                new List<Keyboard>(){   Keyboard.CTRL_L,       Keyboard.WIN_L,      Keyboard.ALT_L,     Keyboard.NA,        Keyboard.NA,
                                        Keyboard.SPACE,        Keyboard.NA,         Keyboard.NA,        Keyboard.NA,        Keyboard.ALT_R,
                                        Keyboard.LOGO,         Keyboard.FN,         Keyboard.Menu,      Keyboard.CTRL_R,    Keyboard.LEFT,
                                        Keyboard.DOWN,         Keyboard.RIGHT,      Keyboard.NA,        Keyboard.NUM_0,     Keyboard.NUM_DEL    }
            };
        }

        /// <summary>
        /// Process Color
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();
            int count = 0;

            byte[] result = new byte[455];
            byte[] red = new byte[120];
            byte[] blue = new byte[120];
            byte[] green = new byte[120];

            foreach (var ledList in COMMAND_LAYOUT[0])
            {
                var position = KEYS_LAYOUTS[ledList];
                var color = colorMatrix[position.Item1, position.Item2];

                red[LED_INDEX[count]] = color.R;
                blue[LED_INDEX[count]] = color.B;
                green[LED_INDEX[count]] = color.G;

                count++;
            }

            result[0x01] = 0x08;
            result[0x02] = 0x06;
            result[0x04] = 0x71;
            result[0x05] = 0x01;

            List<byte> reds = new List<byte>();
            reds.AddRange(red);
            reds.GetRange(0, 53).ToArray().CopyTo(result, 12);

            //Command 1
            result[65] = 0x00;
            result[66] = 0x08;
            result[67] = 0x07;
            result[68] = 0x00;

            reds.GetRange(53, 55).ToArray().CopyTo(result, 69);

            //Command 2
            result[65 * 2] = 0x00;
            result[65 * 2 + 1] = 8;
            result[65 * 2 + 2] = 7;
            result[65 * 2 + 3] = 0;

            reds.GetRange(108, 1).ToArray().CopyTo(result, 138);

            List<byte> greens = new List<byte>();
            greens.AddRange(green);
            greens.GetRange(0, 52).ToArray().CopyTo(result, 143);

            //Command 3
            result[195] = 0x00;
            result[196] = 8;
            result[197] = 7;
            result[198] = 0;

            greens.GetRange(52, 56).ToArray().CopyTo(result, 199);

            //Command 4
            result[260] = 0x00;
            result[261] = 8;
            result[262] = 7;
            result[263] = 0;

            greens.GetRange(108, 1).ToArray().CopyTo(result, 269);

            List<byte> blues = new List<byte>();
            blues.AddRange(blue);
            blues.GetRange(0, 51).ToArray().CopyTo(result, 274);

            //Command 5
            result[325] = 0x00;
            result[326] = 8;
            result[327] = 7;
            result[328] = 0;

            blues.GetRange(51, 57).ToArray().CopyTo(result, 329);

            //Command 6
            result[390] = 0x00;
            result[391] = 8;
            result[392] = 7;
            result[393] = 0;

            blues.GetRange(108, 1).ToArray().CopyTo(result, 400);
            _displayColorBytes.AddRange(result);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();
            int count = 0;
            byte[] result = new byte[455];
            byte[] red = new byte[120];
            byte[] blue = new byte[120];
            byte[] green = new byte[120];

            foreach (var ledList in COMMAND_LAYOUT[0])
            {
                var position = KEYS_LAYOUTS[ledList];

                red[LED_INDEX[count]] = 0;
                blue[LED_INDEX[count]] = 0;
                green[LED_INDEX[count]] = 0;

                count++;
            }

            result[0x01] = 0x08;
            result[0x02] = 0x06;
            result[0x04] = 0x71;
            result[0x05] = 0x01;

            List<byte> reds = new List<byte>();
            reds.AddRange(red);
            reds.GetRange(0, 53).ToArray().CopyTo(result, 12);

            //1
            result[65] = 0x00;
            result[66] = 0x08;
            result[67] = 0x07;
            result[68] = 0x00;

            reds.GetRange(53, 55).ToArray().CopyTo(result, 69);

            //2
            result[65 * 2] = 0x00;
            result[65 * 2 + 1] = 8;
            result[65 * 2 + 2] = 7;
            result[65 * 2 + 3] = 0;

            reds.GetRange(108, 1).ToArray().CopyTo(result, 138);

            List<byte> greens = new List<byte>();
            greens.AddRange(green);
            greens.GetRange(0, 52).ToArray().CopyTo(result, 143);

            //3
            result[195] = 0x00;
            result[196] = 8;
            result[197] = 7;
            result[198] = 0;

            greens.GetRange(52, 56).ToArray().CopyTo(result, 199);

            //4
            result[260] = 0x00;
            result[261] = 8;
            result[262] = 7;
            result[263] = 0;

            greens.GetRange(108, 1).ToArray().CopyTo(result, 269);

            List<byte> blues = new List<byte>();
            blues.AddRange(blue);
            blues.GetRange(0, 51).ToArray().CopyTo(result, 274);

            //5
            result[325] = 0x00;
            result[326] = 8;
            result[327] = 7;
            result[328] = 0;

            blues.GetRange(51, 57).ToArray().CopyTo(result, 329);

            //6
            result[390] = 0x00;
            result[391] = 8;
            result[392] = 7;
            result[393] = 0;

            blues.GetRange(108, 1).ToArray().CopyTo(result, 400);
            _displayColorBytes.AddRange(result);
        }
    }
}
