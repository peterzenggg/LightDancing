using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    class MiniHubLedStrip : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 1;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private readonly int _xAxisCount;

        private readonly List<byte> displayColors = new List<byte>();
        private readonly int usbport;

        public MiniHubLedStrip(int usbport, HardwareModel hardwareModel, int KEYBOARD_XAXIS_COUNTS) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
            _xAxisCount = KEYBOARD_XAXIS_COUNTS;
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
                Layouts = new MatrixLayouts() { Width = _xAxisCount, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID + usbport,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.LedStrip8,
                Name = "Led strip",
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>();
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            for(int i = 0; i < _xAxisCount; i++)
            {
                ColorRGB color = colorMatrix[0, i];
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }
            displayColors.Clear();
            displayColors.AddRange(collectBytes);
            _displayColorBytes = displayColors;
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void TurnOffLed()
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            for (int i = 0; i < _xAxisCount; i++)
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
