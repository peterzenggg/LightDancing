using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LightDancing.Hardware.Devices.UniversalDevice.Corsair.Keyboards
{
    internal class CorsairK100RGBKeyboardController : ILightingControl
    {
        private const int DEVICE_VID = 0x1B1C;
        private const int DEVICE_PID = 0x1B7C; //Corsiar K100 has 3 different PID
        private const int DEVICE_PID_2 = 0x1B7D;
        private const int DEVICE_PID_3 = 0x1BC5;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, 0, 1025, 1025);
            List<HidStream> streams_2 = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID_2, 0, 1025, 1025);
            List<HidStream> streams_3 = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID_3, 0, 1025, 1025);

            List<USBDeviceBase> hardwares = new List<USBDeviceBase>();

            if (streams != null && streams.Count > 0)
            {
                foreach (HidStream stream in streams)
                {
                    CorsairK100RGBKeyboardDevice keyboard = new CorsairK100RGBKeyboardDevice(stream);
                    hardwares.Add(keyboard);
                }
            }

            if (streams_2 != null && streams_2.Count > 0)
            {
                foreach (HidStream stream in streams_2)
                {
                    CorsairK100RGBKeyboardDevice keyboard = new CorsairK100RGBKeyboardDevice(stream);
                    hardwares.Add(keyboard);
                }
            }

            if (streams_3 != null && streams_3.Count > 0)
            {
                foreach (HidStream stream in streams_3)
                {
                    CorsairK100RGBKeyboardDevice keyboard = new CorsairK100RGBKeyboardDevice(stream);
                    hardwares.Add(keyboard);
                }
            }

            return hardwares;
        }
    }

    public class CorsairK100RGBKeyboardDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 1025;

        private bool _isUIControl = false;

        public CorsairK100RGBKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        { }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CorsairK100RGBKeyboard keyboard = new CorsairK100RGBKeyboard(_model);
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
                Trace.WriteLine($"Corsair K100 RGB Keyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                //Streaming Command Header
                int ledCounts = displayColors.Count;
                byte[] result = new byte[1025];
                result[0x01] = 0x08;
                result[0x02] = 0x06;
                result[0x03] = 0x01;
                result[0x04] = 0x45;
                result[0x05] = 0x02;
                result[0x08] = 0x12;

                displayColors.GetRange(0, ledCounts).ToArray().CopyTo(result, 22);
                try
                {
                    ((HidStream)_deviceStream).Write(result);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on Corsair K100 RGB Keyboard");
                }

                Thread.Sleep(10);

                try
                {
                    ((HidStream)_deviceStream).Read(result);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on Corsair K100 RGB Keyboard");
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
                USBDeviceType = USBDevices.CorsairK100RGBKeyboard,
                Name = "Corsair K100 RGB Keyboard"
            };
        }

        /// <summary>
        /// Set Firmware Animation/Control Off
        /// </summary>
        public void SetCurrentLedEffectOff()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x00;
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
            commands[0] = 0x00;
            commands[1] = 0x08;
            commands[2] = 0x01;
            commands[3] = 0x4A;
            commands[5] = 0x01;
            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch { }

            commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x00;
            commands[1] = 0x08;
            commands[2] = 0x01;
            commands[3] = 0x45;
            commands[4] = 0x00;
            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch { }

            commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x00;
            commands[1] = 0x08;
            commands[2] = 0x0D;
            commands[3] = 0x01;
            commands[4] = 0x22;
            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch { }

            _isUIControl = true;
        }

        public override void TurnFwAnimationOn()
        {
            //Corsair will go back to firmware mode after not sending command.
        }
    }

    public class CorsairK100RGBKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 12;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 28;

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
            { Keyboard.Tilde,        Tuple.Create(1, 0) },
            { Keyboard.One,          Tuple.Create(1, 1) },
            { Keyboard.Two,          Tuple.Create(1, 2) },
            { Keyboard.Three,        Tuple.Create(1, 3) },
            { Keyboard.Four,         Tuple.Create(1, 4) },
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
            { Keyboard.Tab,          Tuple.Create(2, 0) },
            { Keyboard.Q,            Tuple.Create(2, 2) },
            { Keyboard.W,            Tuple.Create(2, 3) },
            { Keyboard.E,            Tuple.Create(2, 4) },
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
            { Keyboard.CapLock,      Tuple.Create(3, 0) },
            { Keyboard.A,            Tuple.Create(3, 2) },
            { Keyboard.S,            Tuple.Create(3, 3) },
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
            { Keyboard.SPACE,        Tuple.Create(5, 6) },
            { Keyboard.ALT_R,        Tuple.Create(5, 10) },
            { Keyboard.LOGO,         Tuple.Create(6, 11) },
            { Keyboard.FN,           Tuple.Create(5, 11) },
            { Keyboard.Menu,         Tuple.Create(5, 13) },
            { Keyboard.CTRL_R,       Tuple.Create(5, 14) },
            { Keyboard.LEFT,         Tuple.Create(5, 15) },
            { Keyboard.DOWN,         Tuple.Create(5, 16) },
            { Keyboard.RIGHT,        Tuple.Create(5, 17) },
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
                                        Keyboard.DOWN,      Keyboard.RIGHT,          Keyboard.NUM_0,     Keyboard.NUM_DEL,

                                        Keyboard.TopLED1, Keyboard.TopLED2, Keyboard.TopLED3, Keyboard.TopLED4, Keyboard.TopLED5,
                                        Keyboard.TopLED6, Keyboard.TopLED7, Keyboard.TopLED8, Keyboard.TopLED9, Keyboard.TopLED10,
                                        Keyboard.TopLED11, Keyboard.TopLED12, Keyboard.TopLED13, Keyboard.TopLED14, Keyboard.TopLED15,
                                        Keyboard.TopLED16, Keyboard.TopLED17,Keyboard.TopLED18, Keyboard.TopLED19, Keyboard.TopLED20,
                                        Keyboard.TopLED21, Keyboard.TopLED22,

                                        Keyboard.LeftLED1, Keyboard.LeftLED2, Keyboard.LeftLED3, Keyboard.LeftLED4, Keyboard.LeftLED5,
                                        Keyboard.LeftLED6, Keyboard.LeftLED7, Keyboard.LeftLED8, Keyboard.LeftLED9, Keyboard.LeftLED10,
                                        Keyboard.LeftLED11,

                                        Keyboard.RightLED1, Keyboard.RightLED2, Keyboard.RightLED3, Keyboard.RightLED4, Keyboard.RightLED5,
                                        Keyboard.RightLED6, Keyboard.RightLED7, Keyboard.RightLED8, Keyboard.RightLED9, Keyboard.RightLED10,
                                        Keyboard.RightLED11,

                                        Keyboard.MediaLED1, Keyboard.MediaLED2, Keyboard.MediaLED3, Keyboard.MediaLED4, Keyboard.MediaLED5,

            } }
        };

        private readonly List<int> LED_INDEX = new List<int>()
        {
                    134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155,
            156,                      185, 178, 179,                                                                                167,
            157,                  124, 184, 133, 180, 110,              186, 187, 188, 98,                                              168,
            158,                      183, 182, 181,                                                                                169,
            159, 127,        37,   54, 55, 56, 57,     58, 59, 60, 61,     62, 63, 64, 65,  66, 67, 68,      119, 122, 120, 121,  170,
            160, 128,     49,   26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 41, 42,     38,     69, 70, 71,      79, 80, 81, 82,      171,
            161,                                                                                                                  172,
            162, 129,     39,   16, 22, 4, 17, 19, 24, 20, 8, 14, 15, 43,   44,   45,       72, 73, 74,      91, 92, 93, 83,      173,
            163, 130,     53,   0, 18, 3, 5, 6, 7, 9, 10, 11, 47, 48,         36,                            88, 89, 90,          174,
            164, 131,     102,  25, 23, 2, 21, 1, 13, 12, 50, 51, 52,          106,             78,          85, 86, 87, 84,      175,
            165, 132,     101,  104, 103,           40,              107, 118, 97, 105,      76, 77, 75,        94,    95,          176,
            166,                                                                                                                  177,
            96, 46 //ISO
        };

        private readonly int[,] LED_POSITION = new int[,]
        {
            {0, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0}, {7, 0}, {8, 0}, {9, 0}, {11, 0}, {12, 0}, {13, 0}, {14, 0}, {16, 0}, {17, 0}, {18, 0}, {19, 0}, {20, 0}, {21, 0}, {23, 0}, {24, 0}, {25, 0},
            {0, 1}, {3, 1}, {4, 1}, {5, 1}, {25, 1}, 
            {0, 2}, {2, 2}, {3, 2}, {4, 2}, {5, 2}, {6, 2}, {11, 2}, {12, 2}, {13, 2}, {21, 2}, {25, 2}, 
            {0, 3}, {3, 3}, {4, 3}, {5, 3}, {25, 3},
            {0, 4}, {2, 4}, {3, 4}, {5, 4}, {6, 4}, {7, 4}, {8, 4}, {9, 4}, {10, 4}, {11, 4}, {12, 4}, {14, 4}, {15, 4},
            {16, 4}, {17, 4}, {18, 4}, {19, 4}, {20, 4}, {21, 4}, {22, 4}, {23, 4}, {24, 4}, {25, 4},
            {0, 5}, {2, 5}, {3, 5}, {4, 5}, {5, 5}, {6, 5}, {7, 5}, {8, 5}, {9, 5}, {10, 5}, {11, 5}, {12, 5}, {13, 5}, {14, 5}, {15, 5}, {17, 5}, {18, 5}, {19, 5}, {20, 5}, {21, 5}, {22, 5}, {23, 5}, {24, 5}, {25, 5},
            {0, 6}, {25, 6},
            {0, 7}, {2, 7}, {3, 7}, {4, 7}, {5, 7}, {6, 7}, {7, 7}, {8, 7}, {9, 7}, {10, 7}, {11, 7}, {12, 7}, {13, 7}, {14, 7}, {16, 7}, {17, 7}, {18, 7}, {19, 7}, {20, 7}, {21, 7}, {22, 7}, {23, 7}, {24, 7}, {25, 7},
            {0, 8}, {2, 8}, {3, 8}, {4, 8}, {5, 8}, {6, 8}, {7, 8}, {8, 8}, {9, 8}, {10, 8}, {11, 8}, {12, 8}, {13, 8}, {14, 8}, {16, 8}, {21, 8}, {22, 8}, {23, 8}, {25, 8},
            {0, 9}, {2, 9}, {3, 9}, {4, 9}, {5, 9},
            {6, 9}, {7, 9}, {8, 9}, {9, 9}, {10, 9}, {11, 9}, {12, 9}, {13, 9}, {16, 9}, {19, 9}, {21, 9}, {22, 9}, {23, 9}, {24, 9}, {25, 9},
            {0, 10}, {2, 10}, {3, 10}, {4, 10}, {5, 10},
            {9, 10}, {12, 10}, {13, 10}, {15, 10}, {17, 10}, {19, 10}, {20, 10}, {21, 10}, {22, 10}, {23, 10}, {25, 10},
            {0, 11}, {25, 11},
            //ISO
            {2, 5},
            {13, 4}
        };

        public CorsairK100RGBKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        { }

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
                Type = LightingDevices.CorsairK100RGBKeyboard,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Corsair K100 RGB Keyboard"
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
            byte[] result = new byte[579];

            foreach (var ledList in LED_INDEX)
            {
                var color = colorMatrix[LED_POSITION[count, 1], LED_POSITION[count, 0]];
                result[(ledList) * 3] = color.R;
                result[(ledList) * 3 + 1] = color.G;
                result[(ledList) * 3 + 2] = color.B;

                count++;
            }
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
            byte[] result = new byte[579];

            foreach (var ledList in LED_INDEX)
            {
                result[(ledList) * 3] = 0;
                result[(ledList) * 3 + 1] = 0;
                result[(ledList) * 3 + 2] = 0;
            }
            _displayColorBytes.AddRange(result);
        }
    }
}
