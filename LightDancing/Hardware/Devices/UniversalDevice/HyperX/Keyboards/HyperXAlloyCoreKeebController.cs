using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Linq;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.HyperX.Keyboards
{
    internal class HyperXAlloyCoreKeebController: ILightingControl
    {
        private const int DEVICE_VID = 0x03f0;
        private const int DEVICE_PID = 0x098f;
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
                    HyperXAlloyCoreDevice keyboard = new HyperXAlloyCoreDevice(stream);
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

    public class HyperXAlloyCoreDevice: USBDeviceBase
    {
        const int MAX_REPORT_LENGTH = 65;

        private const int LED_COUNT = 102;

        public HyperXAlloyCoreDevice(HidStream deviceStream) : base(deviceStream)
        { }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            HyperXAlloyCoreKeeb keyboard = new HyperXAlloyCoreKeeb(_model);
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
                    Trace.WriteLine($"Failed to stream on HyperX Alloy Core Keyboard");
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
                USBDeviceType = USBDevices.HyperXAlloyCore,
                Name = "HyperX Alloy Core"
            };
        }
    }

    public class HyperXAlloyCoreKeeb: LightingBase
    {
        public HyperXAlloyCoreKeeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        { }

        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 17;

        private const int COLOR_ARRAY_COUNT = 93;

        private const int COLOR_PACKET_COUNT_SPLITE = 6;
        private const int COLOR_PACKET_COUNT = COLOR_PACKET_COUNT_SPLITE * 2;
        private const int COLOR_STREAM_COUNT = 60;
        private const int LAST_COLOR_STREAM_COUNT = 24;        
        private const int COLOR_STREAM_PACKET_COUNT = 65;

        private static readonly Dictionary<Tuple<int, int>, int> KEYS_LAYOUTS = new Dictionary<Tuple<int, int>, int>()
        {
            {Tuple.Create(0,0),      0 },
            {Tuple.Create(0,2),      1 },
            {Tuple.Create(0,3),      2 },
            {Tuple.Create(0,4),      3 },
            {Tuple.Create(0,5),      4 },
            {Tuple.Create(0,6),      5 },
            {Tuple.Create(0,7),      6 },
            {Tuple.Create(0,8),      7 },
            {Tuple.Create(0,9),      48 },
            {Tuple.Create(0,10),     49},
            {Tuple.Create(0,11),     50 },
            {Tuple.Create(0,12),     51 },
            {Tuple.Create(0,13),     52 },
            {Tuple.Create(0,14),     53 },
            {Tuple.Create(0,15),     54 },
            {Tuple.Create(0,16),     55 },
            {Tuple.Create(1,0),      8 },
            {Tuple.Create(1,1),      9 },//F1
            {Tuple.Create(1,2),      10 },
            {Tuple.Create(1,3),      11 },
            {Tuple.Create(1,4),      12 },
            {Tuple.Create(1,5),      13 },//F5
            {Tuple.Create(1,6),      14 },
            {Tuple.Create(1,7),      15 },
            {Tuple.Create(1,8),      16 },//F8
            {Tuple.Create(1,9),      56 },
            {Tuple.Create(1,10),     57 },//F9
            {Tuple.Create(1,11),     58 },
            {Tuple.Create(1,12),     59 },
            {Tuple.Create(1,13),     60  },
            {Tuple.Create(1,14),     61  },//print
            {Tuple.Create(1,15),     62 },
            {Tuple.Create(1,16),     63 },
            {Tuple.Create(2,0),      17 },
            {Tuple.Create(2,1),      18 },
            {Tuple.Create(2,2),      19 },
            {Tuple.Create(2,3),      20 },
            {Tuple.Create(2,4),      21 },
            {Tuple.Create(2,5),      22 },
            {Tuple.Create(2,6),      23 },
            {Tuple.Create(2,7),      24 },
            {Tuple.Create(2,8),      64 },
            {Tuple.Create(2,9),      65 },
            {Tuple.Create(2,10),     66 },
            {Tuple.Create(2,11),     67 },
            {Tuple.Create(2,12),     68 },
            {Tuple.Create(2,13),     69 },
            {Tuple.Create(2,14),     70 },
            {Tuple.Create(2,15),     71 },
            {Tuple.Create(2,16),     72 },
            {Tuple.Create(3,0),      25 },
            {Tuple.Create(3,1),      26 },
            {Tuple.Create(3,2),      27 },
            {Tuple.Create(3,3),      28 },
            {Tuple.Create(3,4),      29 },
            {Tuple.Create(3,5),      30 },
            {Tuple.Create(3,6),      31 },
            {Tuple.Create(3,7),      32 },
            {Tuple.Create(3,8),      73 },
            {Tuple.Create(3,9),      74 },
            {Tuple.Create(3,10),     75 },
            {Tuple.Create(3,11),     76 },
            {Tuple.Create(3,13),     78 },
            {Tuple.Create(4,0),      33 },
            {Tuple.Create(4,1),      35 },
            {Tuple.Create(4,2),      36 },
            {Tuple.Create(4,3),      37 },
            {Tuple.Create(4,4),      38 },
            {Tuple.Create(4,5),      39 },
            {Tuple.Create(4,6),      40 },
            {Tuple.Create(4,7),      79 },
            {Tuple.Create(4,8),      80 },
            {Tuple.Create(4,9),      81 },
            {Tuple.Create(4,10),     82 },
            {Tuple.Create(4,12),     84 },
            {Tuple.Create(4,14),     85 },
            {Tuple.Create(5,0),      41 },
            {Tuple.Create(5,1),      42 },
            {Tuple.Create(5,2),      43 },
            {Tuple.Create(5,6),      45 },
            {Tuple.Create(5,10),     86 },
            {Tuple.Create(5,11),     87 },
            {Tuple.Create(5,12),     88 },
            {Tuple.Create(5,13),     89 },
            {Tuple.Create(5,14),     90 },
            {Tuple.Create(5,15),     91 },
            {Tuple.Create(5,16),     92 },
        };

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                Type = LightingDevices.HyperXAlloyCore,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "HyperX Alloy Core"
            };
        }

        protected override void TurnOffLed() { }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            var colorArray = new byte[COLOR_ARRAY_COUNT];
            List<byte> red = colorArray.ToList();
            List<byte> green = colorArray.ToList();
            List<byte> blue = colorArray.ToList();
            foreach (KeyValuePair<Tuple<int, int>, int> item in KEYS_LAYOUTS)
            {
                var color = colorMatrix[item.Key.Item1, item.Key.Item2];
                red[item.Value] = color.R;
                green[item.Value] = color.G;
                blue[item.Value] = color.B;
            }
            red.PadListWithEmptyStringsToMultipleOfNumber(COLOR_PACKET_COUNT);
            green.PadListWithEmptyStringsToMultipleOfNumber(COLOR_PACKET_COUNT);
            blue.PadListWithEmptyStringsToMultipleOfNumber(COLOR_PACKET_COUNT);
            List<byte> allCommand = GetAllCommand(red, green, blue);
            SetColorStream(allCommand);
        }

        private static List<byte> GetAllCommand(List<byte> red, List<byte> green, List<byte> blue)
        {
            List<byte> allCommand = new List<byte>();

            for (int i = 0; i < red.Count() / COLOR_PACKET_COUNT; i++)
            {
                int countpack = i * COLOR_PACKET_COUNT;
                int countpack2 = countpack + COLOR_PACKET_COUNT_SPLITE;
                SetColor(green.GetRange(countpack, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
                SetColor(green.GetRange(countpack2, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
                SetColor(red.GetRange(countpack, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
                SetColor(red.GetRange(countpack2, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
                SetColor(blue.GetRange(countpack, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
                SetColor(blue.GetRange(countpack2, COLOR_PACKET_COUNT_SPLITE), ref allCommand);
            }

            return allCommand;
        }

        private static void SetColor(List<byte> color, ref List<byte> allCommand)
        {
            allCommand.AddRange(color);
            allCommand.Add(0);
            allCommand.Add(0);
        }

        private void SetColorStream(List<byte> allCommand)
        {
            for (int i = 0; i < allCommand.Count; i += COLOR_STREAM_COUNT)
            {
                int sendRange = (i == COLOR_STREAM_COUNT * 2) ? LAST_COLOR_STREAM_COUNT : COLOR_STREAM_COUNT;
                var commands = new List<byte>() { 0x00, 0xA2, Convert.ToByte(i / COLOR_STREAM_COUNT), 0x00, Convert.ToByte(sendRange) };
                int remainingCommands = allCommand.Count - i;
                int rangeSize = Math.Min(remainingCommands, COLOR_STREAM_COUNT);
                commands.AddRange(allCommand.GetRange(i, rangeSize));
                commands.PadListWithZeros(COLOR_STREAM_PACKET_COUNT);
                _displayColorBytes.AddRange(commands);
            }
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
