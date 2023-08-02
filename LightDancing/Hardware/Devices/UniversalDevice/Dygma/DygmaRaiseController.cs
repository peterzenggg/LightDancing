using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LightDancing.Hardware.Devices.UniversalDevice.Dygma
{
    internal class DygmaRaiseController : ILightingControl
    {
        private const string DEVICE_PID = "VID_1209&PID_2201";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<SerialStream> streams = serialDetector.GetSerialStreams(DEVICE_PID);

            List<USBDeviceBase> hardwares = null;

            if (streams != null && streams.Count > 0)
            {
                if (hardwares == null)
                {
                    hardwares = new List<USBDeviceBase>();
                }

                foreach (SerialStream stream in streams)
                {
                    stream.BaudRate = BAUDRATES;
                    DygmaRaiseDevice device = new DygmaRaiseDevice(stream);
                    hardwares.Add(device);
                }
            }

            return hardwares;
        }
    }

    public class DygmaRaiseDevice : USBDeviceBase
    {
        private const int LED_COUNT = 139;

        public DygmaRaiseDevice(SerialStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            DygmaRaise keyboard = new DygmaRaise(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"Dygma Raise Keyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                var result = _lightingBase[0].GetDisplayColors().ToArray();

                try
                {
                    _deviceStream.Write(result, 0, result.Length);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on Dygma Raise Keyboard");
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
                USBDeviceType = USBDevices.DygmaRaise,
                Name = "Dygma Raise"
            };
        }

        /// <summary>
        /// Turn firmawre animation off
        /// </summary>
        public void SetCurrentLedEffectOff()
        {
            //Dygma do not need to turn firmware animation off, might need to manually switch to single color mode first
        }

        public override void TurnFwAnimationOn()
        {
            //NA
        }
    }

    public class DygmaRaise : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 11;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 14;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 4 * 3 * 132 + 9;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            //0
            { Keyboard.ESC,          Tuple.Create(0, 0) },
            { Keyboard.One,          Tuple.Create(0, 1)  },
            { Keyboard.Two,          Tuple.Create(0, 2)  },
            { Keyboard.Three,        Tuple.Create(0, 3)  },
            { Keyboard.Four,         Tuple.Create(0, 3)  },
            { Keyboard.Five,         Tuple.Create(0, 5) },
            { Keyboard.Six,          Tuple.Create(0, 6) },
            //7
            { Keyboard.Tab,          Tuple.Create(1, 0)  },
            { Keyboard.Q,            Tuple.Create(1, 1)  },
            { Keyboard.W,            Tuple.Create(1, 2)  },
            { Keyboard.E,            Tuple.Create(1, 3)  },
            { Keyboard.R,            Tuple.Create(1, 3) },
            { Keyboard.T,            Tuple.Create(1, 5) },
            //13
            { Keyboard.CapLock,      Tuple.Create(2, 0)  },
            { Keyboard.A,            Tuple.Create(2, 1)  },
            { Keyboard.S,            Tuple.Create(2, 2)  },
            { Keyboard.D,            Tuple.Create(2, 3) },
            { Keyboard.F,            Tuple.Create(2, 3) },
            { Keyboard.G,            Tuple.Create(2, 5) },
            //19
            { Keyboard.LeftShift,    Tuple.Create(3, 0)  },
            { Keyboard.Z,            Tuple.Create(3, 1)  },
            { Keyboard.X,            Tuple.Create(3, 2)  },
            { Keyboard.C,            Tuple.Create(3, 3) },
            { Keyboard.V,            Tuple.Create(3, 3) },
            { Keyboard.B,            Tuple.Create(3, 5) },
            { Keyboard.N,            Tuple.Create(3, 6) },
            //26
            { Keyboard.CTRL_L,       Tuple.Create(3, 0) },
            { Keyboard.WIN_L,        Tuple.Create(3, 1) },
            { Keyboard.ALT_L,        Tuple.Create(3, 2) },
            { Keyboard.SPACE_LL,        Tuple.Create(3, 3)},
            { Keyboard.SPACE_L,        Tuple.Create(3, 5)},
            //31
            { Keyboard.Dygma_Space_L,        Tuple.Create(5, 3)},
            { Keyboard.Dygma_Space_LL,        Tuple.Create(5, 5)},
            //33
            { Keyboard.BackSpace,    Tuple.Create(0, 13) },
            { Keyboard.Plus,         Tuple.Create(0, 12) },
            { Keyboard.Hyphen,       Tuple.Create(0, 11) },
            { Keyboard.Zero,         Tuple.Create(0, 10) },
            { Keyboard.Nine,         Tuple.Create(0, 9) },
            { Keyboard.Eight,        Tuple.Create(0, 8) },
            { Keyboard.Seven,        Tuple.Create(0, 7) },
            //30
            { Keyboard.Slash,        Tuple.Create(1, 13) },
            { Keyboard.RightBracket, Tuple.Create(1, 12) },
            { Keyboard.LeftBracket,  Tuple.Create(1, 11) },
            { Keyboard.P,            Tuple.Create(1, 10) },
            { Keyboard.O,            Tuple.Create(1, 9) },
            { Keyboard.I,            Tuple.Create(1, 8) },
            { Keyboard.U,            Tuple.Create(1, 7) },
            { Keyboard.Y,            Tuple.Create(1, 6) },
            //38
            { Keyboard.Enter,        Tuple.Create(2, 12) },
            { Keyboard.SingleQuote,  Tuple.Create(2, 11) },
            { Keyboard.Semicolon,    Tuple.Create(2, 10) },
            { Keyboard.L,            Tuple.Create(2, 9) },
            { Keyboard.K,            Tuple.Create(2, 8) },
            { Keyboard.J,            Tuple.Create(2, 7) },
            { Keyboard.H,            Tuple.Create(2, 6) },
            //55
            { Keyboard.RightShift,   Tuple.Create(3, 12) },
            { Keyboard.BackSlash,    Tuple.Create(3, 11) },
            { Keyboard.Period,       Tuple.Create(3, 10) },
            { Keyboard.Comma,        Tuple.Create(3, 9) },
            { Keyboard.M,            Tuple.Create(3, 8) },
            { Keyboard.NA,           Tuple.Create(3, 7) },
            //61
            { Keyboard.CTRL_R,       Tuple.Create(4, 13)},
            { Keyboard.Dygma_Win,    Tuple.Create(4, 12)},
            { Keyboard.FN,           Tuple.Create(4, 11)},
            { Keyboard.ALT_R,        Tuple.Create(4, 10)},
            { Keyboard.SPACE_RR,     Tuple.Create(4, 8)},
            { Keyboard.SPACE_R,      Tuple.Create(4, 7)},
            //67
            { Keyboard.Dygma_Space_RR,       Tuple.Create(5, 8) },
            { Keyboard.Dygma_Space_R,        Tuple.Create(5, 7) },
            //Surround
            //0
            { Keyboard.LED0,       Tuple.Create(2, 0) },
            { Keyboard.LED1,       Tuple.Create(1, 0) },
            { Keyboard.LED2,       Tuple.Create(0, 0) },
            { Keyboard.LED3,       Tuple.Create(0, 1) },
            { Keyboard.LED4,       Tuple.Create(0, 2) },
            { Keyboard.LED5,       Tuple.Create(0, 3) },
            { Keyboard.LED6,       Tuple.Create(0, 4) },
            { Keyboard.LED7,       Tuple.Create(0, 5) },
            { Keyboard.LED8,       Tuple.Create(1, 5) },
            { Keyboard.LED9,       Tuple.Create(2, 5) },
            { Keyboard.LED10,       Tuple.Create(3, 5) },
            { Keyboard.LED11,       Tuple.Create(4, 5) },
            { Keyboard.LED12,       Tuple.Create(5, 5) },
            { Keyboard.LED13,       Tuple.Create(6, 5) },
            { Keyboard.LED14,       Tuple.Create(7, 5) },
            { Keyboard.LED15,       Tuple.Create(8, 5) },
            { Keyboard.LED16,       Tuple.Create(9, 5) },
            { Keyboard.LED17,       Tuple.Create(10, 5) },
            { Keyboard.LED18,       Tuple.Create(10, 4) },
            { Keyboard.LED19,       Tuple.Create(10, 3) },
            { Keyboard.LED20,       Tuple.Create(10, 2) },
            { Keyboard.LED21,       Tuple.Create(10, 1) },
            { Keyboard.LED22,       Tuple.Create(10, 0) },
            { Keyboard.LED23,       Tuple.Create(9, 0) },
            { Keyboard.LED24,       Tuple.Create(8, 0) },
            { Keyboard.LED25,       Tuple.Create(7, 0) },
            { Keyboard.LED26,       Tuple.Create(6, 0) },
            { Keyboard.LED27,       Tuple.Create(5, 0) },
            { Keyboard.LED28,       Tuple.Create(4, 0) },
            { Keyboard.LED29,       Tuple.Create(2, 0) },
            { Keyboard.LED30,       Tuple.Create(1, 0) },
            { Keyboard.LED31,       Tuple.Create(0, 0) },
            { Keyboard.LED32,       Tuple.Create(0, 1) },
            { Keyboard.LED33,       Tuple.Create(0, 2) },
            { Keyboard.LED34,       Tuple.Create(0, 3) },
            { Keyboard.LED35,       Tuple.Create(0, 4) },
            { Keyboard.LED36,       Tuple.Create(0, 5) },
            { Keyboard.LED37,       Tuple.Create(0, 6) },
            { Keyboard.LED38,       Tuple.Create(0, 7) },
            { Keyboard.LED39,       Tuple.Create(1, 7) },
            { Keyboard.LED40,       Tuple.Create(2, 7) },
            { Keyboard.LED41,       Tuple.Create(3, 7) },
            { Keyboard.LED42,       Tuple.Create(4, 7) },
            { Keyboard.LED43,       Tuple.Create(5, 7) },
            { Keyboard.LED44,       Tuple.Create(6, 7) },
            { Keyboard.LED45,       Tuple.Create(7, 7) },
            { Keyboard.LED46,       Tuple.Create(8, 7) },
            { Keyboard.LED47,       Tuple.Create(9, 7) },
            { Keyboard.LED48,       Tuple.Create(10, 7) },
            { Keyboard.LED49,       Tuple.Create(10, 8) },
            { Keyboard.LED50,       Tuple.Create(10, 9) },
            { Keyboard.LED51,       Tuple.Create(10, 10) },
            { Keyboard.LED52,       Tuple.Create(10, 11) },
            { Keyboard.LED53,       Tuple.Create(10, 12) },
            { Keyboard.LED54,       Tuple.Create(10, 13) },
            { Keyboard.LED55,       Tuple.Create(9, 13) },
            { Keyboard.LED56,       Tuple.Create(8, 13) },
            { Keyboard.LED57,       Tuple.Create(7, 13) },
            { Keyboard.LED58,       Tuple.Create(6, 13) },
            { Keyboard.LED59,       Tuple.Create(5, 13) },
            { Keyboard.LED60,       Tuple.Create(4, 13) },
            { Keyboard.LED61,       Tuple.Create(3, 13) },
            { Keyboard.LOGO,       Tuple.Create(3, 13) },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
        };

        public DygmaRaise(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.DygmaRaise,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Dygma Raise"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
                new List<Keyboard>(){   Keyboard.ESC,      Keyboard.NA,       Keyboard.F1,       Keyboard.F2,       Keyboard.F3,
                                        Keyboard.F3,       Keyboard.F5,       Keyboard.F6,       Keyboard.F7,       Keyboard.F8,
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
                                        Keyboard.NA,            Keyboard.NA,                Keyboard.NUM_3,   Keyboard.NUM_5,   Keyboard.NUM_6  },

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
        /// Dygma's commands are in form of string
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();

            List<char> result = new List<char>();

            result.AddRange("led.theme");

            foreach (var led in KEYS_LAYOUTS)
            {
                var position = led.Value;
                var color = colorMatrix[position.Item1, position.Item2];

                result.Add(' ');
                result.AddRange(color.R.ToString());
                result.Add(' ');
                result.AddRange(color.G.ToString());
                result.Add(' ');
                result.AddRange(color.B.ToString());
            }

            result.Add('\n');

            var bytes = Encoding.GetEncoding("UTF-8").GetBytes(result.ToArray());

            _displayColorBytes.AddRange(bytes);
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

            List<char> result = new List<char>();

            result.AddRange("led.theme");

            foreach (var led in KEYS_LAYOUTS)
            {
                result.Add(' ');
                result.AddRange(0.ToString());
                result.Add(' ');
                result.AddRange(0.ToString());
                result.Add(' ');
                result.AddRange(0.ToString());
            }

            result.Add('\n');

            var bytes = Encoding.GetEncoding("UTF-8").GetBytes(result.ToArray());

            _displayColorBytes.AddRange(bytes);
        }
    }
}
