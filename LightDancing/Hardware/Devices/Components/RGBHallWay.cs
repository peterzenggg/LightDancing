using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class RGBHallWay : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 37;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 63;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly int usbport;

        private const int DOOR_COUNT = 10;

        public RGBHallWay(int usbport, HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.qRGBHallWay,
                Name = "qRGB Hallway",
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

            for (int x = 0; x < DOOR_COUNT; x++)
            {
                //Odd door (right)
                if (x % 2 == 0)
                {
                    //Left 0 ~ 18
                    for (int y = 0; y < 19; y++)
                    {
                        var color = colorMatrix[y, 27 - x * 3];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    //Mid 19 ~ 25
                    for (int y = 28; y < 34; y++)
                    {
                        var color = colorMatrix[18 + x * 2, y];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    //Right 26 ~ 44
                    for (int y = 18; y >= 0; y--)
                    {
                        var color = colorMatrix[y, 35 + x * 3];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    //Right Floor
                    for (int y = 0; y < 3; y++)
                    {
                        var color = colorMatrix[0, 36 + x * 3 + y];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }
                }
                else
                {
                    //Right 0 ~ 18
                    for (int y = 0; y < 19; y++)
                    {
                        var color = colorMatrix[y, 35 + x * 3];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    //Mid 19 ~ 25
                    for (int y = 33; y >= 28; y--)
                    {
                        var color = colorMatrix[18 + x * 2, y];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    //Left 26 ~ 44
                    for (int y = 18; y >= 0; y--)
                    {
                        var color = colorMatrix[y, 27 - x * 3];
                        byte[] rgb = new byte[] { color.R, color.G, color.B };
                        collectBytes.AddRange(rgb);
                    }

                    if (x != 9)
                    {
                        //Left Floor
                        for (int y = 0; y < 3; y++)
                        {
                            var color = colorMatrix[0, 26 - x * 3 - y];
                            byte[] rgb = new byte[] { color.R, color.G, color.B };
                            collectBytes.AddRange(rgb);
                        }
                    }
                    else
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            var color = colorMatrix[0, 0];
                            byte[] rgb = new byte[] { color.R, color.G, color.B };
                            collectBytes.AddRange(rgb);
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
