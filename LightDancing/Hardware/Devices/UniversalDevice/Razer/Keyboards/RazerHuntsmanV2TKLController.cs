using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.Razer.Keyboards
{
    internal class RazerHuntsmanV2TKLController : ILightingControl
    {
        private const int DEVICE_VID = 0x1532;
        private const int DEVICE_PID = 0x026B;
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
                    HuntsmanV2TKLDevice keyboard = new HuntsmanV2TKLDevice(stream);
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

    public class HuntsmanV2TKLDevice : USBDeviceBase
    {
        const int MAX_REPORT_LENGTH = 91;

        private const int LED_COUNT = 102;

        public HuntsmanV2TKLDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            HuntsmanV2TKLKeeb keyboard = new HuntsmanV2TKLKeeb(_model);
            hardwares.Add(keyboard);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (process)
            {
                foreach (var device in _lightingBase)
                {
                    device.ProcessStreaming(false, brightness);
                }
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
                    Trace.WriteLine($"False to streaming on Razer Huntsman V2 TKL");
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.RazerHuntsmanV2TKL,
                Name = "Razer Huntsman V2 TKL"
            };
        }

        public override void TurnFwAnimationOn()
        {
            byte[] packet = new byte[91];
            packet[2] = 0x1F;
            packet[6] = 0x03;
            packet[7] = 0x03;
            packet[10] = 0x08;
            packet[89] = 0x08;
            try
            {
                ((HidStream)_deviceStream).SetFeature(packet);
            }
            catch { }

            packet = new byte[91];
            packet[2] = 0x1F;
            packet[6] = 0x0c;
            packet[7] = 0x0f;
            packet[8] = 0x82;
            packet[9] = 0x01;
            packet[10] = 0x05;
            packet[89] = 0x85;
            try
            {
                ((HidStream)_deviceStream).SetFeature(packet);
            }
            catch { }

            packet = new byte[91];
            packet[2] = 0x1F;
            packet[6] = 0x06;
            packet[7] = 0x0f;
            packet[8] = 0x02;
            packet[9] = 0x01;
            packet[11] = 0x03;
            packet[89] = 0x09;
            try
            {
                ((HidStream)_deviceStream).SetFeature(packet);
            }
            catch { }

            packet = new byte[91];
            packet[2] = 0x1F;
            packet[6] = 0x02;
            packet[8] = 0x84;
            packet[89] = 0x86;
            try
            {
                ((HidStream)_deviceStream).SetFeature(packet);
            }
            catch { }
        }
    }

    public class HuntsmanV2TKLKeeb : LightingBase
    {
        public HuntsmanV2TKLKeeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        /// <summary>
        /// Keyboard X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 17;

        /// <summary>
        /// Max command report length
        /// </summary>
        private const int MAX_FEATURE_LENGTH = 91;

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                Type = LightingDevices.RazerHuntsmanV2TKL,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Razer Huntsman V2 TKL"
            };
        }

        protected override void TurnOffLed() 
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            int count = 0;
            for (int i = 0; i < KEYBOARD_YAXIS_COUNTS; i++)
            {
                List<ColorRGB> colors = new List<ColorRGB>();
                for (int j = 0; j < KEYBOARD_XAXIS_COUNTS; j++)
                {
                    colors.Add(ColorRGB.Black());
                }

                _displayColorBytes.AddRange(GetCommands(count, colors.ToArray()));
                count++;
            }
        }

        protected override void SetKeyLayout() 
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            int count = 0;
            for (int i = 0; i < KEYBOARD_YAXIS_COUNTS; i++)
            {
                List<ColorRGB> colors = new List<ColorRGB>();
                for (int j = 0; j < KEYBOARD_XAXIS_COUNTS; j++)
                {
                    colors.Add(colorMatrix[i, j]);
                }

                _displayColorBytes.AddRange(GetCommands(count, colors.ToArray()));
                count++;
            }
        }

        private byte[] GetCommands(int idx, ColorRGB[] colors)
        {
            byte[] commands = new byte[MAX_FEATURE_LENGTH];
            commands[0] = 0x00;
            commands[1] = 0x00;
            commands[2] = 0x1F;
            commands[3] = 0x00;
            commands[4] = 0x00;
            commands[5] = 0x00;
            commands[6] = 0x47;
            commands[7] = 0x0F;
            commands[8] = 0x03;
            commands[11] = Convert.ToByte(idx);
            commands[13] = 0x15;

            int index = 0;
            foreach (ColorRGB displayColor in colors)
            {
                commands[(index * 3) + 14] = displayColor.R;
                commands[(index * 3) + 15] = displayColor.G;
                commands[(index * 3) + 16] = displayColor.B;
                index++;
            }
            commands[89] = Methods.CalculateRazerAccessByte(commands);

            return commands;
        }
    }
}
