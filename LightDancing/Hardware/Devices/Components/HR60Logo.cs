﻿using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Components
{
    internal class HR60Logo : LightingBase
    {
        /// <summary>
        /// The Y-Axis count of led board
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 4;

        /// <summary>
        /// The X-Axis count of led board
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 3;

        private readonly List<byte> displayColors = new List<byte>();

        private readonly string usbport;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.LED1,      Tuple.Create(1, 0)  },
            { Keyboard.LED2,      Tuple.Create(2, 0)  },
            { Keyboard.LED3,      Tuple.Create(3, 1)  },
            { Keyboard.LED4,      Tuple.Create(2, 2)  },
            { Keyboard.LED5,      Tuple.Create(1, 2)  },
            { Keyboard.LED6,      Tuple.Create(0, 1)  },

        };

        /// <summary>
        /// Key = Command Sequence, Value = keys in each command
        /// </summary>
        private readonly Dictionary<int, List<Keyboard>> COMMAND_LAYOUT = new Dictionary<int, List<Keyboard>>()
        {
            {1, new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6
            } },
        };

        public HR60Logo(string usbport, HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.HR60Logo,
                Name = "HR60 Logo",
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
                new List<Keyboard>(){ Keyboard.LED1, Keyboard.LED2, Keyboard.LED3, Keyboard.LED4, Keyboard.LED5, Keyboard.LED6 },
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            foreach (var command in COMMAND_LAYOUT)
            {
                foreach (var key in command.Value)
                {
                    var thiskey = KEYS_LAYOUTS[key];
                    var color = colorMatrix[thiskey.Item1, thiskey.Item2];
                    keyColor.Add(key, color);
                    byte[] grb = new byte[] { color.G, color.R, color.B };
                    collectBytes.AddRange(grb);
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
