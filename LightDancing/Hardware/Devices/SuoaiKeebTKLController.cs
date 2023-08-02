using System;
using System.Collections.Generic;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightDancing.Hardware.Devices
{
    internal class SuoaiKeebTKLController : ILightingControl
    {
        private const int DEVICE_VID = 0x3402;
        private const int DEVICE_PID = 0x0300;
        private const int MAX_FEATURE_LENGTH = 9;
        private const int MAX_OUTPUT_LENGTH = 65;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<Tuple<HidStream, HidStream>> streams = hidDetector.GetKeebStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH, MAX_OUTPUT_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (Tuple<HidStream, HidStream> stream in streams)
                {
                    SuoaiKeebTKLDevice keyboard = new SuoaiKeebTKLDevice(stream.Item1, stream.Item2);
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

    public class SuoaiKeebTKLDevice : USBDeviceBase
    {
        private bool _isUIControl = false;

        private const int MAX_REPORT_LENGTH = 9;

        private const int MAX_OUTPUT_LENGTH = 65;

        private const int MIDDLE_RGB_PACKAGE_COUNT = 6;
        private const int SURROUND_RGB_PACKAGE_COUNT = 3;

        private bool _isProcessing = false;

        private readonly object _locker = new object();

        private ScrollWheelsMode _wheelMode;

        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        public Action<ScrollWheelsMode> ScrollWheelChanged { set; get; }

        public HidStream _streamingStream;

        public SuoaiKeebTKLDevice(HidStream deviceStream, HidStream streamingStream) : base(deviceStream)
        {
            _streamingStream = streamingStream;
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            SuoaiKeebTKL keyboard = new SuoaiKeebTKL(_model);
            hardwares.Add(keyboard);
            //TurnOnScrollWheels();

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isUIControl)
            {
                TurnOnUIControl();
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"Suoai Keeb TKL device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                //Set Feature
                StartStreaming(RGBParts.Middle);
                //Send middle part RGB to _streamingStream
                List<byte> firstPart = displayColors.GetRange(0, 126 * 3);
                List<byte> secondPart = displayColors.GetRange(126 * 3, displayColors.Count - 126 * 3);
                for (int i = 0; i < MIDDLE_RGB_PACKAGE_COUNT; i++)
                {
                    int getByteCount = firstPart.Count - 64 * i < 64 ? firstPart.Count - 64 * i : 64;
                    List<byte> color = firstPart.GetRange(64 * i, getByteCount);

                    color.Insert(0, 0x00);

                    if (color.Count < 65)
                    {
                        Extensions.PadListWithZeros(color, 65);
                    }

                    try
                    {
                        _deviceStream.Write(color.ToArray());
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on Suoai Keeb TKL");
                    }
                }

                try
                {
                    _deviceStream.Write(new byte[65]);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on Suoai Keeb TKL");
                }

                //Set Feature
                StartStreaming(RGBParts.Surround);
                //Send middle part RGB to _streamingStream
                for (int i = 0; i < SURROUND_RGB_PACKAGE_COUNT; i++)
                {
                    int getByteCount = secondPart.Count - 64 * i < 64 ? secondPart.Count - 64 * i : 64;
                    List<byte> color = secondPart.GetRange(64 * i, getByteCount);
                    color.Insert(0, 0x00);

                    if(color.Count < 65)
                    {
                        Extensions.PadListWithZeros(color, 65);
                    }

                    try
                    {
                        _deviceStream.Write(color.ToArray());
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on Suoai Keeb TKL");
                    }
                }
            }
        }

        private void StartStreaming(RGBParts parts)
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x04;
            commands[2] = (byte)(parts == RGBParts.Middle? 0xF0 : 0xF1);
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"False to streaming on Suoai Keeb TKL");
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
                USBDeviceType = USBDevices.SuoaiKeebTKL,
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
                _isUIControl = true;
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on Suoai Keeb TKL");
            }
        }

        public override void TurnFwAnimationOn()
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[1] = 0x04;
            commands[2] = 0x75;
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
                _isUIControl = false;
            }
            catch
            {
                Trace.WriteLine($"False to turn off UI control on HotKeyboard");
            }
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
        byte[] commands = new byte[MAX_REPORT_LENGTH];
        public void StartScrollWheels()
        {
            CancellationToken cancelToken = _cancelSource.Token;

            Task.Run(() =>
            {
                _isProcessing = false;

                lock (_locker)
                {
                    _isProcessing = true;

                    var result = _deviceStream.ReadAsync(commands, 0, MAX_REPORT_LENGTH, cancelToken);

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
                                ScrollWheelChanged?.Invoke(_wheelMode);
                            }

                            if (commands[2] == 0x40)           // ScrollWheels_BottomLeft
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomLeft;
                                ScrollWheelChanged?.Invoke(_wheelMode);
                            }

                            if (commands[3] == 0x20)           // ScrollWheels_TopRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_TopRight;
                                ScrollWheelChanged?.Invoke(_wheelMode);
                            }

                            if (commands[3] == 0x40)           // ScrollWheels_BottomRight
                            {
                                _wheelMode = ScrollWheelsMode.ScrollWheels_BottomRight;
                                ScrollWheelChanged?.Invoke(_wheelMode);
                            }
                        }

                        if (result.IsCompleted)
                        {
                            commands = new byte[MAX_REPORT_LENGTH];
                            result = _deviceStream.ReadAsync(commands, 0, MAX_REPORT_LENGTH, cancelToken);
                            _wheelMode = ScrollWheelsMode.NA;
                        }

                        if(result.IsFaulted)
                        {
                            commands = new byte[MAX_REPORT_LENGTH];
                            result = _deviceStream.ReadAsync(commands, 0, MAX_REPORT_LENGTH, cancelToken);
                            _wheelMode = ScrollWheelsMode.NA;
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
                    Trace.WriteLine($"False to turn off scroll wheel on Suoai Keeb TKL");
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
        /// Includes turn off scroll wheels
        /// </summary>
        public override void Dispose()
        {
            TurnOffScrollWheels();
            base.Dispose();
        }
    }

    public class SuoaiKeebTKL : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 10;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 21;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 65;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly List<KeebLayoutModel> KEYS_LAYOUTS = new List<KeebLayoutModel>()
        {
            //Row 1
            new KeebLayoutModel(Keyboard.TopLED1,      0, 1, 136),
            new KeebLayoutModel(Keyboard.TopLED2,      0, 2 , 135),
            new KeebLayoutModel(Keyboard.TopLED3,      0, 3 , 134),
            new KeebLayoutModel(Keyboard.TopLED4,      0, 4 , 133),
            new KeebLayoutModel(Keyboard.TopLED5,      0, 5 , 132),
            new KeebLayoutModel(Keyboard.TopLED6,      0, 6 , 131),
            new KeebLayoutModel(Keyboard.TopLED7,      0, 7 , 130),
            new KeebLayoutModel(Keyboard.TopLED8,      0, 8 , 129),
            new KeebLayoutModel(Keyboard.TopLED9,      0, 9 , 128),
            new KeebLayoutModel(Keyboard.TopLED10,     0, 10, 127),
            new KeebLayoutModel(Keyboard.TopLED11,     0, 11, 176),
            new KeebLayoutModel(Keyboard.TopLED12,     0, 12, 175),
            new KeebLayoutModel(Keyboard.TopLED13,     0, 13, 174),
            new KeebLayoutModel(Keyboard.TopLED14,     0, 14, 173),
            new KeebLayoutModel(Keyboard.TopLED15,     0, 15, 172),
            new KeebLayoutModel(Keyboard.TopLED16,     0, 16, 171),
            new KeebLayoutModel(Keyboard.TopLED17,     0, 17, 170),
            new KeebLayoutModel(Keyboard.TopLED18,     0, 18, 169),
            new KeebLayoutModel(Keyboard.TopLED19,     0, 19, 168),
            //Row 2
            new KeebLayoutModel(Keyboard.ScrollWheel,     1, 2 , 18),
            //Row 3
            new KeebLayoutModel(Keyboard.MediaLED1,    2, 1, 78),
            new KeebLayoutModel(Keyboard.MediaLED2,    2, 2, 79),
            new KeebLayoutModel(Keyboard.MediaLED3,    2, 3, 80),
            new KeebLayoutModel(Keyboard.MediaLED4,    2, 4, 99),
            new KeebLayoutModel(Keyboard.MediaLED5,    2, 5, 101),
            //Row 4
            new KeebLayoutModel(Keyboard.LeftLED1,     3, 0 , 137),
            new KeebLayoutModel(Keyboard.ESC,          3, 1 , 1),
            new KeebLayoutModel(Keyboard.F1,           3, 3 , 3),
            new KeebLayoutModel(Keyboard.F2,           3, 4 , 4),
            new KeebLayoutModel(Keyboard.F3,           3, 5 , 5),
            new KeebLayoutModel(Keyboard.F4,           3, 6 , 6),
            new KeebLayoutModel(Keyboard.F5,           3, 8, 7),
            new KeebLayoutModel(Keyboard.F6,           3, 9, 8),
            new KeebLayoutModel(Keyboard.F7,           3, 10, 9),
            new KeebLayoutModel(Keyboard.F8,           3, 11, 10),
            new KeebLayoutModel(Keyboard.F9,           3, 13, 11),
            new KeebLayoutModel(Keyboard.F10,          3, 14, 12),
            new KeebLayoutModel(Keyboard.F11,          3, 15, 13),
            new KeebLayoutModel(Keyboard.F12,          3, 16, 14),
            new KeebLayoutModel(Keyboard.Pause,        3, 17, 15),
            new KeebLayoutModel(Keyboard.ScrLk,        3, 18, 16),
            new KeebLayoutModel(Keyboard.PrnSrc,       3, 19, 17),
            new KeebLayoutModel(Keyboard.RightLED1,    3, 20, 167),
            //Row 5
            new KeebLayoutModel(Keyboard.LeftLED2,       4, 0 , 138),
            new KeebLayoutModel(Keyboard.Tilde,          4, 1 , 22),
            new KeebLayoutModel(Keyboard.One,            4, 2 , 23),
            new KeebLayoutModel(Keyboard.Two,            4, 3 , 24),
            new KeebLayoutModel(Keyboard.Three,          4, 4 , 25),
            new KeebLayoutModel(Keyboard.Four,           4, 5, 26),
            new KeebLayoutModel(Keyboard.Five,           4, 6, 27),
            new KeebLayoutModel(Keyboard.Six,            4, 7, 28),
            new KeebLayoutModel(Keyboard.Seven,          4, 8, 29),
            new KeebLayoutModel(Keyboard.Eight,          4, 9, 30),
            new KeebLayoutModel(Keyboard.Nine,           4, 10, 31),
            new KeebLayoutModel(Keyboard.Zero,           4, 11, 32),
            new KeebLayoutModel(Keyboard.Hyphen,         4, 12, 33),
            new KeebLayoutModel(Keyboard.Plus,           4, 13, 34),
            new KeebLayoutModel(Keyboard.BackSpace,      4, 15, 35),
            new KeebLayoutModel(Keyboard.Insert,         4, 17, 36),
            new KeebLayoutModel(Keyboard.HOME,           4, 18, 37),
            new KeebLayoutModel(Keyboard.PAGEUp,         4, 19, 38),
            new KeebLayoutModel(Keyboard.RightLED2,      4, 20, 166),
            //Row 6
            new KeebLayoutModel(Keyboard.LeftLED3,     5, 0 , 139),
            new KeebLayoutModel(Keyboard.Tab,          5, 1 , 43),
            new KeebLayoutModel(Keyboard.Q,            5, 3 , 44),
            new KeebLayoutModel(Keyboard.W,            5, 4 , 45),
            new KeebLayoutModel(Keyboard.E,            5, 5 , 46),
            new KeebLayoutModel(Keyboard.R,            5, 6, 47),
            new KeebLayoutModel(Keyboard.T,            5, 7, 48),
            new KeebLayoutModel(Keyboard.Y,            5, 8, 49),
            new KeebLayoutModel(Keyboard.U,            5, 9, 50),
            new KeebLayoutModel(Keyboard.I,            5, 10, 51),
            new KeebLayoutModel(Keyboard.O,            5, 11, 52),
            new KeebLayoutModel(Keyboard.P,            5, 12, 53),
            new KeebLayoutModel(Keyboard.LeftBracket,  5, 13, 54),
            new KeebLayoutModel(Keyboard.RightBracket, 5, 14, 55),
            new KeebLayoutModel(Keyboard.BackSlash,    5, 15, 56),
            new KeebLayoutModel(Keyboard.DELETE,       5, 17, 57),
            new KeebLayoutModel(Keyboard.END,          5, 18, 58),
            new KeebLayoutModel(Keyboard.PAGEDown,     5, 19, 59),
            new KeebLayoutModel(Keyboard.RightLED3,    5, 20, 165),
            //Row 8
            new KeebLayoutModel(Keyboard.LeftLED4,     6, 0 , 140),
            new KeebLayoutModel(Keyboard.CapLock,      6, 1 , 64),
            new KeebLayoutModel(Keyboard.A,            6, 3 , 65),
            new KeebLayoutModel(Keyboard.S,            6, 4 , 66),
            new KeebLayoutModel(Keyboard.D,            6, 5, 67),
            new KeebLayoutModel(Keyboard.F,            6, 6, 68),
            new KeebLayoutModel(Keyboard.G,            6, 7, 69),
            new KeebLayoutModel(Keyboard.H,            6, 8, 70),
            new KeebLayoutModel(Keyboard.J,            6, 9, 71),
            new KeebLayoutModel(Keyboard.K,            6, 10, 72),
            new KeebLayoutModel(Keyboard.L,            6, 11, 73),
            new KeebLayoutModel(Keyboard.Semicolon,    6, 12, 74),
            new KeebLayoutModel(Keyboard.SingleQuote,  6, 13, 75),
            new KeebLayoutModel(Keyboard.Enter,        6, 15, 77),
            new KeebLayoutModel(Keyboard.RightLED4,    6, 20, 164),
            //Row 10
            new KeebLayoutModel(Keyboard.LeftLED5,     7, 0 , 141),
            new KeebLayoutModel(Keyboard.LeftShift,    7, 2 , 85),
            new KeebLayoutModel(Keyboard.Z,            7, 4 , 87),
            new KeebLayoutModel(Keyboard.X,            7, 5 , 88),
            new KeebLayoutModel(Keyboard.C,            7, 6, 89),
            new KeebLayoutModel(Keyboard.V,            7, 7, 90),
            new KeebLayoutModel(Keyboard.B,            7, 8, 91),
            new KeebLayoutModel(Keyboard.N,            7, 9, 92),
            new KeebLayoutModel(Keyboard.M,            7, 10, 93),
            new KeebLayoutModel(Keyboard.Comma,        7, 11, 94),
            new KeebLayoutModel(Keyboard.Period,       7, 12, 95),
            new KeebLayoutModel(Keyboard.Slash,        7, 13, 96),
            new KeebLayoutModel(Keyboard.RightShift,   7, 15, 98),
            new KeebLayoutModel(Keyboard.UP,           7, 18, 100),
            new KeebLayoutModel(Keyboard.RightLED5,    7, 20, 163),
            //Row 12
            new KeebLayoutModel(Keyboard.LeftLED6,     8, 0 , 142),
            new KeebLayoutModel(Keyboard.CTRL_L,       8, 1, 106),
            new KeebLayoutModel(Keyboard.WIN_L,        8, 2, 107),
            new KeebLayoutModel(Keyboard.ALT_L,        8, 3, 108),
            //new KeebLayoutModel(Keyboard.SPACE_LL,     8, 5)},
            //new KeebLayoutModel(Keyboard.SPACE_L,      8, 6)},
            new KeebLayoutModel(Keyboard.SPACE,        8, 7, 112),
            //new KeebLayoutModel(Keyboard.SPACE_R,      8, 8},
            //new KeebLayoutModel(Keyboard.SPACE_RR,     8, 9},
            new KeebLayoutModel(Keyboard.ALT_R,        8, 12, 116),
            new KeebLayoutModel(Keyboard.FN,           8, 13, 117),
            new KeebLayoutModel(Keyboard.Menu,         8, 14, 118),
            new KeebLayoutModel(Keyboard.CTRL_R,       8, 15, 119),
            new KeebLayoutModel(Keyboard.LEFT,         8, 17, 120),
            new KeebLayoutModel(Keyboard.DOWN,         8, 18, 121),
            new KeebLayoutModel(Keyboard.RIGHT,        8, 19, 122),
            new KeebLayoutModel(Keyboard.RightLED6,    8, 20, 162),
            //Row 14
            new KeebLayoutModel(Keyboard.BottomLED1,   9, 1, 143),
            new KeebLayoutModel(Keyboard.BottomLED2,   9, 2, 144),
            new KeebLayoutModel(Keyboard.BottomLED3,   9, 3, 145),
            new KeebLayoutModel(Keyboard.BottomLED4,   9, 4, 146),
            new KeebLayoutModel(Keyboard.BottomLED5,   9, 5, 147),
            new KeebLayoutModel(Keyboard.BottomLED6,   9, 6, 148),
            new KeebLayoutModel(Keyboard.BottomLED7,   9, 7, 149),
            new KeebLayoutModel(Keyboard.BottomLED8,   9, 8, 150),
            new KeebLayoutModel(Keyboard.BottomLED9,   9, 9, 151),
            new KeebLayoutModel(Keyboard.BottomLED10,  9, 10, 152),
            new KeebLayoutModel(Keyboard.BottomLED11,  9, 11, 153),
            new KeebLayoutModel(Keyboard.BottomLED12,  9, 12, 154),
            new KeebLayoutModel(Keyboard.BottomLED13,  9, 13, 155),
            new KeebLayoutModel(Keyboard.BottomLED14,  9, 14, 156),
            new KeebLayoutModel(Keyboard.BottomLED15,  9, 15, 157),
            new KeebLayoutModel(Keyboard.BottomLED16,  9, 16, 158),
            new KeebLayoutModel(Keyboard.BottomLED17,  9, 17, 159),
            new KeebLayoutModel(Keyboard.BottomLED18,  9, 18, 160),
            new KeebLayoutModel(Keyboard.BottomLED19,  9, 19, 161),
        };

        public SuoaiKeebTKL(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.SuoaiKeebTKL,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "HYTE qRGB Keeb TKL"
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
            byte[] result = new byte[176 * 3];

            foreach (KeebLayoutModel model in KEYS_LAYOUTS)
            {
                if (model != null)
                {
                    ColorRGB thisColor = colorMatrix[model.Y_POSITION, model.X_POSITION];
                    int commandIndex = (model.COMMAND_INDEX - 1) * 3;
                    result[commandIndex] = thisColor.R;
                    result[commandIndex + 1] = thisColor.G;
                    result[commandIndex + 2] = thisColor.B;
                }
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
            _displayColorBytes = new List<byte>();
            byte[] result = new byte[176 * 3];

            _displayColorBytes.AddRange(result);
        }
    }

    public class KeebLayoutModel
    {
        public Keyboard LED_KEY { private set; get; }
        public int Y_POSITION { private set; get; }
        public int X_POSITION { private set; get; }
        public int COMMAND_INDEX { private set; get; }

        public KeebLayoutModel(Keyboard ledKey, int yPosition, int xPosition, int commnadIndex)
        {
            LED_KEY = ledKey;
            Y_POSITION = yPosition;
            X_POSITION = xPosition;
            COMMAND_INDEX = commnadIndex;
        }
    }

    public enum RGBParts
    {
        Middle,
        Surround,
    }
}
