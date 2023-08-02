using System;
using System.Collections.Generic;
using System.Text;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Threading;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class DexinKeebController : ILightingControl
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
                    DexinKeyboardDevice keyboard = new DexinKeyboardDevice(stream);
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

    public class DexinKeyboardDevice : USBDeviceBase
    {
        public DexinKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            DexinKeyboard keyboard = new DexinKeyboard(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"DexinKeyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                for (int i = 0; i < displayColors.Count / 65; i++)
                {
                    byte[] result = displayColors.GetRange(65 * i, 65).ToArray();

                    if (i % 2 == 0) /*Set Feature First*/
                    {
                        try
                        {
                            ((HidStream)_deviceStream).SetFeature(result);
                        }
                        catch
                        {
                            Trace.WriteLine($"False to streaming on DexinKeyboard");
                        }
                    }
                    else /*Write*/
                    {
                        try
                        {
                            _deviceStream.Write(result);
                        }
                        catch
                        {
                            Trace.WriteLine($"False to streaming on DexinKeyboard");
                        }
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
                //Type = LightingDevices.DexinKeyboard,
                Name = "Dexin Keeb",
            };
        }
    }

    public class DexinKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 14;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 27;

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
            { Keyboard.TopLED1,      Tuple.Create(0, 1)  },
            { Keyboard.TopLED2,      Tuple.Create(0, 2)  },
            { Keyboard.TopLED3,      Tuple.Create(0, 3)  },
            { Keyboard.TopLED4,      Tuple.Create(0, 4)  },
            { Keyboard.TopLED5,      Tuple.Create(0, 5)  },
            { Keyboard.TopLED6,      Tuple.Create(0, 6)  },
            { Keyboard.TopLED7,      Tuple.Create(0, 7)  },
            { Keyboard.TopLED8,      Tuple.Create(0, 8)  },
            { Keyboard.TopLED9,      Tuple.Create(0, 9)  },
            { Keyboard.TopLED10,     Tuple.Create(0, 10) },
            { Keyboard.TopLED11,     Tuple.Create(0, 11) },
            { Keyboard.TopLED12,     Tuple.Create(0, 12) },
            { Keyboard.TopLED13,     Tuple.Create(0, 13) },
            { Keyboard.TopLED14,     Tuple.Create(0, 14) },
            { Keyboard.TopLED15,     Tuple.Create(0, 15) },
            { Keyboard.TopLED16,     Tuple.Create(0, 16) },
            { Keyboard.TopLED17,     Tuple.Create(0, 17) },
            { Keyboard.TopLED18,     Tuple.Create(0, 18) },
            { Keyboard.TopLED19,     Tuple.Create(0, 19) },
            { Keyboard.TopLED20,     Tuple.Create(0, 20) },
            { Keyboard.TopLED21,     Tuple.Create(0, 21) },
            { Keyboard.TopLED22,     Tuple.Create(0, 22) },
            { Keyboard.TopLED23,     Tuple.Create(0, 23) },
            { Keyboard.TopLED24,     Tuple.Create(0, 24) },
            { Keyboard.TopLED25,     Tuple.Create(0, 25) },
            //Row 2
            { Keyboard.LeftLED1,     Tuple.Create(1, 0)  },
            { Keyboard.MediaLED1,    Tuple.Create(1, 17) },
            { Keyboard.MediaLED2,    Tuple.Create(1, 19) },
            { Keyboard.MediaLED3,    Tuple.Create(1, 21) },
            { Keyboard.MediaLED4,    Tuple.Create(1, 23) },
            { Keyboard.MediaLED5,    Tuple.Create(1, 25) },
            { Keyboard.RightLED1,    Tuple.Create(1, 26) },
            //Row 4
            { Keyboard.LeftLED2,     Tuple.Create(3, 0)  },
            { Keyboard.ESC,          Tuple.Create(3, 2)  },
            { Keyboard.One,          Tuple.Create(3, 4)  },
            { Keyboard.Two,          Tuple.Create(3, 6)  },
            { Keyboard.Three,        Tuple.Create(3, 8)  },
            { Keyboard.Four,         Tuple.Create(3, 9)  },
            { Keyboard.Five,         Tuple.Create(3, 10) },
            { Keyboard.Six,          Tuple.Create(3, 11) },
            { Keyboard.Seven,        Tuple.Create(3, 12) },
            { Keyboard.Eight,        Tuple.Create(3, 13) },
            { Keyboard.Nine,         Tuple.Create(3, 14) },
            { Keyboard.Zero,         Tuple.Create(3, 15) },
            { Keyboard.Hyphen,       Tuple.Create(3, 17) },
            { Keyboard.Plus,         Tuple.Create(3, 19) },
            { Keyboard.BackSpace,    Tuple.Create(3, 21) },
            { Keyboard.DELETE,       Tuple.Create(3, 24) },
            { Keyboard.RightLED2,    Tuple.Create(3, 26) },
            //Row 6
            { Keyboard.LeftLED3,     Tuple.Create(5, 0)  },
            { Keyboard.Tab,          Tuple.Create(5, 3)  },
            { Keyboard.Q,            Tuple.Create(5, 5)  },
            { Keyboard.W,            Tuple.Create(5, 7)  },
            { Keyboard.E,            Tuple.Create(5, 9)  },
            { Keyboard.R,            Tuple.Create(5, 10) },
            { Keyboard.T,            Tuple.Create(5, 11) },
            { Keyboard.Y,            Tuple.Create(5, 12) },
            { Keyboard.U,            Tuple.Create(5, 13) },
            { Keyboard.I,            Tuple.Create(5, 14) },
            { Keyboard.O,            Tuple.Create(5, 15) },
            { Keyboard.P,            Tuple.Create(5, 16) },
            { Keyboard.LeftBracket,  Tuple.Create(5, 18) },
            { Keyboard.RightBracket, Tuple.Create(5, 20) },
            { Keyboard.BackSlash,    Tuple.Create(5, 22) },
            { Keyboard.HOME,         Tuple.Create(5, 24) },
            { Keyboard.RightLED3,    Tuple.Create(5, 26) },
            //Row 8
            { Keyboard.LeftLED4,     Tuple.Create(7, 0)  },
            { Keyboard.CapLock,      Tuple.Create(7, 4)  },
            { Keyboard.A,            Tuple.Create(7, 6)  },
            { Keyboard.S,            Tuple.Create(7, 8)  },
            { Keyboard.D,            Tuple.Create(7, 10) },
            { Keyboard.F,            Tuple.Create(7, 11) },
            { Keyboard.G,            Tuple.Create(7, 12) },
            { Keyboard.H,            Tuple.Create(7, 13) },
            { Keyboard.J,            Tuple.Create(7, 14) },
            { Keyboard.K,            Tuple.Create(7, 15) },
            { Keyboard.L,            Tuple.Create(7, 16) },
            { Keyboard.Semicolon,    Tuple.Create(7, 17) },
            { Keyboard.SingleQuote,  Tuple.Create(7, 19) },
            { Keyboard.Enter,        Tuple.Create(7, 21) },
            { Keyboard.PAGEUp,       Tuple.Create(7, 24) },
            { Keyboard.RightLED4,    Tuple.Create(7, 26) },
            //Row 10
            { Keyboard.LeftLED5,     Tuple.Create(9, 0)  },
            { Keyboard.LeftShift,    Tuple.Create(9, 5)  },
            { Keyboard.Z,            Tuple.Create(9, 7)  },
            { Keyboard.X,            Tuple.Create(9, 9)  },
            { Keyboard.C,            Tuple.Create(9, 11) },
            { Keyboard.V,            Tuple.Create(9, 12) },
            { Keyboard.B,            Tuple.Create(9, 13) },
            { Keyboard.N,            Tuple.Create(9, 14) },
            { Keyboard.M,            Tuple.Create(9, 15) },
            { Keyboard.Comma,        Tuple.Create(9, 16) },
            { Keyboard.Period,       Tuple.Create(9, 17) },
            { Keyboard.Slash,        Tuple.Create(9, 18) },
            { Keyboard.RightShift,   Tuple.Create(9, 20) },
            { Keyboard.UP,           Tuple.Create(9, 22) },
            { Keyboard.PAGEDown,     Tuple.Create(9, 24) },
            { Keyboard.RightLED5,    Tuple.Create(9, 26) },
            //Row 12
            { Keyboard.LeftLED6,     Tuple.Create(11, 0)  },
            { Keyboard.CTRL_L,       Tuple.Create(11, 2) },
            { Keyboard.WIN_L,        Tuple.Create(11, 4) },
            { Keyboard.ALT_L,        Tuple.Create(11, 6) },
            { Keyboard.SPACE_LL,     Tuple.Create(11, 9)},
            { Keyboard.SPACE_L,      Tuple.Create(11, 10)},
            { Keyboard.SPACE,        Tuple.Create(11, 11)},
            { Keyboard.SPACE_R,      Tuple.Create(11, 12)},
            { Keyboard.SPACE_RR,     Tuple.Create(11, 13)},
            { Keyboard.ALT_R,        Tuple.Create(11, 16)},
            { Keyboard.FN,           Tuple.Create(11, 18)},
            { Keyboard.BLOCKER,      Tuple.Create(11, 19)},
            { Keyboard.LEFT,         Tuple.Create(11, 20)},
            { Keyboard.DOWN,         Tuple.Create(11, 22)},
            { Keyboard.RIGHT,        Tuple.Create(11, 24)},
            { Keyboard.RightLED6,    Tuple.Create(11, 26) },
            //Row 14
            { Keyboard.BottomLED1,   Tuple.Create(13, 0) },
            { Keyboard.BottomLED2,   Tuple.Create(13, 2) },
            { Keyboard.BottomLED3,   Tuple.Create(13, 4) },
            { Keyboard.BottomLED4,   Tuple.Create(13, 5) },
            { Keyboard.BottomLED5,   Tuple.Create(13, 6) },
            { Keyboard.BottomLED6,   Tuple.Create(13, 7) },
            { Keyboard.BottomLED7,   Tuple.Create(13, 8) },
            { Keyboard.BottomLED8,   Tuple.Create(13, 9) },
            { Keyboard.BottomLED9,   Tuple.Create(13, 10)},
            { Keyboard.BottomLED10,  Tuple.Create(13, 11)},
            { Keyboard.BottomLED11,  Tuple.Create(13, 12)},
            { Keyboard.BottomLED12,  Tuple.Create(13, 13)},
            { Keyboard.BottomLED13,  Tuple.Create(13, 14)},
            { Keyboard.BottomLED14,  Tuple.Create(13, 15)},
            { Keyboard.BottomLED15,  Tuple.Create(13, 16)},
            { Keyboard.BottomLED16,  Tuple.Create(13, 17)},
            { Keyboard.BottomLED17,  Tuple.Create(13, 18)},
            { Keyboard.BottomLED18,  Tuple.Create(13, 19)},
            { Keyboard.BottomLED19,  Tuple.Create(13, 20)},
            { Keyboard.BottomLED20,  Tuple.Create(13, 22)},
            { Keyboard.BottomLED21,  Tuple.Create(13, 24)},
            { Keyboard.BottomLED22,  Tuple.Create(13, 26)}
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){   Keyboard.ESC,           Keyboard.LeftLED6,      Keyboard.LeftLED5,      Keyboard.LeftLED4,      Keyboard.NA,
                                        Keyboard.One,           Keyboard.Two,           Keyboard.Three,         Keyboard.Tab,           Keyboard.Q,
                                        Keyboard.W,             Keyboard.E,             Keyboard.CapLock,       Keyboard.A,             Keyboard.S,
                                        Keyboard.D,             Keyboard.LeftShift,     Keyboard.NA,            Keyboard.Z,             Keyboard.X                  } },

            {2, new List<Keyboard>(){   Keyboard.CTRL_L,        Keyboard.WIN_L,         Keyboard.ALT_L,         Keyboard.NA,            Keyboard.RightShift,
                                        Keyboard.UP,            Keyboard.PAGEDown,      Keyboard.DOWN,          Keyboard.TopLED4,       Keyboard.TopLED5,
                                        Keyboard.TopLED6,       Keyboard.TopLED7,       Keyboard.TopLED11,      Keyboard.TopLED12,      Keyboard.TopLED13,
                                        Keyboard.TopLED14,      Keyboard.Eight,         Keyboard.Nine,          Keyboard.Zero,          Keyboard.Hyphen             } },

            {3, new List<Keyboard>(){   Keyboard.I,             Keyboard.O,             Keyboard.P,             Keyboard.LeftBracket,   Keyboard.K,
                                        Keyboard.L,             Keyboard.Semicolon,     Keyboard.SingleQuote,   Keyboard.LeftLED3,      Keyboard.LeftLED2,
                                        Keyboard.LeftLED1,      Keyboard.TopLED1,       Keyboard.Four,          Keyboard.Five,          Keyboard.Six,
                                        Keyboard.Seven,         Keyboard.R,             Keyboard.T,             Keyboard.Y,             Keyboard.U                  } },

            {4, new List<Keyboard>(){   Keyboard.F,             Keyboard.G,             Keyboard.H,             Keyboard.J,             Keyboard.C,
                                        Keyboard.V,             Keyboard.B,             Keyboard.N,             Keyboard.SPACE,         Keyboard.ALT_R,
                                        Keyboard.FN,            Keyboard.LEFT,          Keyboard.RIGHT,         Keyboard.TopLED2,       Keyboard.TopLED3,
                                        Keyboard.TopLED18,      Keyboard.TopLED8,       Keyboard.TopLED9,       Keyboard.TopLED10,      Keyboard.TopLED19           } },

            {5, new List<Keyboard>(){   Keyboard.TopLED15,      Keyboard.TopLED16,      Keyboard.TopLED17,      Keyboard.TopLED20,      Keyboard.Plus,
                                        Keyboard.BackSpace,     Keyboard.DELETE,        Keyboard.TopLED21,      Keyboard.RightBracket,  Keyboard.BackSlash,
                                        Keyboard.HOME,          Keyboard.TopLED22,      Keyboard.Enter,         Keyboard.PAGEUp,        Keyboard.BLOCKER,
                                        Keyboard.TopLED23,      Keyboard.M,             Keyboard.Comma,         Keyboard.Period,        Keyboard.Slash              } },

            {6, new List<Keyboard>(){   Keyboard.TopLED24,      Keyboard.BottomLED22,   Keyboard.BottomLED14,   Keyboard.BottomLED7,    Keyboard.TopLED25,
                                        Keyboard.BottomLED21,   Keyboard.BottomLED13,   Keyboard.BottomLED6,    Keyboard.RightLED1,     Keyboard.BottomLED20,
                                        Keyboard.BottomLED12,   Keyboard.BottomLED5,    Keyboard.RightLED2,     Keyboard.BottomLED19,   Keyboard.BottomLED11,
                                        Keyboard.BottomLED4,    Keyboard.RightLED3,     Keyboard.BottomLED18,   Keyboard.BottomLED10,   Keyboard.BottomLED3         } },

            {7, new List<Keyboard>(){   Keyboard.RightLED4,     Keyboard.BottomLED17,   Keyboard.BottomLED9,    Keyboard.BottomLED2,    Keyboard.MediaLED1,
                                        Keyboard.BottomLED16,   Keyboard.BottomLED8,    Keyboard.BottomLED1,    Keyboard.MediaLED2,     Keyboard.BottomLED15,
                                        Keyboard.SPACE_LL,      Keyboard.NA,            Keyboard.MediaLED3,     Keyboard.NA,            Keyboard.NA,
                                        Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA                 } },

            {8, new List<Keyboard>(){   Keyboard.NA } },
            {9, new List<Keyboard>(){   Keyboard.NA } }
        };

        public DexinKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        /// <summary>
        /// Init the model
        /// MEMO: Device ID should be unique, the better way is get it from the firmware.
        /// MEMO: Marked type since PID, VID is the same as AlienKeyboard
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
                Name = "Dexin Keeb",
                //Type = LightingDevices.DexinKeyboard,
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){ Keyboard.TopLED1, Keyboard.TopLED2, Keyboard.TopLED3, Keyboard.TopLED4, Keyboard.TopLED5, Keyboard.TopLED6, Keyboard.TopLED7,
                                      Keyboard.TopLED8, Keyboard.TopLED9, Keyboard.TopLED10, Keyboard.TopLED11, Keyboard.TopLED12, Keyboard.TopLED13, Keyboard.TopLED14,
                                      Keyboard.TopLED15, Keyboard.TopLED16, Keyboard.TopLED17,Keyboard.TopLED18, Keyboard.TopLED19, Keyboard.TopLED20, Keyboard.TopLED21,
                                      Keyboard.TopLED22, Keyboard.TopLED23, Keyboard.TopLED24, Keyboard.TopLED25},
                //Row 2
                new List<Keyboard>(){ Keyboard.LeftLED1, Keyboard.MediaLED1, Keyboard.MediaLED2, Keyboard.MediaLED3, Keyboard.MediaLED4, Keyboard.MediaLED5, Keyboard.RightLED1 },
                //Row 4
                new List<Keyboard>(){ Keyboard.LeftLED2, Keyboard.ESC,Keyboard.One,Keyboard.Two, Keyboard.Three, Keyboard.Four,
                                      Keyboard.Five, Keyboard.Six, Keyboard.Seven, Keyboard.Eight, Keyboard.Nine, Keyboard.Zero, Keyboard.Hyphen, Keyboard.Plus,
                                      Keyboard.BackSpace, Keyboard.DELETE, Keyboard.RightLED2 },
                //Row 6
                new List<Keyboard>(){ Keyboard.LeftLED3, Keyboard.Tab, Keyboard.Q, Keyboard.W, Keyboard.E, Keyboard.R, Keyboard.T,
                                      Keyboard.Y, Keyboard.U, Keyboard.I, Keyboard.O, Keyboard.P, Keyboard.LeftBracket, Keyboard.RightBracket, Keyboard.BackSlash,
                                      Keyboard.HOME, Keyboard.RightLED3 },
                //Row 8
                new List<Keyboard>(){ Keyboard.LeftLED4, Keyboard.CapLock, Keyboard.A, Keyboard.S, Keyboard.D, Keyboard.F, Keyboard.G, Keyboard.H,
                                      Keyboard.J, Keyboard.K, Keyboard.L, Keyboard.Semicolon, Keyboard.SingleQuote, Keyboard.Enter, Keyboard.PAGEUp, Keyboard.RightLED4 },
                //Row 10
                new List<Keyboard>(){ Keyboard.LeftLED5, Keyboard.LeftShift,Keyboard.Z, Keyboard.X, Keyboard.C, Keyboard.V,Keyboard.B,
                                      Keyboard.N, Keyboard.M, Keyboard.Comma, Keyboard.Period, Keyboard.Slash,Keyboard.RightShift, Keyboard.UP, Keyboard.PAGEDown, Keyboard.RightLED5 },
                //Row 12
                new List<Keyboard>(){ Keyboard.LeftLED6, Keyboard.CTRL_L,Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.SPACE_LL, Keyboard.SPACE_L, Keyboard.SPACE, Keyboard.SPACE_R, Keyboard.SPACE_RR,
                                      Keyboard.ALT_R, Keyboard.FN, Keyboard.BLOCKER, Keyboard.LEFT, Keyboard.DOWN, Keyboard.RIGHT, Keyboard.RightLED6 },
                //Row 14
                new List<Keyboard>(){ Keyboard.BottomLED1, Keyboard.BottomLED2,Keyboard.BottomLED3, Keyboard.BottomLED4, Keyboard.BottomLED5,
                                      Keyboard.BottomLED6, Keyboard.BottomLED7, Keyboard.BottomLED8, Keyboard.BottomLED9, Keyboard.BottomLED10, Keyboard.BottomLED11,
                                      Keyboard.BottomLED12, Keyboard.BottomLED13, Keyboard.BottomLED14, Keyboard.BottomLED15, Keyboard.BottomLED16, Keyboard.BottomLED17,
                                      Keyboard.BottomLED18, Keyboard.BottomLED19, Keyboard.BottomLED20, Keyboard.BottomLED21, Keyboard.BottomLED22 }
            };
        }

        /// <summary>
        /// Each command has 65 bytes, and total is 9 commands
        /// Each commnad should contain 20 keys (or less)
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
                result[1] = 0x11;
                result[2] = 0xf0;
                result[3] = (byte)rowMappings.Key;
                try
                {
                    //((HidStream)_deviceStream).SetFeature(result);
                }
                catch
                {
                    Trace.WriteLine($"False to set feature on dexin keyboard");
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
                        Trace.WriteLine($"False to streaming on dexin keyboard");
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
                    Trace.WriteLine($"False to set feature on dexin keyboard");
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
                        Trace.WriteLine($"False to streaming on dexin keyboard");
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
                    Trace.WriteLine($"False to turn off dexin keyboard");
                }
            }
        }
    }
}
