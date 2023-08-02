using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.Razer.Keyboards
{
    internal class RazerBlackwidowV3miniKeyboardController : ILightingControl
    {
        private const int DEVICE_VID = 0x1532;
        private const int DEVICE_PID = 0x0258;
        private const int MAX_FEATURE_LENGTH = 91;

        public List<USBDeviceBase> InitDevices()
        {
            HidDetector hidDetector = new HidDetector();
            List<HidStream> streams = hidDetector.GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    RazerBlackwidowV3miniKeyboardDevice keyboard = new RazerBlackwidowV3miniKeyboardDevice(stream);
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

    public class RazerBlackwidowV3miniKeyboardDevice : USBDeviceBase
    {
        private const int MAX_REPORT_LENGTH = 91;
        private const int LED_COUNT = 80;

        public RazerBlackwidowV3miniKeyboardDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            RazerBlackwidowV3miniKeyboard keyboard = new RazerBlackwidowV3miniKeyboard(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;

            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.RazerBlackwidowV3MiniKeyboard,
                Name = "Razer Blackwidow V3 mini"
            };
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"Razer Blackwidow V3 Keyboard device count error");
            }
            else
            {
                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }
                List<byte> displayColors = _lightingBase[0].GetDisplayColors();

                for (int i = 0; i < displayColors.Count / MAX_REPORT_LENGTH; i++)
                {
                    byte[] result = displayColors.GetRange(MAX_REPORT_LENGTH * i, MAX_REPORT_LENGTH).ToArray();
                    try
                    {
                        ((HidStream)_deviceStream).SetFeature(result);
                    }
                    catch
                    {
                        Trace.WriteLine($"False to streaming on Razer Blackwidow v3 mini Keyboard");
                    }
                }
            }
        }

        public override void TurnFwAnimationOn()
        {
            try
            {
                byte[] commands = new byte[MAX_REPORT_LENGTH];
                commands[2] = 0x1F;
                commands[6] = 0x03;
                commands[7] = 0x03;
                commands[10] = 0x08;
                commands[89] = 0x08;
                ((HidStream)_deviceStream).SetFeature(commands);
                commands = new byte[91];
                commands[2] = 0x1F;
                commands[6] = 0x0c;
                commands[7] = 0x0f;
                commands[8] = 0x82;
                commands[9] = 0x01;
                commands[10] = 0x05;
                commands[89] = 0x85;
                ((HidStream)_deviceStream).SetFeature(commands);
                commands = new byte[91];
                commands[2] = 0x1F;
                commands[6] = 0x06;
                commands[7] = 0x0f;
                commands[8] = 0x02;
                commands[9] = 0x01;
                commands[11] = 0x03;
                commands[89] = 0x09;
                ((HidStream)_deviceStream).SetFeature(commands);
                commands = new byte[91];
                commands[2] = 0x1F;
                commands[6] = 0x02;
                commands[8] = 0x84;
                commands[89] = 0x86;
                ((HidStream)_deviceStream).SetFeature(commands);
            }
            catch
            {
                Trace.WriteLine($"False to turn on UI control on BlackWidowmini Keyboard");
            }
        }
    }

    public class RazerBlackwidowV3miniKeyboard : LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 5;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 16;
        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_REPORT_LENGTH = 91;

        public RazerBlackwidowV3miniKeyboard(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.RazerBlackwidowV3MiniKeyboard,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Razer Blackwidow V3 mini"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>();
        }

        /// <summary>
        /// Each command has 91 bytes, and total is 5 commands
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();
            for (int i = 0; i < KEYBOARD_YAXIS_COUNTS; i++)
            {
                List<ColorRGB> colorArray = new List<ColorRGB>();
                for (int t = 0; t < KEYBOARD_XAXIS_COUNTS; t++)
                {
                    colorArray.Add(colorMatrix[i, t]);
                }
                CollectCommand(_displayColorBytes, i, colorArray);
            }
        }

        private void CollectCommand(List<byte> listbyte, int idx, List<ColorRGB> colorArray)
        {
            byte[] commands = new byte[MAX_REPORT_LENGTH];
            commands[0] = 0x00;
            commands[1] = 0x00;
            commands[2] = 0x1F;
            commands[3] = 0x00;
            commands[4] = 0x00;
            commands[5] = 0x00;
            commands[6] = 0x35;
            commands[7] = 0x0F;
            commands[8] = 0x03;
            commands[11] = Convert.ToByte(idx);
            commands[13] = 0x0F;
            int colorIndex = 0;
            foreach (ColorRGB Color in colorArray)
            {
                commands[(colorIndex * 3) + 14] = Color.R;
                commands[(colorIndex * 3) + 15] = Color.G;
                commands[(colorIndex * 3) + 16] = Color.B;
                colorIndex++;
            }
            commands[89] = Methods.CalculateRazerAccessByte(commands);
            listbyte.AddRange(commands);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        /// <summary>
        /// Turn off Leds, write 0s
        /// </summary>
        protected override void TurnOffLed()
        {
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            _displayColorBytes = new List<byte>();
            for (int i = 0; i < KEYBOARD_YAXIS_COUNTS; i++)
            {
                byte[] commands = new byte[MAX_REPORT_LENGTH];
                commands[0] = 0x00;
                commands[1] = 0x00;
                commands[2] = 0x1F;
                commands[3] = 0x00;
                commands[4] = 0x00;
                commands[5] = 0x00;
                commands[6] = 0x35;
                commands[7] = 0x0F;
                commands[8] = 0x03;
                commands[11] = Convert.ToByte(i);
                commands[13] = 0x0F;
                commands[89] = Methods.CalculateRazerAccessByte(commands);
                _displayColorBytes.AddRange(commands);
            }
        }
    }
}