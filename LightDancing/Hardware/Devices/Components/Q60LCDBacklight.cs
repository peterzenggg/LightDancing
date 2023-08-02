using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class Q60LCDBacklight : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 9;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 5;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly string usbport;

        public Q60LCDBacklight(string usbport, HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
            this.usbport = usbport;
            _model = InitModel();
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
                DeviceID = _usbModel.DeviceID + usbport,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.Q60LCDBacklight,
                Name = "Q60 LCD Backlight",
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            for (int x = 0; x < KEYBOARD_XAXIS_COUNTS; x++)
            {
                if (x == 2)
                {
                    for (int y = 3; y < KEYBOARD_YAXIS_COUNTS; y++)
                    {
                        var color = colorMatrix[y, x];
                        byte[] grb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(grb);
                    }
                }
                else
                {
                    if (x % 2 == 0)
                    {
                        for (int y = 0; y < KEYBOARD_YAXIS_COUNTS; y++)
                        {
                            var color = colorMatrix[y, x];
                            byte[] grb = new byte[] { color.R, color.G, color.B };
                            collectBytes.AddRange(grb);
                        }
                    }
                    else
                    {
                        for (int y = KEYBOARD_YAXIS_COUNTS - 1; y >= 0; y--)
                        {
                            var color = colorMatrix[y, x];
                            byte[] grb = new byte[] { color.R, color.G, color.B };
                            collectBytes.AddRange(grb);
                        }
                    }
                }
            }

            displayColors.Clear();
            displayColors.AddRange(collectBytes);
            _displayColorBytes = displayColors;
        }

        /// <summary>
        /// Do nothing cuz there's no keylayout for serial keyboard
        /// </summary>
        /// <param name="keyColors"></param>
        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void TurnOffLed()
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            for (int x = 0; x < KEYBOARD_XAXIS_COUNTS; x++)
            {
                for (int y = 0; y < KEYBOARD_YAXIS_COUNTS; y++)
                {
                    byte[] grb = new byte[] { 0, 0, 0 };
                    collectBytes.AddRange(grb);
                }
            }

            displayColors.Clear();
            displayColors.AddRange(collectBytes);
            _displayColorBytes = displayColors;
        }
    }
}
