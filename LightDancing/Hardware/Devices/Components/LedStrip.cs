using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Hardware.Devices.Components
{
    internal class LedStrip : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 1;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private int KEYBOARD_XAXIS_COUNTS = 20;

        private List<int> LED_COUNTS;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly string usbport;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.LED1,      Tuple.Create(0, 0)  },
            { Keyboard.LED2,      Tuple.Create(0, 1)  },
            { Keyboard.LED3,      Tuple.Create(0, 2)  },
            { Keyboard.LED4,      Tuple.Create(0, 3)  },
            { Keyboard.LED5,      Tuple.Create(0, 4)  },
            { Keyboard.LED6,      Tuple.Create(0, 5)  },
            { Keyboard.LED7,      Tuple.Create(0, 6)  },
            { Keyboard.LED8,      Tuple.Create(0, 7)  },
            { Keyboard.LED9,      Tuple.Create(0, 8)  },
            { Keyboard.LED10,      Tuple.Create(0, 9)  },
            { Keyboard.LED11,      Tuple.Create(0, 10)  },
            { Keyboard.LED12,      Tuple.Create(0, 11)  },
            { Keyboard.LED13,      Tuple.Create(0, 12)  },
            { Keyboard.LED14,      Tuple.Create(0, 13)  },
            { Keyboard.LED15,      Tuple.Create(0, 14)  },
            { Keyboard.LED16,      Tuple.Create(0, 15)  },
            { Keyboard.LED17,      Tuple.Create(0, 16)  },
            { Keyboard.LED18,      Tuple.Create(0, 17)  },
            { Keyboard.LED19,      Tuple.Create(0, 18)  },
            { Keyboard.LED20,      Tuple.Create(0, 19)  },
        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6, Keyboard.LED7, Keyboard.LED8,
                                      Keyboard.LED9, Keyboard.LED10, Keyboard.LED11, Keyboard.LED12, Keyboard.LED13, Keyboard.LED14, Keyboard.LED15, Keyboard.LED16,
                                      Keyboard.LED17, Keyboard.LED18, Keyboard.LED19, Keyboard.LED20, 
            } },
        };

        public LedStrip(string usbport, HardwareModel hardwareModel, List<int> ledCounts) : base(KEYBOARD_YAXIS_COUNTS, ledCounts.Sum(), hardwareModel)
        {
            this.usbport = usbport;
            KEYBOARD_XAXIS_COUNTS = ledCounts.Sum();
            LED_COUNTS = ledCounts;
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
                Type = LightingDevices.SmartLedStrip,
                Name = "LedStrip" + KEYBOARD_XAXIS_COUNTS,
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
                new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6, Keyboard.LED7, Keyboard.LED8,
                                      Keyboard.LED9, Keyboard.LED10, Keyboard.LED11, Keyboard.LED12, Keyboard.LED13, Keyboard.LED14, Keyboard.LED15, Keyboard.LED16,
                                      Keyboard.LED17, Keyboard.LED18, Keyboard.LED19, Keyboard.LED20, },
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            foreach(var ledCount in LED_COUNTS)
            {
                if (ledCount == 20)
                {
                    for (int i = 0; i < ledCount; i++)
                    {
                        var color = colorMatrix[0, i];
                        byte[] grb = new byte[] { color.G, color.R, color.B };
                        collectBytes.AddRange(grb);
                    }
                }
                else
                {
                    for (int i = 0; i < ledCount; i++)
                    {
                        var color = colorMatrix[0, i];
                        byte[] grb = new byte[] { color.R, color.B, color.B };
                        collectBytes.AddRange(grb);
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

            foreach (var command in COMMAND_LAYOUT)
            {
                foreach (var key in command.Value)
                {
                    keyColor.Add(key, ColorRGB.Black());
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
