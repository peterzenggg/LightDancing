using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.WhirlwindFx.Keyboards
{
    internal class WhirlwindFxElementKeyboardController : ILightingControl
    {
        private const string DEVICE_PID = "VID_0483&PID_A33E";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<SerialStream> streams = serialDetector.GetSerialStreams(DEVICE_PID);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (SerialStream stream in streams)
                {
                    stream.BaudRate = BAUDRATES;
                    WhirlwindFxElementKeyboardDevice keyboard = new WhirlwindFxElementKeyboardDevice(stream);
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

    public class WhirlwindFxElementKeyboardDevice : USBDeviceBase
    {
        private const int LED_COUNT = 104;

        public WhirlwindFxElementKeyboardDevice(SerialStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            WhirlwindFxElementKeyboard keyboard = new WhirlwindFxElementKeyboard(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"WhirlwindFx Element Keyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }
                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                PreStreamingCommand();
                try
                {
                    _deviceStream.Write(displayColors.ToArray());
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on WhirlwindFx Element");
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
                USBDeviceType = USBDevices.WhirlwindFxElement,
                Name = "Whirlwind Fx Element"
            };
        }

        public void PreStreamingCommand()
        {
            byte[] commands = new byte[8];
            commands[0] = 0x5b;
            commands[1] = 0x56;
            commands[2] = 0x02;
            commands[3] = 0x00;
            commands[4] = 0x47;
            commands[5] = 0x00;
            commands[6] = 0x47;
            commands[7] = 0x00;

            try
            {
                _deviceStream.Write(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on WhirlwindFx Element");
            }
        }

        public override void TurnFwAnimationOn()
        {
            //NA
        }
    }

    public class WhirlwindFxElementKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 22;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 319;

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
            { Keyboard.NUM_Plus,     Tuple.Create(2, 21) },
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
            {0, new List<Keyboard>(){   Keyboard.ESC,      Keyboard.NA,       Keyboard.F1,       Keyboard.F2,       Keyboard.F3,       Keyboard.F4,
                                        Keyboard.F5,       Keyboard.F6,       Keyboard.F7,       Keyboard.F8,       Keyboard.F9,
                                        Keyboard.F10,      Keyboard.F11,      Keyboard.F12,      Keyboard.PrnSrc,   Keyboard.ScrLk,
                                        Keyboard.Pause,   }},

            {1, new List<Keyboard>(){   Keyboard.Tilde,       Keyboard.One,         Keyboard.Two,           Keyboard.Three,             Keyboard.Four,
                                        Keyboard.Five,        Keyboard.Six,         Keyboard.Seven,         Keyboard.Eight,             Keyboard.Nine,              Keyboard.Zero,
                                        Keyboard.Hyphen,      Keyboard.Plus,        Keyboard.BackSpace,     Keyboard.Insert,            Keyboard.HOME,
                                        Keyboard.PAGEUp,      Keyboard.NumLock,     Keyboard.NumBackSlash,  Keyboard.NUM_Multiple,      Keyboard.NUM_Hypen,     }},

            {2, new List<Keyboard>(){   Keyboard.Tab,       Keyboard.Q,       Keyboard.W,       Keyboard.E,       Keyboard.R,
                                        Keyboard.T,       Keyboard.Y,       Keyboard.U,       Keyboard.I,       Keyboard.O,
                                        Keyboard.P,      Keyboard.LeftBracket,      Keyboard.RightBracket,      Keyboard.Slash,      Keyboard.DELETE,
                                        Keyboard.END,      Keyboard.PAGEDown,      Keyboard.NUM_7,      Keyboard.NUM_8,      Keyboard.NUM_9,
                                        Keyboard.NUM_Plus}},

            {3, new List<Keyboard>(){   Keyboard.CapLock,       Keyboard.A,                 Keyboard.S,       Keyboard.D,       Keyboard.F,
                                        Keyboard.G,             Keyboard.H,                 Keyboard.J,       Keyboard.K,       Keyboard.L,
                                        Keyboard.Semicolon,     Keyboard.SingleQuote,       Keyboard.NA,      Keyboard.Enter,   Keyboard.NA,
                                        Keyboard.NA,            Keyboard.NA,                Keyboard.NUM_4,   Keyboard.NUM_5,   Keyboard.NUM_6,}},

            {4, new List<Keyboard>(){   Keyboard.LeftShift,       Keyboard.NA,       Keyboard.Z,       Keyboard.X,       Keyboard.C,
                                        Keyboard.V,       Keyboard.B,       Keyboard.N,       Keyboard.M,       Keyboard.Comma,
                                        Keyboard.Period,      Keyboard.BackSlash,      Keyboard.NA,      Keyboard.RightShift,      Keyboard.NA,
                                        Keyboard.UP,      Keyboard.NA,      Keyboard.NUM_1,      Keyboard.NUM_2,      Keyboard.NUM_3, Keyboard.NUM_Enter}},

            {5, new List<Keyboard>(){   Keyboard.CTRL_L,       Keyboard.WIN_L,       Keyboard.ALT_L,       Keyboard.NA,       Keyboard.NA,
                                        Keyboard.SPACE,       Keyboard.NA,       Keyboard.NA,       Keyboard.NA,       Keyboard.ALT_R,
                                        Keyboard.LOGO,      Keyboard.FN,      Keyboard.Menu,      Keyboard.CTRL_R,      Keyboard.LEFT,
                                        Keyboard.DOWN,      Keyboard.RIGHT,      Keyboard.NA,      Keyboard.NUM_0,      Keyboard.NUM_DEL}}
        };

        public WhirlwindFxElementKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.WhirlwindFxElement,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Whirlwind Fx Element"
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
        /// Each command has 33 bytes, and total is 3 commands
        /// Each commnad should contain 9 keys (or less)
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();

            byte[] result = new byte[MAX_REPORT_LENGTH];
            result[0] = 0x5b;
            result[1] = 0x56;
            result[2] = 0x39;
            result[3] = 0x01;
            result[4] = 0x4c;

            int count = 0;
            foreach (var led in KEYS_LAYOUTS)
            {
                if (led.Key != Keyboard.NA)
                {
                    var position = led.Value;
                    var color = colorMatrix[position.Item1, position.Item2];
                    result[count * 3 + 5] = color.R;
                    result[count * 3 + 6] = color.G;
                    result[count * 3 + 7] = color.B;
                }

                count++;
            }

            result[317] = CalculateByte(result)[1];
            result[318] = CalculateByte(result)[0];
            _displayColorBytes.AddRange(result);
        }

        private byte[] CalculateByte(byte[] result)
        {
            int num = 0;

            for (int i = 4; i < 317; i++)
            {
                num += result[i];
            }

            byte[] resultArray = new byte[2];

            resultArray[0] = (byte)(num / 256);
            resultArray[1] = (byte)(num % (256 * 256));

            return resultArray;
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

            byte[] result = new byte[MAX_REPORT_LENGTH];
            result[0] = 0x5b;
            result[1] = 0x56;
            result[2] = 0x39;
            result[3] = 0x01;
            result[4] = 0x4c;

            int count = 0;
            foreach (var led in KEYS_LAYOUTS)
            {
                if (led.Key != Keyboard.NA)
                {
                    result[count * 3 + 5] = 0;
                    result[count * 3 + 6] = 0;
                    result[count * 3 + 7] = 0;
                }

                count++;
            }

            result[317] = CalculateByte(result)[1];
            result[318] = CalculateByte(result)[0];
            _displayColorBytes.AddRange(result);
        }
    }
}
