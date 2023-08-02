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
    internal class CoyaController: ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_9033";
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
                    CoyaDevice device = new CoyaDevice(stream.Item1, stream.Item2);
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

    public class CoyaDevice: USBDeviceBase
    {
        private readonly List<List<LightingBase>> serialDevices = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };

        public CoyaDevice(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();

            /*qRGB Demo*/
            CoyaLedStrip qrgbwall = new CoyaLedStrip(1, _model);
            serialDevices[0].Add(qrgbwall);
            hardwares.Add(qrgbwall);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (serialDevices[0][0].GetType() != null)
            {
                if (process)
                    serialDevices[0][0].ProcessStreaming(false, brightness);

                var colors = serialDevices[0][0].GetDisplayColors();

                /*Port3 - 280 surround leds*/
                List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x01, 0x01, 0x68, 0x00 };
                List<byte> port3Colors = colors; 
                for (int i = port3Colors.Count; i < 750; i++)
                {
                    port3Colors.Add(0x00);
                }
                collectBytes.AddRange(port3Colors);
                try
                {
                    _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
                }
                catch { }

                collectBytes = new List<byte>() { 0xff, 0xee, 0x04, 0x01, 0x68, 0x00 };
                List<byte> port4Colors = new List<byte>();
                for (int i = port4Colors.Count; i < 750; i++)
                {
                    port4Colors.Add(0x00);
                }
                collectBytes.AddRange(port4Colors);
                try
                {
                    _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
                }
                catch { }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _serialID;
            return new HardwareModel()
            {
                FirmwareVersion = "N/A",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.CoyaLedStrip,
                Name = "CoyaController",
            };
        }
    }
}
