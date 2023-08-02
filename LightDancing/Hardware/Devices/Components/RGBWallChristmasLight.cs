using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class RGBWallChristmasLight : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 24;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 24;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly int usbport;

        public RGBWallChristmasLight(int usbport, HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.RGBWallChristmasLight,
                Name = "RGB Wall",
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

            for(int x = 0; x < KEYBOARD_XAXIS_COUNTS; x++)
            {
                if (x % 2 == 0)
                {
                    for (int y = KEYBOARD_YAXIS_COUNTS / 2 - 1; y >= 0; y--)
                    {
                        var color = colorMatrix[y, x];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }
                }
                else
                {
                    for (int y = 0; y < KEYBOARD_YAXIS_COUNTS / 2; y++)
                    {
                        var color = colorMatrix[y, x];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }
                }
            }

            for (int x = KEYBOARD_XAXIS_COUNTS - 1; x >= 0; x--)
            {
                if (x % 2 == 0)
                {
                    for (int y = KEYBOARD_YAXIS_COUNTS / 2 - 1; y >= 0; y--)
                    {
                        var color = colorMatrix[y + KEYBOARD_YAXIS_COUNTS / 2, x];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }
                }
                else
                {
                    for (int y = 0; y < KEYBOARD_YAXIS_COUNTS / 2; y++)
                    {
                        var color = colorMatrix[y + KEYBOARD_YAXIS_COUNTS / 2, x];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
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
