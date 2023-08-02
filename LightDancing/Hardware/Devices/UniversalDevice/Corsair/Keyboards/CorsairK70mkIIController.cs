using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LightDancing.Hardware.Devices.UniversalDevice.Corsair.Keyboards
{
    internal class CorsairK70mkIIController : ILightingControl
    {
        private const int DEVICE_VID = 0x1b1c;
        private const int DEVICE_PID = 0x1b49;
        private const int MAX_FEATURE_LENGTH = 65;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    CorsairK70mkIIDevice keyboard = new CorsairK70mkIIDevice(stream);
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

    public class CorsairK70mkIIDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 65;

        private const int LED_COUNT = 115;

        private bool _isUIControl = false;

        private static readonly byte COMMAND_STREAM = 0x7F;
        private static readonly byte COMMAND_WRITE = 0x07;
        private static readonly byte COMMAND_SUBMIT = 0x28;
        private static readonly int KEY_CODE_LENGTH = 132;
        private static readonly int TOTAL_KEY_CODE_COMMAND_LENGTH = 230; //(132 - 17) * 2;
        private static readonly int KEY_CODE_COMMAND_PACKET_LENGTH = 60;
        private readonly List<int> SKIP_KEY_CODES = new List<int> { 63, 65, 66, 81, 83, 85, 111, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129 };

        public CorsairK70mkIIDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CorsairK70mkIIKeeb keyboard = new CorsairK70mkIIKeeb(_model);
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
                Trace.WriteLine($"Corsair K70 mk2 Keyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                List<byte> displayColors = _lightingBase[0].GetDisplayColors();
                /*To send color R, G, B*/
                for (int i = 0; i < 3; i++)
                {
                    byte[] colors = displayColors.GetRange(144 * i, 144).ToArray();
                    SendStreaming(1, 60, colors[0..60]);
                    SendStreaming(2, 60, colors[60..120]);
                    SendStreaming(3, 24, colors[120..144]);
                    SubmitColor((byte)(i + 1), 3, (byte)(i / 2 + 1));
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
                USBDeviceType = USBDevices.CorsairK70MKII,
                Name = "Corsair K70 RGB mkII"
            };
        }

        /// <summary>
        /// Turn firmware animation off
        /// </summary>
        public void SetCurrentLedEffectOff()
        {
            SendFeatureCommands(new byte[3] { 0x00, 0x0E, 0x01 });
            SendWriteCommands(new byte[4] { 0x00, 0x07, 0x04, 0x02 });
            SendWriteCommands(new byte[6] { 0x00, 0x07, 0x05, 0x02, 0x00, 0x03 });

            InitKeyCodes();
            _isUIControl = true;
        }

        private void InitKeyCodes()
        {
            SendWriteCommands(new byte[6] { 0x00, 0x07, 0x05, 0x08, 0x00, 0x01 });
            byte[] commands = new byte[TOTAL_KEY_CODE_COMMAND_LENGTH];
            int count = 0;

            for (int i = 0; i < KEY_CODE_LENGTH; i++)
            {
                if (!SKIP_KEY_CODES.Contains(i))
                {
                    commands[count * 2] = (byte)i;
                    commands[count * 2 + 1] = 0xC0;
                    count++;
                }
            }

            SendKeyCommands(commands);
        }

        private void SendKeyCommands(byte[] commands)
        {
            for (int i = 0; i < commands.Length; i += KEY_CODE_COMMAND_PACKET_LENGTH)
            {
                byte[] array = new byte[MAX_REPORT_LENGTH];
                array[0] = 0x00;
                array[1] = 0x07;
                array[2] = 0x40;
                array[3] = (byte)(i == 3 * KEY_CODE_COMMAND_PACKET_LENGTH ? 0x19 : 0x1E);
                array[4] = 0x00;

                int command_count = commands.Length - i < KEY_CODE_COMMAND_PACKET_LENGTH ? commands.Length - i : KEY_CODE_COMMAND_PACKET_LENGTH;
                Buffer.BlockCopy(commands, i, array, 5, command_count);

                try
                {
                    ((HidStream)_deviceStream).Write(array);
                }
                catch { }
            }
        }

        /// <summary>
        /// Turn firmware animation on
        /// </summary>
        public void SetCurrentLedEffectOn()
        {
            SendWriteCommands(new byte[4] { 0x00, 0x07, 0x04, 0x01 });
            SendWriteCommands(new byte[6] { 0x00, 0x07, 0x05, 0x01, 0x00, 0x03 });
        }

        public override void TurnFwAnimationOn()
        {
            SetCurrentLedEffectOn();
        }

        private void SubmitColor(byte colorIndex, byte packetIndex, byte finish)
        {
            byte[] arr = new byte[MAX_REPORT_LENGTH];
            arr[0x00] = 0x00;
            arr[0x01] = COMMAND_WRITE;
            arr[0x02] = COMMAND_SUBMIT;
            arr[0x03] = colorIndex;
            arr[0x04] = packetIndex;
            arr[0x05] = finish;
            try
            {
                ((HidStream)_deviceStream).Write(arr);
            }
            catch
            {
                Trace.WriteLine($"Corsair K70 mk2 Keyboard fail to write commands");
            }
        }

        private void SendStreaming(byte packetId, byte dataSize, byte[] data)
        {
            byte[] arr = new byte[MAX_REPORT_LENGTH];
            arr[0x00] = 0x00;
            arr[0x01] = COMMAND_STREAM;
            arr[0x02] = packetId;
            arr[0x03] = dataSize;
            arr[0x04] = 0;

            for (int i = 0; i < data.Length; i++)
            {
                arr[i + 5] = data[i];
            }
            try
            {
                ((HidStream)_deviceStream).Write(arr);
            }
            catch
            {
                Trace.WriteLine($"Corsair K70 mk2 Keyboard fail to write commands");
            }
        }

        private void SendWriteCommands(byte[] data)
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            for (int i = 0; i < data.Length; i++)
            {
                commands[i] = data[i];
            }

            try
            {
                ((HidStream)_deviceStream).Write(commands);
            }
            catch
            {
                Trace.WriteLine($"Corsair K70 mk2 Keyboard fail to write commands");
            }
        }

        private void SendFeatureCommands(byte[] data)
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            for (int i = 0; i < data.Length; i++)
            {
                commands[i] = data[i];
            }

            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"Corsair K70 mk2 Keyboard fail to set feature");
            }
        }
    }

    public class CorsairK70mkIIKeeb : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 7;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 21;

        /// <summary>
        /// Key = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// Value = command index, -1 means no led
        /// </summary>
        private static readonly Dictionary<Tuple<int, int>, int> KEYS_LAYOUTS = new Dictionary<Tuple<int, int>, int>()
        {
            {Tuple.Create(0,0),      -1 },
            {Tuple.Create(0,1),      -1 },
            {Tuple.Create(0,2),      -1 },
            {Tuple.Create(0,3),      125 },
            {Tuple.Create(0,4),      137 },
            {Tuple.Create(0,5),      8 },
            {Tuple.Create(0,6),      -1 },
            {Tuple.Create(0,7),      -1 },
            {Tuple.Create(0,8),      -1 },
            {Tuple.Create(0,9),      -1 },
            {Tuple.Create(0,10),      -1 },
            {Tuple.Create(0,11),      59 },
            {Tuple.Create(0,12),      -1 },
            {Tuple.Create(0,13),      -1 },
            {Tuple.Create(0,14),      -1 },
            {Tuple.Create(0,15),      -1 },
            {Tuple.Create(0,16),      -1 },
            {Tuple.Create(0,17),      20 },
            {Tuple.Create(0,18),      -1 },
            {Tuple.Create(0,19),      -1 },
            {Tuple.Create(0,20),      -1 },
            {Tuple.Create(1,0),      0 },
            {Tuple.Create(1,1),      12 },
            {Tuple.Create(1,2),      24 },
            {Tuple.Create(1,3),      36 },
            {Tuple.Create(1,4),      48 },
            {Tuple.Create(1,5),      60 },
            {Tuple.Create(1,6),      72 },
            {Tuple.Create(1,7),      84 },
            {Tuple.Create(1,8),      96 },
            {Tuple.Create(1,9),      -1 },
            {Tuple.Create(1,10),     108 },
            {Tuple.Create(1,11),     120 },
            {Tuple.Create(1,12),     132 },
            {Tuple.Create(1,13),     6   },
            {Tuple.Create(1,14),     18  },
            {Tuple.Create(1,15),     30 },
            {Tuple.Create(1,16),     42 },
            {Tuple.Create(1,17),     32 },
            {Tuple.Create(1,18),     44 },
            {Tuple.Create(1,19),     56 },
            {Tuple.Create(1,20),     68 },
            {Tuple.Create(2,0),      1 },
            {Tuple.Create(2,1),      13 },
            {Tuple.Create(2,2),      25 },
            {Tuple.Create(2,3),      37 },
            {Tuple.Create(2,4),      49 },
            {Tuple.Create(2,5),      61 },
            {Tuple.Create(2,6),      73 },
            {Tuple.Create(2,7),      85 },
            {Tuple.Create(2,8),      97 },
            {Tuple.Create(2,9),      109 },
            {Tuple.Create(2,10),     121 },
            {Tuple.Create(2,11),     133 },
            {Tuple.Create(2,12),     7 },
            {Tuple.Create(2,13),     31 },
            {Tuple.Create(2,14),     54 },
            {Tuple.Create(2,15),     66 },
            {Tuple.Create(2,16),     78 },
            {Tuple.Create(2,17),     80 },
            {Tuple.Create(2,18),     92 },
            {Tuple.Create(2,19),     104 },
            {Tuple.Create(2,20),     116 },
            {Tuple.Create(3,0),      2 },
            {Tuple.Create(3,1),      14 },
            {Tuple.Create(3,2),      26 },
            {Tuple.Create(3,3),      38 },
            {Tuple.Create(3,4),      50 },
            {Tuple.Create(3,5),      62 },
            {Tuple.Create(3,6),      74 },
            {Tuple.Create(3,7),      86 },
            {Tuple.Create(3,8),      98 },
            {Tuple.Create(3,9),      110 },
            {Tuple.Create(3,10),     122 },
            {Tuple.Create(3,11),     134 },
            {Tuple.Create(3,12),     90 },
            {Tuple.Create(3,13),     102 },
            {Tuple.Create(3,14),     43 },
            {Tuple.Create(3,15),     55 },
            {Tuple.Create(3,16),     67 },
            {Tuple.Create(3,17),     9 },
            {Tuple.Create(3,18),     21 },
            {Tuple.Create(3,19),     33 },
            {Tuple.Create(3,20),     128 },
            {Tuple.Create(4,0),      3 },
            {Tuple.Create(4,1),      15 },
            {Tuple.Create(4,2),      27 },
            {Tuple.Create(4,3),      39 },
            {Tuple.Create(4,4),      51 },
            {Tuple.Create(4,5),      63 },
            {Tuple.Create(4,6),      75 },
            {Tuple.Create(4,7),      87 },
            {Tuple.Create(4,8),      99 },
            {Tuple.Create(4,9),      111 },
            {Tuple.Create(4,10),     123 },
            {Tuple.Create(4,11),     135 },
            {Tuple.Create(4,12),     -1 },
            {Tuple.Create(4,13),     126 },
            {Tuple.Create(4,14),     -1 },
            {Tuple.Create(4,15),     -1 },
            {Tuple.Create(4,16),     -1 },
            {Tuple.Create(4,17),     57 },
            {Tuple.Create(4,18),     69 },
            {Tuple.Create(4,19),     81 },
            {Tuple.Create(4,20),     -1 },
            {Tuple.Create(5,0),      4 },
            {Tuple.Create(5,1),      28 },
            {Tuple.Create(5,2),      40 },
            {Tuple.Create(5,3),      52 },
            {Tuple.Create(5,4),      64 },
            {Tuple.Create(5,5),      76 },
            {Tuple.Create(5,6),      88 },
            {Tuple.Create(5,7),      100 },
            {Tuple.Create(5,8),      112 },
            {Tuple.Create(5,9),      124 },
            {Tuple.Create(5,10),     136 },
            {Tuple.Create(5,11),      -1 },
            {Tuple.Create(5,12),      -1 },
            {Tuple.Create(5,13),      79 },
            {Tuple.Create(5,14),      -1 },
            {Tuple.Create(5,15),      103 },
            {Tuple.Create(5,16),      -1 },
            {Tuple.Create(5,17),      93 },
            {Tuple.Create(5,18),      105 },
            {Tuple.Create(5,19),      117 },
            {Tuple.Create(5,20),      140 },
            {Tuple.Create(6,0),       5 },
            {Tuple.Create(6,1),       17 },
            {Tuple.Create(6,2),       29 },
            {Tuple.Create(6,3),       -1 },
            {Tuple.Create(6,4),       -1 },
            {Tuple.Create(6,5),       -1 },
            {Tuple.Create(6,6),       -1 },
            {Tuple.Create(6,7),       53 },
            {Tuple.Create(6,8),       -1 },
            {Tuple.Create(6,9),       -1 },
            {Tuple.Create(6,10),       -1 },
            {Tuple.Create(6,11),       -1 },
            {Tuple.Create(6,12),      89 },
            {Tuple.Create(6,13),      101 },
            {Tuple.Create(6,14),      113 },
            {Tuple.Create(6,15),      91 },
            {Tuple.Create(6,16),      115 },
            {Tuple.Create(6,17),      127 },
            {Tuple.Create(6,18),      139 },
            {Tuple.Create(6,19),      129 },
            {Tuple.Create(6,20),      141 },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
        };

        public CorsairK70mkIIKeeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.CorsairK70MKII,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Corsair K70 RGB mkII"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();
            byte[] red = new byte[144];
            byte[] green = new byte[144];
            byte[] blue = new byte[144];

            foreach (KeyValuePair<Tuple<int, int>, int> item in KEYS_LAYOUTS)
            {
                if (item.Value == -1)
                    continue;
                var color = colorMatrix[item.Key.Item1, item.Key.Item2];
                red[item.Value] = color.R;
                green[item.Value] = color.G;
                blue[item.Value] = color.B;
            }
            _displayColorBytes.AddRange(red);
            _displayColorBytes.AddRange(green);
            _displayColorBytes.AddRange(blue);
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
            byte[] black = new byte[144];
            _displayColorBytes.AddRange(black);
            _displayColorBytes.AddRange(black);
            _displayColorBytes.AddRange(black);
        }
    }
}