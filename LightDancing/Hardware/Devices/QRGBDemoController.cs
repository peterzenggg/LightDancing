using HidSharp;
using LightDancing.Enums;
using LightDancing.Hardware.Devices.Components;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices
{
    /// <summary>
    /// For 2023 CES only
    /// qRGB demo with WS2815
    /// </summary>
    internal class QRGBDemoController : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0903";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<Tuple<SerialStream, string>> streams = serialDetector.GetSerialStreamsAndSerialID(DEVICE_PID);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (Tuple<SerialStream, string> stream in streams)
                {
                    stream.Item1.BaudRate = BAUDRATES;
                    QRGBDemoDevice device = new QRGBDemoDevice(stream.Item1, stream.Item2);
                    hardwares.Add(device);
                }
                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class QRGBDemoDevice : USBDeviceBase
    {
        private readonly List<List<LightingBase>> serialDevices = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };

        public QRGBDemoDevice(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();

            /*qRGB Demo*/
            QRGBWall qrgbwall = new QRGBWall(3, _model);
            serialDevices[2].Add(qrgbwall);
            hardwares.Add(qrgbwall);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (serialDevices[2][0].GetType() != null)
            {
                if (process)
                    serialDevices[2][0].ProcessStreaming(false, brightness);

                var colors = serialDevices[2][0].GetDisplayColors();

                /*Port3 - 280 surround leds*/
                List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x03, 0x01, 0x68, 0x00 };
                List<byte> port3Colors = colors.GetRange(0, 210 * 3); //414, 36
                for (int i = port3Colors.Count; i < 750; i++)
                {
                    port3Colors.Add(0x00);
                }
                collectBytes.AddRange(port3Colors);
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                /*Port4 - qRGB letter 20 leds */
                collectBytes = new List<byte>() { 0xff, 0xee, 0x04, 0x01, 0x68, 0x00 };
                List<byte> port4Colors = colors.GetRange(210 * 3, 90 * 3); //0, 414
                for (int i = port4Colors.Count; i < 750; i++)
                {
                    port4Colors.Add(0x00);
                }
                collectBytes.AddRange(port4Colors);
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _serialID;
            return new HardwareModel()
            {
                FirmwareVersion = "N/A",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.qRGBDemo,
                Name = "qRGB Demo",
            };
        }
    }
}
