using System;
using System.Collections.Generic;
using System.Text;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class DROPALTKeebController : ILightingControl
    {
        private const int DEVICE_VID = 0x04D8;
        private const int DEVICE_PID = 0xEED3;
        private const int MAX_FEATURE_LENGTH = 0;
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
                    DROPALTKeyboardDevice keyboard = new DROPALTKeyboardDevice(stream);
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

    public class DROPALTKeyboardDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 65;
        private readonly object _locker = new object();
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private bool _isProcessing = false;
        private ScrollWheelsMode _wheelMode;
        private Action m_updateHandler;
        public bool _isScrollWheelInvoke = false;

        public DROPALTKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            DROPALTKeyboard keyboard = new DROPALTKeyboard(_model);
            hardwares.Add(keyboard);
            TurnOnScrollWheels();

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
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                //Type = LightingDevices.DROPALTKeyboard,
                Name = "DROPALT Keeb"
            };
        }

        /// <summary>
        /// Turn On Scroll Wheels
        /// </summary>
        public void TurnOnScrollWheels()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x12;
            commands[2] = 0x01;

            try
            {
                _deviceStream.Write(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on scroll wheel on dropalt");
            }
            SartScrollWheels();
        }

        /// <summary>
        /// Turn Off Scroll Wheels
        /// </summary>
        public void TurnOffScrollWheels()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x12;
            commands[2] = 0x00;

            try
            {
                _deviceStream.Write(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn off scroll wheel on dropalt");
            }
            _cancelSource.Cancel();

            lock (_locker)
            {
                _cancelSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Scroll Wheels four mode: TopLeft, BottomLeft, TopRight, BottomRight
        /// </summary>
        byte[] commands = new byte[MAX_REPORT_LENGTH];
        public void SartScrollWheels()
        {
            CancellationToken cancelToken = _cancelSource.Token;

            Task.Run(() =>
            {
                _isProcessing = false;

                lock (_locker)
                {
                    _isProcessing = true;

                    var result = _deviceStream.ReadAsync(commands, 0, 65);

                    while (_isProcessing)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            _isProcessing = false;
                            return;
                        }

                        if (commands[1] == 0x01)
                        {
                            if (commands[35] == 0x20)           // ScrollWheels_TopLeft
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_TopLeft;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[35] == 0x40)           // ScrollWheels_BottomLeft
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomLeft;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[39] == 0x02)           // ScrollWheels_TopRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_TopRight;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }

                            if (commands[38] == 0x40)           // ScrollWheels_BottomRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomRight;
                                _isScrollWheelInvoke = true;
                                m_updateHandler?.Invoke();
                            }
                        }

                        if (result.IsCompleted)
                        {
                            commands = new byte[MAX_REPORT_LENGTH];
                            result = _deviceStream.ReadAsync(commands, 0, 65);
                            _wheelMode = ScrollWheelsMode.NA;
                            _isScrollWheelInvoke = false;
                        }

                        Thread.Sleep(30);
                    }
                }
            }, cancelToken);
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

    public class DROPALTKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 7;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 19;

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
            { Keyboard.TopLED1,      Tuple.Create(0, 4)  },
            { Keyboard.TopLED2,      Tuple.Create(0, 5)  },
            { Keyboard.TopLED3,      Tuple.Create(0, 6)  },
            { Keyboard.TopLED4,      Tuple.Create(0, 7)  },
            { Keyboard.TopLED5,      Tuple.Create(0, 8)  },
            { Keyboard.TopLED6,      Tuple.Create(0, 9)  },
            { Keyboard.TopLED7,      Tuple.Create(0, 10) },
            { Keyboard.TopLED8,      Tuple.Create(0, 11) },
            { Keyboard.TopLED9,      Tuple.Create(0, 12) },
            { Keyboard.TopLED10,     Tuple.Create(0, 13) },
            { Keyboard.TopLED11,     Tuple.Create(0, 14) },
            { Keyboard.TopLED12,     Tuple.Create(0, 15) },
            { Keyboard.TopLED13,     Tuple.Create(0, 16) },
            //Row 2
            { Keyboard.LeftLED1,     Tuple.Create(1, 0)  },
            { Keyboard.ESC,          Tuple.Create(1, 2)  },
            { Keyboard.One,          Tuple.Create(1, 3)  },
            { Keyboard.Two,          Tuple.Create(1, 4)  },
            { Keyboard.Three,        Tuple.Create(1, 5)  },
            { Keyboard.Four,         Tuple.Create(1, 6)  },
            { Keyboard.Five,         Tuple.Create(1, 7)  },
            { Keyboard.Six,          Tuple.Create(1, 8)  },
            { Keyboard.Seven,        Tuple.Create(1, 9)  },
            { Keyboard.Eight,        Tuple.Create(1, 10) },
            { Keyboard.Nine,         Tuple.Create(1, 11) },
            { Keyboard.Zero,         Tuple.Create(1, 12) },
            { Keyboard.Hyphen,       Tuple.Create(1, 13) },
            { Keyboard.Plus,         Tuple.Create(1, 14) },
            { Keyboard.BackSpace,    Tuple.Create(1, 16) },
            { Keyboard.DELETE,       Tuple.Create(1, 17) },
            { Keyboard.RightLED1,    Tuple.Create(1, 18) },
            //Row 3
            { Keyboard.LeftLED2,     Tuple.Create(2, 0)  },
            { Keyboard.Tab,          Tuple.Create(2, 2)  },
            { Keyboard.Q,            Tuple.Create(2, 3)  },
            { Keyboard.W,            Tuple.Create(2, 4)  },
            { Keyboard.E,            Tuple.Create(2, 5)  },
            { Keyboard.R,            Tuple.Create(2, 6)  },
            { Keyboard.T,            Tuple.Create(2, 7)  },
            { Keyboard.Y,            Tuple.Create(2, 8)  },
            { Keyboard.U,            Tuple.Create(2, 9)  },
            { Keyboard.I,            Tuple.Create(2, 10) },
            { Keyboard.O,            Tuple.Create(2, 11) },
            { Keyboard.P,            Tuple.Create(2, 12) },
            { Keyboard.LeftBracket,  Tuple.Create(2, 13) },
            { Keyboard.RightBracket, Tuple.Create(2, 14) },
            { Keyboard.BackSlash,    Tuple.Create(2, 16) },
            { Keyboard.HOME,         Tuple.Create(2, 17) },
            { Keyboard.RightLED2,    Tuple.Create(2, 18) },
            //Row 4
            { Keyboard.LeftLED3,     Tuple.Create(3, 0)  },
            { Keyboard.CapLock,      Tuple.Create(3, 2)  },
            { Keyboard.A,            Tuple.Create(3, 3)  },
            { Keyboard.S,            Tuple.Create(3, 4)  },
            { Keyboard.D,            Tuple.Create(3, 5)  },
            { Keyboard.F,            Tuple.Create(3, 6)  },
            { Keyboard.G,            Tuple.Create(3, 7)  },
            { Keyboard.H,            Tuple.Create(3, 8)  },
            { Keyboard.J,            Tuple.Create(3, 9)  },
            { Keyboard.K,            Tuple.Create(3, 10) },
            { Keyboard.L,            Tuple.Create(3, 11) },
            { Keyboard.Semicolon,    Tuple.Create(3, 12) },
            { Keyboard.SingleQuote,  Tuple.Create(3, 13) },
            { Keyboard.Enter,        Tuple.Create(3, 16) },
            { Keyboard.PAGEUp,       Tuple.Create(3, 17) },
            { Keyboard.RightLED3,    Tuple.Create(3, 18) },
            //Row 5
            { Keyboard.LeftLED4,     Tuple.Create(4, 0)  },
            { Keyboard.LeftShift,    Tuple.Create(4, 2)  },
            { Keyboard.Z,            Tuple.Create(4, 3)  },
            { Keyboard.X,            Tuple.Create(4, 4)  },
            { Keyboard.C,            Tuple.Create(4, 5) },
            { Keyboard.V,            Tuple.Create(4, 6) },
            { Keyboard.B,            Tuple.Create(4, 7) },
            { Keyboard.N,            Tuple.Create(4, 8) },
            { Keyboard.M,            Tuple.Create(4, 9) },
            { Keyboard.Comma,        Tuple.Create(4, 10) },
            { Keyboard.Period,       Tuple.Create(4, 11) },
            { Keyboard.Slash,        Tuple.Create(4, 12) },
            { Keyboard.RightShift,   Tuple.Create(4, 14) },
            { Keyboard.UP,           Tuple.Create(4, 16) },
            { Keyboard.PAGEDown,     Tuple.Create(4, 17) },
            { Keyboard.RightLED4,    Tuple.Create(4, 18) },
            //Row 6
            { Keyboard.LeftLED5,     Tuple.Create(5, 0)  },
            { Keyboard.CTRL_L,       Tuple.Create(5, 1)  },
            { Keyboard.WIN_L,        Tuple.Create(5, 2)  },
            { Keyboard.ALT_L,        Tuple.Create(5, 3)  },
            { Keyboard.SPACE,        Tuple.Create(5, 7)  },
            { Keyboard.ALT_R,        Tuple.Create(5, 11) },
            { Keyboard.FN,           Tuple.Create(5, 12) },
            { Keyboard.LEFT,         Tuple.Create(5, 14) },
            { Keyboard.DOWN,         Tuple.Create(5, 16) },
            { Keyboard.RIGHT,        Tuple.Create(5, 17) },
            { Keyboard.RightLED5,    Tuple.Create(5, 18) },
            //Row 7
            { Keyboard.BottomLED1,   Tuple.Create(6, 0)  },
            { Keyboard.BottomLED2,   Tuple.Create(6, 1)  },
            { Keyboard.BottomLED3,   Tuple.Create(6, 2)  },
            { Keyboard.BottomLED4,   Tuple.Create(6, 3)  },
            { Keyboard.BottomLED5,   Tuple.Create(6, 5)  },
            { Keyboard.BottomLED6,   Tuple.Create(6, 6)  },
            { Keyboard.BottomLED7,   Tuple.Create(6, 7)  },
            { Keyboard.BottomLED8,   Tuple.Create(6, 8)  },
            { Keyboard.BottomLED9,   Tuple.Create(6, 9)  },
            { Keyboard.BottomLED10,  Tuple.Create(6, 11) },
            { Keyboard.BottomLED11,  Tuple.Create(6, 12) },
            { Keyboard.BottomLED12,  Tuple.Create(6, 13) },
            { Keyboard.BottomLED13,  Tuple.Create(6, 14) },
            { Keyboard.BottomLED14,  Tuple.Create(6, 16) },
            { Keyboard.BottomLED15,  Tuple.Create(6, 18) }
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){   Keyboard.ESC,           Keyboard.One,           Keyboard.Two,           Keyboard.Three,         Keyboard.Four,
                                        Keyboard.Five,          Keyboard.Six,           Keyboard.Seven,         Keyboard.Eight,         Keyboard.Nine,
                                        Keyboard.Zero,          Keyboard.Hyphen,        Keyboard.Plus,          Keyboard.BackSpace,     Keyboard.DELETE,
                                        Keyboard.Tab,           Keyboard.Q,             Keyboard.W,             Keyboard.E,             Keyboard.R              } },

            {2, new List<Keyboard>(){   Keyboard.T,             Keyboard.Y,             Keyboard.U,             Keyboard.I,             Keyboard.O,
                                        Keyboard.P,             Keyboard.LeftBracket,   Keyboard.RightBracket,  Keyboard.BackSlash,     Keyboard.HOME,
                                        Keyboard.CapLock,       Keyboard.A,             Keyboard.S,             Keyboard.D,             Keyboard.F,
                                        Keyboard.G,             Keyboard.H,             Keyboard.J,             Keyboard.K,             Keyboard.L              } },

            {3, new List<Keyboard>(){   Keyboard.Semicolon,     Keyboard.SingleQuote,   Keyboard.Enter,         Keyboard.PAGEUp,        Keyboard.LeftShift,
                                        Keyboard.Z,             Keyboard.X,             Keyboard.C,             Keyboard.V,             Keyboard.B,
                                        Keyboard.N,             Keyboard.M,             Keyboard.Comma,         Keyboard.Period,        Keyboard.Slash,
                                        Keyboard.RightShift,    Keyboard.UP,            Keyboard.PAGEDown,      Keyboard.CTRL_L,        Keyboard.WIN_L          } },

            {4, new List<Keyboard>(){   Keyboard.ALT_L,         Keyboard.SPACE,         Keyboard.ALT_R,         Keyboard.FN,            Keyboard.LEFT,
                                        Keyboard.DOWN,          Keyboard.RIGHT,         Keyboard.BottomLED1,    Keyboard.BottomLED2,    Keyboard.BottomLED3,
                                        Keyboard.BottomLED4,    Keyboard.BottomLED5,    Keyboard.BottomLED6,    Keyboard.BottomLED7,    Keyboard.BottomLED8,
                                        Keyboard.BottomLED9,    Keyboard.BottomLED10,   Keyboard.BottomLED11,   Keyboard.BottomLED12,   Keyboard.BottomLED13    } },

            {5, new List<Keyboard>(){   Keyboard.BottomLED14,   Keyboard.BottomLED15,   Keyboard.RightLED5,     Keyboard.RightLED4,     Keyboard.RightLED3,
                                        Keyboard.RightLED2,     Keyboard.RightLED1,     Keyboard.TopLED13,      Keyboard.TopLED12,      Keyboard.TopLED11,
                                        Keyboard.TopLED10,      Keyboard.TopLED9,       Keyboard.TopLED8,       Keyboard.TopLED7,       Keyboard.TopLED6,
                                        Keyboard.TopLED5,       Keyboard.TopLED4,       Keyboard.TopLED3,       Keyboard.TopLED2,       Keyboard.TopLED1        } },

            {6, new List<Keyboard>(){   Keyboard.LeftLED1,      Keyboard.LeftLED2,      Keyboard.LeftLED3,      Keyboard.LeftLED4,      Keyboard.LeftLED5,
                                        Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,
                                        Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,
                                        Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA,            Keyboard.NA             } }
        };


        public DROPALTKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                //Type = LightingDevices.DROPALTKeyboard,
                Name = "DROPALT Keeb"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){ Keyboard.TopLED1, Keyboard.TopLED2, Keyboard.TopLED3, Keyboard.TopLED4, Keyboard.TopLED5, Keyboard.TopLED6, Keyboard.TopLED7,
                                      Keyboard.TopLED8, Keyboard.TopLED9, Keyboard.TopLED10, Keyboard.TopLED11, Keyboard.TopLED12, Keyboard.TopLED13},
                //Row 2
                new List<Keyboard>(){ Keyboard.LeftLED1, Keyboard.ESC,Keyboard.One,Keyboard.Two, Keyboard.Three, Keyboard.Four,
                                      Keyboard.Five, Keyboard.Six, Keyboard.Seven, Keyboard.Eight, Keyboard.Nine, Keyboard.Zero, Keyboard.Hyphen, Keyboard.Plus,
                                      Keyboard.BackSpace, Keyboard.DELETE, Keyboard.RightLED1 },
                //Row 3
                new List<Keyboard>(){ Keyboard.LeftLED2, Keyboard.Tab, Keyboard.Q, Keyboard.W, Keyboard.E, Keyboard.R, Keyboard.T,
                                      Keyboard.Y, Keyboard.U, Keyboard.I, Keyboard.O, Keyboard.P, Keyboard.LeftBracket, Keyboard.RightBracket, Keyboard.BackSlash,
                                      Keyboard.HOME, Keyboard.RightLED2 },
                //Row 4
                new List<Keyboard>(){ Keyboard.LeftLED3, Keyboard.CapLock, Keyboard.A, Keyboard.S, Keyboard.D, Keyboard.F, Keyboard.G, Keyboard.H,
                                      Keyboard.J, Keyboard.K, Keyboard.L, Keyboard.Semicolon, Keyboard.SingleQuote, Keyboard.Enter, Keyboard.PAGEUp, Keyboard.RightLED3 },
                //Row 5
                new List<Keyboard>(){ Keyboard.LeftLED4, Keyboard.LeftShift,Keyboard.Z, Keyboard.X, Keyboard.C, Keyboard.V,Keyboard.B,
                                      Keyboard.N, Keyboard.M, Keyboard.Comma, Keyboard.Period, Keyboard.Slash,Keyboard.RightShift, Keyboard.UP, Keyboard.PAGEDown, Keyboard.RightLED4 },
                //Row 6
                new List<Keyboard>(){ Keyboard.LeftLED5, Keyboard.CTRL_L,Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.SPACE,
                                      Keyboard.ALT_R, Keyboard.FN, Keyboard.LEFT, Keyboard.DOWN, Keyboard.RIGHT, Keyboard.RightLED5 },
                //Row 7
                new List<Keyboard>(){ Keyboard.BottomLED1, Keyboard.BottomLED2,Keyboard.BottomLED3, Keyboard.BottomLED4, Keyboard.BottomLED5,
                                      Keyboard.BottomLED6, Keyboard.BottomLED7, Keyboard.BottomLED8, Keyboard.BottomLED9, Keyboard.BottomLED10, Keyboard.BottomLED11,
                                      Keyboard.BottomLED12, Keyboard.BottomLED13, Keyboard.BottomLED14, Keyboard.BottomLED15, }
            };
        }

        /// <summary>
        /// Each command has 65 bytes, and total is 6 commands
        /// Each commnad should contain 20 keys (or less)
        /// When DROPALT needs to send data, it will delay time.  Lines 283
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
                result[2] = 0xF0;
                result[3] = (byte)rowMappings.Key;
                result[4] = 0x01;

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
                        Trace.WriteLine($"False to streaming on dropalt");
                    }
                }
            }

            Thread.Sleep(15);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            foreach (var rowMappings in COMMAND_LAYOUT)
            {
                int count = 0;
                byte[] result = new byte[MAX_REPORT_LENGTH];
                result[1] = 0x11;
                result[2] = 0xF0;
                result[3] = (byte)rowMappings.Key;
                result[4] = 0x01;

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
                        Trace.WriteLine($"False to streaming on dropalt");
                    }
                }
            }

            Thread.Sleep(15);
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];
            for (byte i = 1; i <= 6; i++)
            {
                collectBytes[1] = 0x11;
                collectBytes[2] = 0xf0;
                collectBytes[3] = i;
                collectBytes[3] = 0x01;

                try
                {
                    //_deviceStream.Write(collectBytes);
                }
                catch
                {
                    Trace.WriteLine($"False to turn off on dropalt");
                }
            }
        }
    }
}
