using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class AsrockLedStrip : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 1;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private int KEYBOARD_XAXIS_COUNTS;

        private int channel;

        public AsrockLedStrip(int XCount, int channel, HardwareModel usbModel) : base(KEYBOARD_YAXIS_COUNTS, XCount, usbModel)
        {
            this.channel = channel;
            KEYBOARD_XAXIS_COUNTS = XCount;
            _model = InitModel();
        }

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID + channel,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.SmartLedStrip,
                Name = "LedStrip" + KEYBOARD_XAXIS_COUNTS,
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>();
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
            {
                ColorRGB colorRGB = colorMatrix[0, i];
                collectBytes.Add(colorRGB.R);
                collectBytes.Add(colorRGB.G);
                collectBytes.Add(colorRGB.B);
            }
            _displayColorBytes = collectBytes;
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void TurnOffLed()
        {
            _displayColorBytes.Clear();
            for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
            {
                _displayColorBytes.Add(0x00);
                _displayColorBytes.Add(0x00);
                _displayColorBytes.Add(0x00);
            }
        }

    }

    public class AsrockLightFan : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 5;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 4;

        private int channel;
        private readonly Tuple<int, int>[] KEYS_LAYOUTS = new Tuple<int, int>[]
        {
                Tuple.Create(0, 3),
                Tuple.Create(0, 2),
                Tuple.Create(0, 1),
                Tuple.Create(0, 0),
                Tuple.Create(1, 0),
                Tuple.Create(2, 0),
                Tuple.Create(3, 0),
                Tuple.Create(4, 0),
                Tuple.Create(4, 1),
                Tuple.Create(4, 2),
                Tuple.Create(4, 3),
                Tuple.Create(3, 3),
                Tuple.Create(2, 3),
                Tuple.Create(1, 3),
                Tuple.Create(0, 3),
                Tuple.Create(0, 3),
        };

        public AsrockLightFan(int channel, HardwareModel usbModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, usbModel)
        {
            this.channel = channel;
            _model = InitModel();
        }

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID + channel,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.SmartLedStrip,
                Name = "Fan" + channel,
            };
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            foreach (Tuple<int, int> item in KEYS_LAYOUTS)
            {
                ColorRGB colorRGB = colorMatrix[item.Item1, item.Item2];
                collectBytes.Add(colorRGB.R);
                collectBytes.Add(colorRGB.G);
                collectBytes.Add(colorRGB.B);
            }
            _displayColorBytes = collectBytes;
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>();
        }

        protected override void TurnOffLed()
        {
            List<byte> collectBytes = new List<byte>();
            foreach (Tuple<int, int> item in KEYS_LAYOUTS)
            {
                collectBytes.Add(0x00);
                collectBytes.Add(0x00);
                collectBytes.Add(0x00);
            }
            _displayColorBytes = collectBytes;
        }

    }
}
