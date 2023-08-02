using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;

namespace LightDancing.Hardware.Devices
{
    internal class RedragonMK4Controller : ILightingControl
    {
        private const int DEVICE_VID = 0x3402;
        private const int DEVICE_PID = 0x0302;
        private const int MAX_FEATURE_LENGTH = 382;
        
        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);
            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    RedragonMK4Device keyboard = new RedragonMK4Device(stream);
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

    public class RedragonMK4Device : USBDeviceBase
    {
        private const int MAX_FEATURE_LENGTH = 382;
        public RedragonMK4Device(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            RedragonMK4 keyboard = new RedragonMK4(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        /// <summary>
        /// Send streaming signal will auto change from firmware light mode to software streaming mode
        /// Need to send protocol to go back to firmware animation mode
        /// </summary>
        /// <param name="process"></param>
        /// <param name="brightness"></param>
        protected override void SendToHardware(bool process, float brightness)
        {
            if (process)
            {
                _lightingBase[0].ProcessStreaming(false, brightness);
            }
            foreach (var device in _lightingBase)
            {
                byte[] output = device.GetDisplayColors().ToArray();
                try
                {
                    ((HidStream)_deviceStream).SetFeature(output);
                }
                catch { }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;

            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.Redragon_MK4,
                Name = "Redragon MK4"
            };
        }

        /// <summary>
        /// Turn firmware animation on
        /// </summary>
        public override void TurnFwAnimationOn()
        {
            byte[] commands = new byte[MAX_FEATURE_LENGTH];
            commands[0] = 0x08; //report id
            commands[1] = 0xf9;
            commands[2] = 0x01; //on -> turn on firmware animation
            try
            {
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on firmware animation on Redragon MK4");
            }
        }
    }

    public class RedragonMK4 : LightingBase
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
        private const int MAX_REPORT_LENGTH = 382;

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> KEYS_LAYOUTS = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.LED1,Tuple.Create(0, 0)},
            { Keyboard.LED2,Tuple.Create(1, 0)},
            { Keyboard.LED3,Tuple.Create(2, 0)},
            { Keyboard.LED4,Tuple.Create(3, 0)},
            { Keyboard.LED5,Tuple.Create(4, 0)},
            { Keyboard.LED6,Tuple.Create(5, 0)},
            { Keyboard.LED7,Tuple.Create(0, 1)},
            { Keyboard.LED8,Tuple.Create(1, 1)},
            { Keyboard.LED9,Tuple.Create(2, 1)},
            { Keyboard.LED10,Tuple.Create(3, 1)},
            { Keyboard.LED11,Tuple.Create(4, 1)},
            { Keyboard.LED12,Tuple.Create(5, 1)},
            { Keyboard.LED13,Tuple.Create(0, 2)},
            { Keyboard.LED14,Tuple.Create(1, 2)},
            { Keyboard.LED15,Tuple.Create(2, 2)},
            { Keyboard.LED16,Tuple.Create(3, 2)},
            { Keyboard.LED17,Tuple.Create(4, 2)},
            { Keyboard.LED18,Tuple.Create(5, 2)},
            { Keyboard.LED19,Tuple.Create(0, 3)},
            { Keyboard.LED20,Tuple.Create(1, 3)},
            { Keyboard.LED21,Tuple.Create(2, 3)},
            { Keyboard.LED22,Tuple.Create(3, 3)},
            { Keyboard.LED23,Tuple.Create(4, 3)},
            { Keyboard.LED24,Tuple.Create(5, 3)},
            { Keyboard.LED25,Tuple.Create(0, 4)},
            { Keyboard.LED26,Tuple.Create(1, 4)},
            { Keyboard.LED27,Tuple.Create(2, 4)},
            { Keyboard.LED28,Tuple.Create(3, 4)},
            { Keyboard.LED29,Tuple.Create(4, 4)},
            { Keyboard.LED30,Tuple.Create(5, 4)},
            { Keyboard.LED31,Tuple.Create(0, 5)},
            { Keyboard.LED32,Tuple.Create(1, 5)},
            { Keyboard.LED33,Tuple.Create(2, 5)},
            { Keyboard.LED34,Tuple.Create(3, 5)},
            { Keyboard.LED35,Tuple.Create(4, 5)},
            { Keyboard.LED36,Tuple.Create(5, 5)},
            { Keyboard.LED37,Tuple.Create(0, 6)},
            { Keyboard.LED38,Tuple.Create(1, 6)},
            { Keyboard.LED39,Tuple.Create(2, 6)},
            { Keyboard.LED40,Tuple.Create(3, 6)},
            { Keyboard.LED41,Tuple.Create(4, 6)},
            { Keyboard.LED42,Tuple.Create(5, 6)},
            { Keyboard.LED43,Tuple.Create(0, 7)},
            { Keyboard.LED44,Tuple.Create(1, 7)},
            { Keyboard.LED45,Tuple.Create(2, 7)},
            { Keyboard.LED46,Tuple.Create(3, 7)},
            { Keyboard.LED47,Tuple.Create(4, 7)},
            { Keyboard.LED48,Tuple.Create(5, 7)},
            { Keyboard.LED49,Tuple.Create(0, 8)},
            { Keyboard.LED50,Tuple.Create(1, 8)},
            { Keyboard.LED51,Tuple.Create(2, 8)},
            { Keyboard.LED52,Tuple.Create(3, 8)},
            { Keyboard.LED53,Tuple.Create(4, 8)},
            { Keyboard.LED54,Tuple.Create(5, 8)},
            { Keyboard.LED55,Tuple.Create(0, 9)},
            { Keyboard.LED56,Tuple.Create(1, 9)},
            { Keyboard.LED57,Tuple.Create(2, 9)},
            { Keyboard.LED58,Tuple.Create(3, 9)},
            { Keyboard.LED59,Tuple.Create(4, 9)},
            { Keyboard.LED60,Tuple.Create(5, 9)},
            { Keyboard.LED61,Tuple.Create(0, 10)},
            { Keyboard.LED62,Tuple.Create(1, 10)},
            { Keyboard.LED63,Tuple.Create(2, 10)},
            { Keyboard.LED64,Tuple.Create(3, 10)},
            { Keyboard.LED65,Tuple.Create(4, 10)},
            { Keyboard.LED66,Tuple.Create(5, 10)},
            { Keyboard.LED67,Tuple.Create(0, 11)},
            { Keyboard.LED68,Tuple.Create(1, 11)},
            { Keyboard.LED69,Tuple.Create(2, 11)},
            { Keyboard.LED70,Tuple.Create(3, 11)},
            { Keyboard.LED71,Tuple.Create(4, 11)},
            { Keyboard.LED72,Tuple.Create(5, 11)},
            { Keyboard.LED73,Tuple.Create(0, 12)},
            { Keyboard.LED74,Tuple.Create(1, 12)},
            { Keyboard.LED75,Tuple.Create(2, 12)},
            { Keyboard.LED76,Tuple.Create(3, 12)},
            { Keyboard.LED77,Tuple.Create(4, 12)},
            { Keyboard.LED78,Tuple.Create(5, 12)},
            { Keyboard.LED79,Tuple.Create(0, 13)},
            { Keyboard.LED80,Tuple.Create(1, 13)},
            { Keyboard.LED81,Tuple.Create(2, 13)},
            { Keyboard.LED82,Tuple.Create(3, 13)},
            { Keyboard.LED83,Tuple.Create(4, 13)},
            { Keyboard.LED84,Tuple.Create(5, 13)},
            { Keyboard.LED85,Tuple.Create(0, 14)},
            { Keyboard.LED86,Tuple.Create(1, 14)},
            { Keyboard.LED87,Tuple.Create(2, 14)},
            { Keyboard.LED88,Tuple.Create(3, 14)},
            { Keyboard.LED89,Tuple.Create(4, 14)},
            { Keyboard.LED90,Tuple.Create(5, 14)},
            { Keyboard.LED91,Tuple.Create(0, 15)},
            { Keyboard.LED92,Tuple.Create(1, 15)},
            { Keyboard.LED93,Tuple.Create(2, 15)},
            { Keyboard.LED94,Tuple.Create(3, 15)},
            { Keyboard.LED95,Tuple.Create(4, 15)},
            { Keyboard.LED96,Tuple.Create(5, 15)},
            { Keyboard.LED97,Tuple.Create(0, 16)},
            { Keyboard.LED98,Tuple.Create(1, 16)},
            { Keyboard.LED99,Tuple.Create(2, 16)},
            { Keyboard.LED100,Tuple.Create(3, 16)},
            { Keyboard.LED101,Tuple.Create(4, 16)},
            { Keyboard.LED102,Tuple.Create(5, 16)},
            { Keyboard.LED103,Tuple.Create(0, 17)},
            { Keyboard.LED104,Tuple.Create(1, 17)},
            { Keyboard.LED105,Tuple.Create(2, 17)},
            { Keyboard.LED106,Tuple.Create(3, 17)},
            { Keyboard.LED107,Tuple.Create(4, 17)},
            { Keyboard.LED108,Tuple.Create(5, 17)},
            { Keyboard.LED109,Tuple.Create(0, 18)},
            { Keyboard.LED110,Tuple.Create(1, 18)},
            { Keyboard.LED111,Tuple.Create(2, 18)},
            { Keyboard.LED112,Tuple.Create(3, 18)},
            { Keyboard.LED113,Tuple.Create(4, 18)},
            { Keyboard.LED114,Tuple.Create(5, 18)},
            { Keyboard.LED115,Tuple.Create(0, 19)},
            { Keyboard.LED116,Tuple.Create(1, 19)},
            { Keyboard.LED117,Tuple.Create(2, 19)},
            { Keyboard.LED118,Tuple.Create(3, 19)},
            { Keyboard.LED119,Tuple.Create(4, 19)},
            { Keyboard.LED120,Tuple.Create(5, 19)},
            { Keyboard.LED121,Tuple.Create(0, 20)},
            { Keyboard.LED122,Tuple.Create(1, 20)},
            { Keyboard.LED123,Tuple.Create(2, 20)},
            { Keyboard.LED124,Tuple.Create(3, 20)},
            { Keyboard.LED125,Tuple.Create(4, 20)},
            { Keyboard.LED126,Tuple.Create(5, 20)},
        };

        public RedragonMK4(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
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
                DeviceID = _usbModel.DeviceID,
                Type = LightingDevices.Redragon_MK4,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Redragon MK4"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){   Keyboard.LED1,       Keyboard.LED2,       Keyboard.LED3,       Keyboard.LED4,       Keyboard.LED5,
                                        Keyboard.LED6,       Keyboard.LED7,       Keyboard.LED8,       Keyboard.LED9,       Keyboard.LED10,
                                        Keyboard.LED11,      Keyboard.LED12,      Keyboard.LED13,      Keyboard.LED14,      Keyboard.LED15,
                                        Keyboard.LED16,      Keyboard.LED17,      Keyboard.LED18,      Keyboard.LED19,      Keyboard.LED20,
                                        Keyboard.LED21,      Keyboard.LED22,      Keyboard.LED23,      Keyboard.LED24,      Keyboard.LED25,
                                        Keyboard.LED26,      Keyboard.LED27,      Keyboard.LED28,      Keyboard.LED29,      Keyboard.LED30,
                                        Keyboard.LED31,      Keyboard.LED32,      Keyboard.LED33,      Keyboard.LED34,      Keyboard.LED35,
                                        Keyboard.LED36,      Keyboard.LED37,      Keyboard.LED38,      Keyboard.LED39,      Keyboard.LED40,
                                        Keyboard.LED41,      Keyboard.LED42,      Keyboard.LED43,      Keyboard.LED44,      Keyboard.LED45,
                                        Keyboard.LED46,      Keyboard.LED47,      Keyboard.LED48,      Keyboard.LED49,      Keyboard.LED50,
                                        Keyboard.LED51,      Keyboard.LED52,      Keyboard.LED53,      Keyboard.LED54,      Keyboard.LED55,
                                        Keyboard.LED56,      Keyboard.LED57,      Keyboard.LED58,      Keyboard.LED59,      Keyboard.LED60,
                                        Keyboard.LED61,      Keyboard.LED62,      Keyboard.LED63,      Keyboard.LED64,      Keyboard.LED65,
                                        Keyboard.LED66,      Keyboard.LED67,      Keyboard.LED68,      Keyboard.LED69,      Keyboard.LED70,
                                        Keyboard.LED71,      Keyboard.LED72,      Keyboard.LED73,      Keyboard.LED74,      Keyboard.LED75,
                                        Keyboard.LED76,      Keyboard.LED77,      Keyboard.LED78,      Keyboard.LED79,      Keyboard.LED80,
                                        Keyboard.LED81,      Keyboard.LED82,      Keyboard.LED83,      Keyboard.LED84,      Keyboard.LED85,
                                        Keyboard.LED86,      Keyboard.LED87,      Keyboard.LED88,      Keyboard.LED89,      Keyboard.LED90,
                                        Keyboard.LED91,      Keyboard.LED92,      Keyboard.LED93,      Keyboard.LED94,      Keyboard.LED95,
                                        Keyboard.LED96,      Keyboard.LED97,      Keyboard.LED98,      Keyboard.LED99,      Keyboard.LED100,
                                        Keyboard.LED100,     Keyboard.LED102,     Keyboard.LED103,     Keyboard.LED104,     Keyboard.LED105,
                                        Keyboard.LED106,     Keyboard.LED107,     Keyboard.LED108,     Keyboard.LED109,     Keyboard.LED110,
                                        Keyboard.LED111,     Keyboard.LED112,     Keyboard.LED113,     Keyboard.LED114,     Keyboard.LED115,
                                        Keyboard.LED116,     Keyboard.LED117,     Keyboard.LED118,     Keyboard.LED119,     Keyboard.LED120,
                                        Keyboard.LED121,     Keyboard.LED122,     Keyboard.LED123,     Keyboard.LED124,     Keyboard.LED125,
                                        Keyboard.LED126,

                },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];
            collectBytes[0] = 0x08;
            collectBytes[1] = 0x0A;
            collectBytes[2] = 0x7A;
            collectBytes[3] = 0x01;

            int Count = 0;
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            foreach (var key in KEYS_LAYOUTS)
            {
                var value = key.Value;
                ColorRGB color = colorMatrix[value.Item1, value.Item2];
                collectBytes[Count * 3 + 4] = color.R;
                collectBytes[Count * 3 + 5] = color.G;
                collectBytes[Count * 3 + 6] = color.B;
                Count++;
            }
            _displayColorBytes.AddRange(collectBytes);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            _displayColorBytes = new List<byte>();
            byte[] collectBytes = new byte[MAX_REPORT_LENGTH];
            collectBytes[0] = 0x08;
            collectBytes[1] = 0x0A;
            collectBytes[2] = 0x7A;
            collectBytes[3] = 0x01;

            _displayColorBytes.AddRange(collectBytes);
        }
    }
}
