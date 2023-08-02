using System;
using System.Collections.Generic;
using System.Text;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightDancing.Hardware.Devices
{
    internal class HotKeebController : ILightingControl
    {
        private const int DEVICE_VID = 0x3402; //HM_VID 0x0C45
        private const int DEVICE_PID = 0x0301; //HM_PID 0x0902
        private const int MAX_FEATURE_LENGTH = 65;
        private const int MAX_OUTPUT_LENGTH = 65;
        private const int MAX_INPUT_LENGTH = 65;
        private const int MAX_SCROLLWHEEL_INPUT_LENGTH = 64; //8

        /// <summary>
        /// HotKeyboard use two RX, Item1 for Keyboard, Item2 for ScrollWheels
        /// </summary>
        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<Tuple<HidStream, HidStream>> streams = hidDetector.GetHidStreamSets(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH, MAX_OUTPUT_LENGTH, MAX_INPUT_LENGTH, MAX_SCROLLWHEEL_INPUT_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (Tuple<HidStream, HidStream> stream in streams)
                {
                    HotKeyboardDevice keyboard = new HotKeyboardDevice(stream);
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

    public class HotKeyboardDevice : USBDeviceBase
    {
        private bool _isUIControl = false;

        private const int MAX_REPORT_LENGTH = 65;

        private const int MAX_SCROLLWHEEL_INPUT_LENGTH = 8;

        private bool _isProcessing = false;

        private readonly object _locker = new object();

        private ScrollWheelsMode _wheelMode;
        private Action m_updateHandler;
        public bool _isScrollWheelInvoke = false;

        private CancellationTokenSource _cancelSource = new CancellationTokenSource();

        public HotKeyboardDevice(Tuple<HidStream, HidStream> deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            HotKeyboard keyboard = new HotKeyboard(_model);
            hardwares.Add(keyboard);
            TurnOnScrollWheels();

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                TurnOnUIControl();
                _isUIControl = true;
            }

            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];
            collectBytes[1] = 0x04;
            collectBytes[2] = 0xF2;
            try
            {
                ((HidStream)_deviceStream).SetFeature(collectBytes);
            }
            catch
            {
                Trace.WriteLine($"False to streaming on HotKeyboard");
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"HotKeyboard device count error");
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
                    try
                    {
                        ((HidStream)_deviceStream).SetFeature(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on HotKeyboard");
                    }
                }
            }
        }

        /// <summary>
        /// Get current device of firmware version.
        /// Not support for current device
        /// </summary>
        /// <returns>Firmware version</returns>
        private string GetFirmwareVersion()
        {
            return "N/A";
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = deviceID,
                //USBDeviceType = USBDevices.HotKeyboard,
                Name = "HYTE qRGB Keeb"
            };
        }

        public void TurnOnUIControl()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x04;
            commands[2] = 0x74;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);

                byte[] RXdata = new byte[MAX_REPORT_LENGTH];
                ((HidStream)_deviceStream).GetFeature(RXdata);

                if (RXdata[1] == 0x04 && RXdata[2] == 0x74)
                {
                }
                else
                    Debug.WriteLine("UI control didn't return the correct value");
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on HotKeyboard");
            }
        }

        public void TurnOffUIControl()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x04;
            commands[2] = 0x75;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);

                byte[] RXdata = new byte[MAX_REPORT_LENGTH];
                ((HidStream)_deviceStream).GetFeature(RXdata);

                if (RXdata[1] == 0x04 && RXdata[2] == 0x75)
                {
                }
                else
                    Debug.WriteLine("UI control didn't return the correct value");
            }
            catch
            {
                Trace.WriteLine($"False to turn off UI control on HotKeyboard");
            }
        }

        public override void TurnFwAnimationOn()
        {
            TurnOffUIControl();
            _isUIControl = false;
        }

        /// <summary>
        /// Turn On Scroll Wheels
        /// </summary>
        public void TurnOnScrollWheels()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x12;
            commands[2] = 0xA0;
            commands[3] = 0x6A;
            commands[4] = 0x11;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);

                byte[] RXdata = new byte[MAX_REPORT_LENGTH];
                ((HidStream)_deviceStream).GetFeature(RXdata);

                if (RXdata[1] == 0x12 && RXdata[2] == 0xA0 && RXdata[3] == 0x6A && RXdata[4] == 0x11)
                {
                }
                else
                    Debug.WriteLine("Scrollwheel start didn't return the correct value");

                StartScrollWheels();
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on HotKeyboard");
            }
        }

        /// <summary>
        /// Scroll Wheels four mode: TopLeft, BottomLeft, TopRight, BottomRight
        /// </summary>
        byte[] commands = new byte[MAX_SCROLLWHEEL_INPUT_LENGTH];
        public void StartScrollWheels()
        {
            CancellationToken cancelToken = _cancelSource.Token;

            Task.Run(() =>
            {
                _isProcessing = false;

                lock (_locker)
                {
                    _isProcessing = true;

                    var result = _scrollStream.ReadAsync(commands, 0, MAX_SCROLLWHEEL_INPUT_LENGTH, cancelToken);

                    while (_isProcessing)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            _isProcessing = false;
                            return;
                        }

                        if (commands[0] == 0x05 && commands[1] == 0x09)
                        {
                            if (commands[2] == 0x20)           // ScrollWheels_TopLeft
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_TopLeft;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[2] == 0x40)           // ScrollWheels_BottomLeft
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomLeft;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[3] == 0x20)           // ScrollWheels_TopRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_TopRight;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[3] == 0x40)           // ScrollWheels_BottomRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomRight;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }
                        }

                        if (result.IsCompleted)
                        {
                            commands = new byte[MAX_SCROLLWHEEL_INPUT_LENGTH];
                            result = _scrollStream.ReadAsync(commands, 0, MAX_SCROLLWHEEL_INPUT_LENGTH, cancelToken);
                            _wheelMode = ScrollWheelsMode.NA;
                            _isScrollWheelInvoke = false;
                        }

                        Thread.Sleep(30);
                    }
                }
            }, cancelToken);
        }

        public void TurnOffScrollWheels()
        {
            _cancelSource.Cancel();

            lock (_locker)
            {
                _cancelSource = new CancellationTokenSource();
                byte[] _commands = new byte[MAX_REPORT_LENGTH];
                _commands[1] = 0x12;
                _commands[2] = 0xA0;
                _commands[3] = 0x6A;
                _commands[4] = 0x10;
                try
                {
                    ((HidStream)_deviceStream).SetFeature(_commands);
                }
                catch
                {
                    Trace.WriteLine($"False to turn off scroll wheel on HotKeyboard");
                }

                Thread.Sleep(30);

                if (commands[1] == 0x12 && commands[2] == 0xA0 && commands[3] == 0x6A && commands[4] == 0x10)
                {
                }
                else
                {
                    Debug.WriteLine("Scrollwheel cancellation didn't return the correct value");
                }
            }
        }

        /// <summary>
        /// call enum ScrollWheelsMode
        /// </summary>
        public ScrollWheelsMode GetDeviceScrollWheels()
        {
            return _wheelMode;
        }

        /// <summary>
        /// UI call DLL, register update callback
        /// </summary>
        public void RegisterUpdateCallback(Action func)
        {
            m_updateHandler = func;
        }

        /// <summary>
        /// Includes turn off scroll wheels
        /// </summary>
        public override void Dispose()
        {
            TurnOffScrollWheels();
            base.Dispose();
        }
    }

    public class HotKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 14;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 32;

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
            { Keyboard.TopLED6,      Tuple.Create(0, 7)  },
            { Keyboard.TopLED7,      Tuple.Create(0, 8)  },
            { Keyboard.TopLED8,      Tuple.Create(0, 9)  },
            { Keyboard.TopLED9,      Tuple.Create(0, 11)  },
            { Keyboard.TopLED10,     Tuple.Create(0, 12) },
            { Keyboard.TopLED11,     Tuple.Create(0, 13) },
            { Keyboard.TopLED12,     Tuple.Create(0, 14) },
            { Keyboard.TopLED13,     Tuple.Create(0, 15) },
            { Keyboard.TopLED14,     Tuple.Create(0, 17) },
            { Keyboard.TopLED15,     Tuple.Create(0, 18) },
            { Keyboard.TopLED16,     Tuple.Create(0, 19) },
            { Keyboard.TopLED17,     Tuple.Create(0, 21) },
            { Keyboard.TopLED18,     Tuple.Create(0, 22) },
            { Keyboard.TopLED19,     Tuple.Create(0, 23) },
            { Keyboard.TopLED20,     Tuple.Create(0, 24) },
            { Keyboard.TopLED21,     Tuple.Create(0, 26) },
            { Keyboard.TopLED22,     Tuple.Create(0, 27) },
            { Keyboard.TopLED23,     Tuple.Create(0, 28) },
            { Keyboard.TopLED24,     Tuple.Create(0, 29) },
            { Keyboard.TopLED25,     Tuple.Create(0, 30) },
            //Row 2
            { Keyboard.LeftLED1,     Tuple.Create(1, 0)  },
            { Keyboard.MediaLED1,    Tuple.Create(1, 1) },
            { Keyboard.MediaLED2,    Tuple.Create(1, 3) },
            { Keyboard.MediaLED3,    Tuple.Create(1, 5) },
            { Keyboard.MediaLED4,    Tuple.Create(1, 7) },
            { Keyboard.MediaLED5,    Tuple.Create(1, 9) },
            { Keyboard.RightLED1,    Tuple.Create(1, 31) },
            //Row 4
            { Keyboard.LeftLED2,     Tuple.Create(3, 0)  },
            { Keyboard.ESC,          Tuple.Create(3, 2)  },
            { Keyboard.One,          Tuple.Create(3, 4)  },
            { Keyboard.Two,          Tuple.Create(3, 5)  },
            { Keyboard.Three,        Tuple.Create(3, 7)  },
            { Keyboard.Four,         Tuple.Create(3, 9)  },
            { Keyboard.Five,         Tuple.Create(3, 11) },
            { Keyboard.Six,          Tuple.Create(3, 13) },
            { Keyboard.Seven,        Tuple.Create(3, 14) },
            { Keyboard.Eight,        Tuple.Create(3, 17) },
            { Keyboard.Nine,         Tuple.Create(3, 19) },
            { Keyboard.Zero,         Tuple.Create(3, 21) },
            { Keyboard.Hyphen,       Tuple.Create(3, 23) },
            { Keyboard.Plus,         Tuple.Create(3, 25) },
            { Keyboard.BackSpace,    Tuple.Create(3, 27) },
            { Keyboard.DELETE,       Tuple.Create(3, 30) },
            { Keyboard.RightLED2,    Tuple.Create(3, 31) },
            //Row 6
            { Keyboard.LeftLED3,     Tuple.Create(5, 0)  },
            { Keyboard.Tab,          Tuple.Create(5, 2)  },
            { Keyboard.Q,            Tuple.Create(5, 4)  },
            { Keyboard.W,            Tuple.Create(5, 6)  },
            { Keyboard.E,            Tuple.Create(5, 8)  },
            { Keyboard.R,            Tuple.Create(5, 10) },
            { Keyboard.T,            Tuple.Create(5, 12) },
            { Keyboard.Y,            Tuple.Create(5, 14) },
            { Keyboard.U,            Tuple.Create(5, 16) },
            { Keyboard.I,            Tuple.Create(5, 18) },
            { Keyboard.O,            Tuple.Create(5, 20) },
            { Keyboard.P,            Tuple.Create(5, 22) },
            { Keyboard.LeftBracket,  Tuple.Create(5, 24) },
            { Keyboard.RightBracket, Tuple.Create(5, 25) },
            { Keyboard.BackSlash,    Tuple.Create(5, 27) },
            { Keyboard.HOME,         Tuple.Create(5, 30) },
            { Keyboard.RightLED3,    Tuple.Create(5, 31) },
            //Row 8
            { Keyboard.LeftLED4,     Tuple.Create(7, 0)  },
            { Keyboard.CapLock,      Tuple.Create(7, 2)  },
            { Keyboard.A,            Tuple.Create(7, 4)  },
            { Keyboard.S,            Tuple.Create(7, 6)  },
            { Keyboard.D,            Tuple.Create(7, 8) },
            { Keyboard.F,            Tuple.Create(7, 10) },
            { Keyboard.G,            Tuple.Create(7, 12) },
            { Keyboard.H,            Tuple.Create(7, 14) },
            { Keyboard.J,            Tuple.Create(7, 16) },
            { Keyboard.K,            Tuple.Create(7, 18) },
            { Keyboard.L,            Tuple.Create(7, 20) },
            { Keyboard.Semicolon,    Tuple.Create(7, 22) },
            { Keyboard.SingleQuote,  Tuple.Create(7, 24) },
            { Keyboard.Enter,        Tuple.Create(7, 26) },
            { Keyboard.PAGEUp,       Tuple.Create(7, 30) },
            { Keyboard.RightLED4,    Tuple.Create(7, 31) },
            //Row 10
            { Keyboard.LeftLED5,     Tuple.Create(9, 0)  },
            { Keyboard.LeftShift,    Tuple.Create(9, 3)  },
            { Keyboard.Z,            Tuple.Create(9, 6)  },
            { Keyboard.X,            Tuple.Create(9, 8)  },
            { Keyboard.C,            Tuple.Create(9, 9) },
            { Keyboard.V,            Tuple.Create(9, 11) },
            { Keyboard.B,            Tuple.Create(9, 13) },
            { Keyboard.N,            Tuple.Create(9, 15) },
            { Keyboard.M,            Tuple.Create(9, 17) },
            { Keyboard.Comma,        Tuple.Create(9, 19) },
            { Keyboard.Period,       Tuple.Create(9, 21) },
            { Keyboard.Slash,        Tuple.Create(9, 23) },
            { Keyboard.RightShift,   Tuple.Create(9, 26) },
            { Keyboard.UP,           Tuple.Create(9, 28) },
            { Keyboard.PAGEDown,     Tuple.Create(9, 30) },
            { Keyboard.RightLED5,    Tuple.Create(9, 31) },
            //Row 12
            { Keyboard.LeftLED6,     Tuple.Create(11, 0)  },
            { Keyboard.CTRL_L,       Tuple.Create(11, 2) },
            { Keyboard.WIN_L,        Tuple.Create(11, 4) },
            { Keyboard.ALT_L,        Tuple.Create(11, 6) },
            { Keyboard.SPACE_LL,     Tuple.Create(11, 11)},
            { Keyboard.SPACE_L,      Tuple.Create(11, 12)},
            { Keyboard.SPACE,        Tuple.Create(11, 13)},
            { Keyboard.SPACE_R,      Tuple.Create(11, 14)},
            { Keyboard.SPACE_RR,     Tuple.Create(11, 15)},
            { Keyboard.ALT_R,        Tuple.Create(11, 20)},
            { Keyboard.FN,           Tuple.Create(11, 23)},
            { Keyboard.BLOCKER,      Tuple.Create(11, 24)},
            { Keyboard.LEFT,         Tuple.Create(11, 26)},
            { Keyboard.DOWN,         Tuple.Create(11, 29)},
            { Keyboard.RIGHT,        Tuple.Create(11, 30)},
            { Keyboard.RightLED6,    Tuple.Create(11, 31) },
            //Row 14
            { Keyboard.BottomLED1,   Tuple.Create(13, 1) },
            { Keyboard.BottomLED2,   Tuple.Create(13, 2) },
            { Keyboard.BottomLED3,   Tuple.Create(13, 3) },
            { Keyboard.BottomLED4,   Tuple.Create(13, 4) },
            { Keyboard.BottomLED5,   Tuple.Create(13, 5) },
            { Keyboard.BottomLED6,   Tuple.Create(13, 6) },
            { Keyboard.BottomLED7,   Tuple.Create(13, 7) },
            { Keyboard.BottomLED8,   Tuple.Create(13, 8) },
            { Keyboard.BottomLED9,   Tuple.Create(13, 9)},
            { Keyboard.BottomLED10,  Tuple.Create(13, 10)},
            { Keyboard.BottomLED11,  Tuple.Create(13, 11)},
            { Keyboard.BottomLED12,  Tuple.Create(13, 12)},
            { Keyboard.BottomLED13,  Tuple.Create(13, 13)},
            { Keyboard.BottomLED14,  Tuple.Create(13, 14)},
            { Keyboard.BottomLED15,  Tuple.Create(13, 15)},
            { Keyboard.BottomLED16,  Tuple.Create(13, 16)},
            { Keyboard.BottomLED17,  Tuple.Create(13, 17)},
            { Keyboard.BottomLED18,  Tuple.Create(13, 18)},
            { Keyboard.BottomLED19,  Tuple.Create(13, 19)},
            { Keyboard.BottomLED20,  Tuple.Create(13, 20)},
            { Keyboard.BottomLED21,  Tuple.Create(13, 21)},
            { Keyboard.BottomLED22,  Tuple.Create(13, 22)},
            { Keyboard.BottomLED23,  Tuple.Create(13, 23)},
            { Keyboard.BottomLED24,  Tuple.Create(13, 24)},
            { Keyboard.BottomLED25,  Tuple.Create(13, 25)},
            { Keyboard.BottomLED26,  Tuple.Create(13, 26)},
            { Keyboard.BottomLED27,  Tuple.Create(13, 27)},
            { Keyboard.BottomLED28,  Tuple.Create(13, 28)},
            { Keyboard.BottomLED29,  Tuple.Create(13, 29)},
            { Keyboard.BottomLED30,  Tuple.Create(13, 30)},
        };

        /// <summary>
        /// Command layout for fw "Keeb_65_534B_20220712_Test"
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){ Keyboard.TopLED3,  Keyboard.TopLED2,    Keyboard.TopLED1,    Keyboard.LeftLED1,
                                      Keyboard.LeftLED2,  Keyboard.LeftLED3,    Keyboard.LeftLED4,    Keyboard.LeftLED5,
                                      Keyboard.LeftLED6,  Keyboard.BottomLED1,   Keyboard.BottomLED2,   Keyboard.BottomLED3,
                                      Keyboard.BottomLED4, Keyboard.BottomLED5,   Keyboard.BottomLED6,   Keyboard.BottomLED7,
                                      Keyboard.BottomLED8, Keyboard.BottomLED9,   Keyboard.BottomLED10,   Keyboard.BottomLED11} },

            {2, new List<Keyboard>(){ Keyboard.BottomLED12, Keyboard.BottomLED13,   Keyboard.BottomLED14,   Keyboard.BottomLED15,
                                      Keyboard.BottomLED16, Keyboard.BottomLED17,   Keyboard.BottomLED18,   Keyboard.BottomLED19,
                                      Keyboard.BottomLED20, Keyboard.BottomLED21,   Keyboard.BottomLED22,   Keyboard.BottomLED23,
                                      Keyboard.BottomLED24, Keyboard.BottomLED25,     Keyboard.BottomLED26,     Keyboard.BottomLED27,
                                      Keyboard.BottomLED28, Keyboard.BottomLED29,     Keyboard.BottomLED30,     Keyboard.RightLED6} },

            {3, new List<Keyboard>(){ Keyboard.RightLED5,    Keyboard.RightLED4,      Keyboard.RightLED3,      Keyboard.RightLED2,
                                      Keyboard.RightLED1,    Keyboard.TopLED25,      Keyboard.TopLED24,      Keyboard.TopLED23,
                                      Keyboard.TopLED22,     Keyboard.TopLED21,       Keyboard.TopLED20,       Keyboard.TopLED19,
                                      Keyboard.TopLED18,     Keyboard.TopLED16,       Keyboard.TopLED15,       Keyboard.TopLED14,
                                      Keyboard.TopLED13,     Keyboard.TopLED12,       Keyboard.TopLED11,       Keyboard.TopLED10} },

            {4, new List<Keyboard>(){ Keyboard.TopLED9,     Keyboard.TopLED8,      Keyboard.TopLED7,       Keyboard.TopLED6,
                                      Keyboard.TopLED5,     Keyboard.TopLED4,      Keyboard.TopLED17,      Keyboard.MediaLED1,
                                      Keyboard.MediaLED2,   Keyboard.MediaLED3,    Keyboard.MediaLED4,     Keyboard.MediaLED5,
                                      Keyboard.ESC,         Keyboard.One,          Keyboard.Two,           Keyboard.Three,
                                      Keyboard.Four,        Keyboard.Five,         Keyboard.Six,           Keyboard.Seven,  } },

            {5, new List<Keyboard>(){ Keyboard.Eight,       Keyboard.Nine,          Keyboard.Zero,          Keyboard.Hyphen,
                                      Keyboard.Tab,         Keyboard.Q,
                                      Keyboard.W,           Keyboard.E,             Keyboard.R,             Keyboard.T,
                                      Keyboard.Y,           Keyboard.U,             Keyboard.I,             Keyboard.O,
                                      Keyboard.P,           Keyboard.LeftBracket,    Keyboard.CapLock,     Keyboard.A,             Keyboard.S,             Keyboard.D,} },

            {6, new List<Keyboard>(){ Keyboard.F,           Keyboard.G,             Keyboard.H,             Keyboard.J,
                                      Keyboard.K,           Keyboard.L,             Keyboard.Semicolon,     Keyboard.SingleQuote,
                                      Keyboard.LeftShift,   Keyboard.ALT_R,  
                                      Keyboard.Z,           Keyboard.X,             Keyboard.C,             Keyboard.V,
                                      Keyboard.B,           Keyboard.N,             Keyboard.M,             Keyboard.Comma,
                                      Keyboard.Period,      Keyboard.Slash, } },

            {7, new List<Keyboard>(){ Keyboard.WIN_L, Keyboard.CTRL_L, Keyboard.SPACE_LL,
                                      Keyboard.SPACE_L,       Keyboard.SPACE,     Keyboard.SPACE_R, Keyboard.SPACE_RR,       Keyboard.ALT_L,
                                      Keyboard.FN,       Keyboard.BLOCKER, Keyboard.LEFT,     Keyboard.PAGEDown, Keyboard.Plus, Keyboard.BackSpace,
                                      Keyboard.HOME, Keyboard.RightBracket, Keyboard.BackSlash, Keyboard.DELETE, Keyboard.Enter, Keyboard.PAGEUp } },

            {8, new List<Keyboard>(){ Keyboard.RightShift, Keyboard.UP, Keyboard.DOWN, Keyboard.RIGHT   } },
        };

        public HotKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                //Type = LightingDevices.HotKeyboard,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "HYTE qRGB Keeb"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){ Keyboard.TopLED1, Keyboard.TopLED2, Keyboard.TopLED3, Keyboard.TopLED4, Keyboard.TopLED5, Keyboard.TopLED6, Keyboard.TopLED7,
                                      Keyboard.TopLED8, Keyboard.TopLED9, Keyboard.TopLED10, Keyboard.TopLED11, Keyboard.TopLED12, Keyboard.TopLED13, Keyboard.TopLED14,
                                      Keyboard.TopLED15, Keyboard.TopLED16, Keyboard.TopLED17,Keyboard.TopLED18, Keyboard.TopLED19, Keyboard.TopLED20, Keyboard.TopLED21 },
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
                new List<Keyboard>(){ Keyboard.LeftLED6, Keyboard.CTRL_L,Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.SPACE,
                                      Keyboard.ALT_R, Keyboard.FN, Keyboard.LEFT, Keyboard.DOWN, Keyboard.RIGHT, Keyboard.RightLED6 },
                //Row 14
                new List<Keyboard>(){ Keyboard.BottomLED1, Keyboard.BottomLED2,Keyboard.BottomLED3, Keyboard.BottomLED4, Keyboard.BottomLED5,
                                      Keyboard.BottomLED6, Keyboard.BottomLED7, Keyboard.BottomLED8, Keyboard.BottomLED9, Keyboard.BottomLED10, Keyboard.BottomLED11,
                                      Keyboard.BottomLED12, Keyboard.BottomLED13, Keyboard.BottomLED14, Keyboard.BottomLED15, Keyboard.BottomLED16, Keyboard.BottomLED17,
                                      Keyboard.BottomLED18, Keyboard.BottomLED19, Keyboard.BottomLED20, Keyboard.BottomLED21, Keyboard.BottomLED22 }
            };
        }

        /// <summary>
        /// Each command has 65 bytes, and total is 9 commands
        /// Each commnad should contain 16 keys (or less)
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
                result[1] = 0x81;
                result[2] = 0x08;

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
                    _displayColorBytes.AddRange(result);
                }
                else
                {
                    Trace.WriteLine($"HotKeyboard command key count error");
                }
            }
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            _displayColorBytes = new List<byte>();

            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[1] = 0x81;
                result[2] = 0x08;

                foreach (Keyboard cloumn in rowMappings.Value)
                {
                    if (cloumn != Keyboard.NA)
                    {
                        var color = keyColors[cloumn];
                        result[count * 3 + 5] = color.R;
                        result[count * 3 + 6] = color.G;
                        result[count * 3 + 7] = color.B;
                    }
                    else
                    {
                        Trace.WriteLine($"False to streaming on HotKeyboard");
                    }
                    count++;
                }

                if (count <= 20)
                {
                    try
                    {
                        _displayColorBytes.AddRange(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to process zone select color on HotKeyboard");
                    }
                }
            }
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            _displayColorBytes = new List<byte>();
            for (byte i = 1; i <= COMMAND_LAYOUT.Count; i++)
            {
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[1] = 0x81;
                result[2] = 0x08;

                _displayColorBytes.AddRange(result);
            }
        }
    }
}
