using System;
using System.Collections.Generic;
using HidSharp;
using LightDancing.Common;
using LightDancing.Colors;
using LightDancing.Enums;
using System.Threading;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class CNVSController : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0BFF"; //change to VID_3402&PID_0BFF for right hand side CNVS
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<Tuple<SerialStream, string>> streams = serialDetector.GetSerialStreamsAndSerialID(DEVICE_PID);

            List<USBDeviceBase> hardwares = null;

            if (streams != null && streams.Count > 0)
            {
                if(hardwares == null)
                {
                    hardwares = new List<USBDeviceBase>();
                }

                foreach (Tuple<SerialStream, string> stream in streams)
                {
                    stream.Item1.BaudRate = BAUDRATES;
                    CNVSDevice device = new CNVSDevice(stream.Item1, stream.Item2);
                    hardwares.Add(device);
                }
            }
            return hardwares;
        }
    }

    public class CNVSDevice : USBDeviceBase
    {
        public bool UpdateRequested { get; set; }

        private bool _isFwAnimationON = true;

        private bool _updating = false;

        private Tuple<bool, string> result;

        public CNVSDevice(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CNVS cnvs = new CNVS(_model);
            hardwares.Add(cnvs);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_isFwAnimationON)
            {
                TurnFwAnimationOFF();
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"CNVS device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }

                byte[] commands = _lightingBase[0].GetDisplayColors().ToArray();
                try
                {
                    _deviceStream.Write(commands);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on CNVS");
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = _serialID,
                USBDeviceType = USBDevices.CNVS,
                Name = "HYTE qRGB CNVS"
            };
        }

        /// <summary>
        /// 2023/02/16 Protocol
        /// FF DD 02 __ __ __ __ => fw version large, mid, minor, hardware version
        /// </summary>
        /// <returns></returns>
        private string GetFirmwareVersion()
        {
            byte[] commands = new byte[] { 0xff, 0xdd, 0x02 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { }
            Thread.Sleep(20);
            byte[] command = new byte[7];
            try
            {
                _deviceStream.Read(command);
            }
            catch { }
            return string.Format("{0}.{1}.{2}.{3}", command[3], command[4], command[5], command[6]);
        }

        public override void TurnFwAnimationOn()
        {
            List<byte> collectBytes = new List<byte>() { 0xff, 0xdc, 0x05, 0x01 };
            try
            {
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
            }
            catch { }
            _isFwAnimationON = true;
        }

        private void TurnFwAnimationOFF()
        {
            List<byte> collectBytes = new List<byte>() { 0xff, 0xdc, 0x05, 0x00 };
            try
            {
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
                _isFwAnimationON = false;
            }
            catch { }
        }
    }

    /// <summary>
    /// 2023/02/18 Change to command 50, 50, 50, 100 fw
    /// 2022/11/14 Change to 50 leds layout
    /// Control box right hand side version
    /// </summary>
    public class CNVS : LightingBase
    {
        /// <summary>
        /// The keyboard of Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 7;

        /// <summary>
        /// The keyboard of X-Axis count
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 20;

        public CNVS(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.CNVS,
                Name = "HYTE qRGB CNVS"
            };
        }

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> ColorToLayout = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.RightLED1, Tuple.Create(1, 19)},
            { Keyboard.RightLED2, Tuple.Create(2, 19)},
            { Keyboard.RightLED3, Tuple.Create(3, 19)},
            { Keyboard.RightLED4, Tuple.Create(4, 19)},
            { Keyboard.RightLED5, Tuple.Create(5, 19)},

            { Keyboard.BottomLED20, Tuple.Create(6, 19)},
            { Keyboard.BottomLED19, Tuple.Create(6, 18)},
            { Keyboard.BottomLED18, Tuple.Create(6, 17)},
            { Keyboard.BottomLED17, Tuple.Create(6, 16)},
            { Keyboard.BottomLED16, Tuple.Create(6, 15)},
            { Keyboard.BottomLED15, Tuple.Create(6, 14)},
            { Keyboard.BottomLED14, Tuple.Create(6, 13)},
            { Keyboard.BottomLED13, Tuple.Create(6, 12)},
            { Keyboard.BottomLED12, Tuple.Create(6, 11)},
            { Keyboard.BottomLED11, Tuple.Create(6, 10)},
            { Keyboard.BottomLED10, Tuple.Create(6, 9)},
            { Keyboard.BottomLED9, Tuple.Create(6, 8)},
            { Keyboard.BottomLED8, Tuple.Create(6, 7)},
            { Keyboard.BottomLED7, Tuple.Create(6, 6)},
            { Keyboard.BottomLED6, Tuple.Create(6, 5)},
            { Keyboard.BottomLED5, Tuple.Create(6, 4)},
            { Keyboard.BottomLED4, Tuple.Create(6, 3)},
            { Keyboard.BottomLED3, Tuple.Create(6, 2)},
            { Keyboard.BottomLED2, Tuple.Create(6, 1)},
            { Keyboard.BottomLED1, Tuple.Create(6, 0)},

            { Keyboard.LeftLED5, Tuple.Create(5, 0)},
            { Keyboard.LeftLED4, Tuple.Create(4, 0)},
            { Keyboard.LeftLED3, Tuple.Create(3, 0)},
            { Keyboard.LeftLED2, Tuple.Create(2, 0)},
            { Keyboard.LeftLED1, Tuple.Create(1, 0)},

            { Keyboard.TopLED1, Tuple.Create(0, 0)},
            { Keyboard.TopLED2, Tuple.Create(0, 1)},
            { Keyboard.TopLED3, Tuple.Create(0, 2)},
            { Keyboard.TopLED4, Tuple.Create(0, 3)},
            { Keyboard.TopLED5, Tuple.Create(0, 4)},
            { Keyboard.TopLED6, Tuple.Create(0, 5)},
            { Keyboard.TopLED7, Tuple.Create(0, 6)},
            { Keyboard.TopLED8, Tuple.Create(0, 7)},
            { Keyboard.TopLED9, Tuple.Create(0, 8)},
            { Keyboard.TopLED10, Tuple.Create(0, 9)},
            { Keyboard.TopLED11, Tuple.Create(0, 10)},
            { Keyboard.TopLED12, Tuple.Create(0, 11)},
            { Keyboard.TopLED13, Tuple.Create(0, 12)},
            { Keyboard.TopLED14, Tuple.Create(0, 13)},
            { Keyboard.TopLED15, Tuple.Create(0, 14)},
            { Keyboard.TopLED16, Tuple.Create(0, 15)},
            { Keyboard.TopLED17, Tuple.Create(0, 16)},
            { Keyboard.TopLED18, Tuple.Create(0, 17)},
            { Keyboard.TopLED19, Tuple.Create(0, 18)},
            { Keyboard.TopLED20, Tuple.Create(0, 19)}
        };

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>
            {
                new List<Keyboard>()
                {
                    Keyboard.RightLED1,
                    Keyboard.RightLED2,
                    Keyboard.RightLED3,
                    Keyboard.RightLED4,
                    Keyboard.RightLED5,

                    Keyboard.BottomLED20,
                    Keyboard.BottomLED19,
                    Keyboard.BottomLED18,
                    Keyboard.BottomLED17,
                    Keyboard.BottomLED16,
                    Keyboard.BottomLED15,
                    Keyboard.BottomLED14,
                    Keyboard.BottomLED13,
                    Keyboard.BottomLED12,
                    Keyboard.BottomLED11,
                    Keyboard.BottomLED10,
                    Keyboard.BottomLED9,
                    Keyboard.BottomLED8,
                    Keyboard.BottomLED7,
                    Keyboard.BottomLED6,
                    Keyboard.BottomLED5,
                    Keyboard.BottomLED4,
                    Keyboard.BottomLED3,
                    Keyboard.BottomLED2,
                    Keyboard.BottomLED1,

                    Keyboard.LeftLED5,
                    Keyboard.LeftLED4,
                    Keyboard.LeftLED3,
                    Keyboard.LeftLED2,
                    Keyboard.LeftLED1,

                    Keyboard.TopLED1,
                    Keyboard.TopLED2,
                    Keyboard.TopLED3,
                    Keyboard.TopLED4,
                    Keyboard.TopLED5,
                    Keyboard.TopLED6,
                    Keyboard.TopLED7,
                    Keyboard.TopLED8,
                    Keyboard.TopLED9,
                    Keyboard.TopLED10,
                    Keyboard.TopLED11,
                    Keyboard.TopLED12,
                    Keyboard.TopLED13,
                    Keyboard.TopLED14,
                    Keyboard.TopLED15,
                    Keyboard.TopLED16,
                    Keyboard.TopLED17,
                    Keyboard.TopLED18,
                    Keyboard.TopLED19,
                    Keyboard.TopLED20,
                }
            };
        }

        /// <summary>
        /// Byte 4(0x00) & 5(0x32) is for led count = 50 led
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x02, 0x01, 0x00, 0x32, 0x00 };
            foreach (KeyValuePair<Keyboard, Tuple<int, int>> led in ColorToLayout)
            {
                ColorRGB color = colorMatrix[led.Value.Item1, led.Value.Item2];
                keyColor.Add(led.Key, color);
                color.CNVSBrightnessAdjustment();
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }

            _displayColorBytes.AddRange(collectBytes);
        }

        /// <summary>
        /// Byte 4(0x00) & 5(0x32) is for led count = 50 led
        /// </summary>
        protected override void TurnOffLed()
        {
            _displayColorBytes = new List<byte>();
            List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x02, 0x01, 0x00, 0x32, 0x00 };
            foreach (KeyValuePair<Keyboard, Tuple<int, int>> led in ColorToLayout)
            {
                byte[] dark = new byte[] { 0x00, 0x00, 0x00 };
                collectBytes.AddRange(dark);
            }
            _displayColorBytes.AddRange(collectBytes);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            /*Not implemented for CNVS since current UI didn't have this feature*/
        }
    }
}
