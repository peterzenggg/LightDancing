using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.SteelSeries.Keyboards
{
    internal class SteelSeriesApex5KeebController: ILightingControl
    {
        private const int DEVICE_VID = 0x1038;
        private const int DEVICE_PID = 0x161C;
        private const int MAX_FEATURE_LENGTH = 643;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    SteelSeriesApex5KeyboardDevice keyboard = new SteelSeriesApex5KeyboardDevice(stream);
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

    public class SteelSeriesApex5KeyboardDevice: USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 643;

        public SteelSeriesApex5KeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;

            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                //USBDeviceType = USBDevices.SteelSeriesApex5,
                Name = "SteelSeries Apex5 Keyboard"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return new List<LightingBase>() { new SteelSeriesApex5Keeb(_model) };
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"SteelSeriesApex5 Keyboard count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }
                List<byte> displayColors = _lightingBase[0].GetDisplayColors();
                while (displayColors.Count <= MAX_REPORT_LENGTH)
                {
                    displayColors.Add(0x00);
                }
                try
                {
                    ((HidStream)_deviceStream).SetFeature(displayColors.ToArray());
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on SteelSeriesApex5 Keyboard");
                }
            }
        }

        public override void TurnFwAnimationOn()
        {
        }
    }

    public class SteelSeriesApex5Keeb: LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 21;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 643;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<int, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<int, Tuple<int, int>>()
        {
            { 100,     Tuple.Create(1, 4) },
            { 50,      Tuple.Create(12, 3) },
            { 41,      Tuple.Create(0, 0) },
            { 58,      Tuple.Create(1, 0) },
            { 59,      Tuple.Create(2, 0) },
            { 60,      Tuple.Create(3, 0) },
            { 61,      Tuple.Create(4, 0) },
            { 62,      Tuple.Create(5, 0) },
            { 63,      Tuple.Create(6, 0) },
            { 64,      Tuple.Create(7, 0) },
            { 65,     Tuple.Create(8, 0) },
            { 66,     Tuple.Create(9, 0) },
            { 67,     Tuple.Create(10, 0) },
            {68,     Tuple.Create(11, 0) },
            { 69,     Tuple.Create(12, 0) },
            { 70,     Tuple.Create(14, 0) },
            { 71,     Tuple.Create(15, 0) },
            { 72,     Tuple.Create(16, 0) },
            { 53,     Tuple.Create(0, 1) },
            { 30,     Tuple.Create(1, 1) },
            { 31,     Tuple.Create(2, 1) },
            { 32,     Tuple.Create(3, 1) },
            { 33,     Tuple.Create(4, 1) },
            { 34,     Tuple.Create(5, 1) },
            { 35,     Tuple.Create(6, 1) },
            { 36,     Tuple.Create(7, 1) },
            { 37,     Tuple.Create(8, 1) },
            { 38,     Tuple.Create(9, 1) },
            { 39,     Tuple.Create(10, 1) },
            { 45,     Tuple.Create(11, 1) },
            { 46,     Tuple.Create(12, 1) },
            { 42,     Tuple.Create(13, 1) },
            { 73,     Tuple.Create(14, 1) },
            { 74,     Tuple.Create(15, 1) },
            { 75,     Tuple.Create(16, 1) },
            { 83,     Tuple.Create(17, 1) },
            { 84,     Tuple.Create(18, 1) },
            { 85,     Tuple.Create(19, 1) },
            { 86,     Tuple.Create(20, 1) },
            { 43,     Tuple.Create(0, 2) },
            { 20,     Tuple.Create(1, 2) },
            { 26,     Tuple.Create(2, 2) },
            { 8,      Tuple.Create(3, 2) },
            { 21,     Tuple.Create(4, 2) },
            { 23,     Tuple.Create(5, 2) },
            { 28,     Tuple.Create(6, 2) },
            { 24,     Tuple.Create(7, 2) },
            { 12,     Tuple.Create(8,2) },
            { 18,     Tuple.Create(9, 2) },
            { 19,     Tuple.Create(10, 2) },
            { 47,     Tuple.Create(11, 2) },
            { 48,     Tuple.Create(12, 2) },
            { 49,     Tuple.Create(13, 2) },
            { 76,     Tuple.Create(14, 2) },
            { 77,     Tuple.Create(15, 2) },
            { 78,     Tuple.Create(16, 2) },
            { 95,     Tuple.Create(17, 2) },
            { 96,     Tuple.Create(18, 2) },
            { 97,     Tuple.Create(19, 2) },
            { 87,     Tuple.Create(20, 2) },
            { 57,     Tuple.Create(0, 3) },
            { 4,     Tuple.Create(1, 3) },
            { 22,     Tuple.Create(2, 3) },
            { 7,      Tuple.Create(3, 3) },
            { 9,     Tuple.Create(4, 3) },
            { 10,     Tuple.Create(5, 3) },
            { 11,     Tuple.Create(6, 3) },
            { 13,     Tuple.Create(7, 3) },
            { 14,     Tuple.Create(8,3) },
            { 15,     Tuple.Create(9, 3) },
            { 51,     Tuple.Create(10, 3) },
            { 52,     Tuple.Create(11, 3) },
            { 40,     Tuple.Create(13, 3) },
            { 92,     Tuple.Create(17, 3) },
            { 93,     Tuple.Create(18, 3) },
            { 94,     Tuple.Create(19, 3) },
            { 225,     Tuple.Create(0, 4) },
            { 29,     Tuple.Create(1, 4) },
            { 27,     Tuple.Create(2, 4) },
            { 6,      Tuple.Create(3, 4) },
            { 25,     Tuple.Create(4, 4) },
            { 5,     Tuple.Create(5, 4) },
            { 17,     Tuple.Create(6, 4) },
            { 16,     Tuple.Create(7, 4) },
            { 54,     Tuple.Create(8,4) },
            { 55,     Tuple.Create(9, 4) },
            { 56,     Tuple.Create(10, 4) },
            { 229,     Tuple.Create(13, 4) },
            { 82,     Tuple.Create(15, 4) },
            { 89,     Tuple.Create(17, 4) },
            { 90,     Tuple.Create(18, 4) },
            { 91,     Tuple.Create(19, 4) },
            { 88,     Tuple.Create(20, 4) },
            { 224,     Tuple.Create(0, 5) },
            { 227,     Tuple.Create(1, 5) },
            { 226,     Tuple.Create(2, 5) },
            { 44,      Tuple.Create(6, 5) },
            { 230,     Tuple.Create(10, 5) },
            { 231,     Tuple.Create(11, 5) },
            { 240,     Tuple.Create(12, 5) },
            { 228,     Tuple.Create(13, 5) },
            { 80,      Tuple.Create(14,5) },
            { 81,      Tuple.Create(15, 5) },
            { 79,      Tuple.Create(16, 5) },
            { 98,      Tuple.Create(17, 5) },
            { 99,      Tuple.Create(18, 5) },
        };

        /// <summary>
        /// Need To 
        /// </summary>
        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        private readonly List<byte> DISPLAY_COLOR_BYTES_HEADER = new List<byte>() {
                0x00,
                0x3A,
                0x6A
        };

        public SteelSeriesApex5Keeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        /// <summary>
        /// Init the model
        /// MEMO: Device ID should be unique, the better way is get it from the firmware.
        /// </summary>
        /// <returns></returns>
        protected override LightingModel InitModel()
        {
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                //Type = LightingDevices.SteelSeriesApex5,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "SteelSeries Apex5 Keyboard"
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = DISPLAY_COLOR_BYTES_HEADER;
            foreach (var rowMappings in KEYS_LAYOUTS)
            {
                _displayColorBytes.Add(Convert.ToByte(rowMappings.Key));
                var posistion = rowMappings.Value;
                ColorRGB rgbColor = colorMatrix[posistion.Item2, posistion.Item1];
                _displayColorBytes.Add(rgbColor.R);
                _displayColorBytes.Add(rgbColor.G);
                _displayColorBytes.Add(rgbColor.B);
            }
        }

        protected override void TurnOffLed()
        {
            _displayColorBytes = DISPLAY_COLOR_BYTES_HEADER;
            foreach (var rowMappings in KEYS_LAYOUTS)
            {
                _displayColorBytes.Add(Convert.ToByte(rowMappings.Key));
                _displayColorBytes.Add(0);
                _displayColorBytes.Add(0);
                _displayColorBytes.Add(0);
            }
            while (_displayColorBytes.Count < MAX_REPORT_LENGTH)
            {
                _displayColorBytes.Add(0);
            }
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
