using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class QRGBWall : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 70; //39

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 72; //32

        private const int LED_COUNT = 300;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly int usbport;

        public QRGBWall(int usbport, HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.qRGBWall,
                Name = "qRGB Demo",
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

            for (int y = 0; y < KEYBOARD_YAXIS_COUNTS; y++) //39
            {
                ColorRGB color = colorMatrix[KEYBOARD_YAXIS_COUNTS - 1 - y, 0];
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }
            for (int x = 0; x < KEYBOARD_XAXIS_COUNTS - 2; x++) //30
            {
                ColorRGB color = colorMatrix[0, x + 1];
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }
            for (int y = 0; y < KEYBOARD_YAXIS_COUNTS; y++) //13
            {
                ColorRGB color = colorMatrix[y, KEYBOARD_XAXIS_COUNTS - 1]; //31
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }
            for (int x = 0; x < KEYBOARD_XAXIS_COUNTS - 2; x++) //30
            {
                ColorRGB color = colorMatrix[45, KEYBOARD_XAXIS_COUNTS - 2 - x]; //25, 30
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }
            for (int x = 26; x < 46; x++) //10, 22
            {
                ColorRGB color = colorMatrix[10, x]; //8
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
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

            for (int x = 0; x < LED_COUNT; x++)
            {
                byte[] grb = new byte[] { 0, 0, 0 };
                collectBytes.AddRange(grb);
            }

            displayColors.Clear();
            displayColors.AddRange(collectBytes);
            _displayColorBytes = displayColors;
        }
    }
}
