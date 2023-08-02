using HidSharp;
using LightDancing.Enums;
using LightDancing.Hardware.Devices.Components;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices
{
    /// <summary>
    /// For 2023 CES only
    /// RGB wall demo with christmas light
    /// </summary>
    internal class RGBWallController : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0902";
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
                    RGBWallDevice device = new RGBWallDevice(stream.Item1, stream.Item2);
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

    public class RGBWallDevice : USBDeviceBase
    {
        private readonly List<List<LightingBase>> serialDevices = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };

        public RGBWallDevice(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();

            /*Christmas Light*/
            //RGBWallChristmasLight christmasLight = new RGBWallChristmasLight(1, _model);
            RGBHallWay device = new RGBHallWay(1, _model);
            serialDevices[0].Add(device);
            hardwares.Add(device);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (serialDevices[0][0].GetType() != null)
            {
                if (process)
                    serialDevices[0][0].ProcessStreaming(false, brightness);

                var colors = serialDevices[0][0].GetDisplayColors();

                ///*Port1*/
                //List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x01, 0x01, 0x68, 0x00 };
                //List<byte> port1Colors = colors.GetRange(192 * 2 * 3, 192 * 3);

                //for (int i = port1Colors.Count; i < 750; i++)
                //{
                //    port1Colors.Add(0x00);
                //}

                //collectBytes.AddRange(port1Colors);
                //_deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                ///*Port3*/
                //collectBytes = new List<byte>() { 0xff, 0xee, 0x03, 0x01, 0x68, 0x00 };
                //List<byte> port3Colors = colors.GetRange(192 * 3, 192 * 3);

                //for(int i = port3Colors.Count; i < 750; i++)
                //{
                //    port3Colors.Add(0x00);
                //}
                //collectBytes.AddRange(port3Colors);
                //_deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                ///*Port4*/
                //collectBytes = new List<byte>() { 0xff, 0xee, 0x04, 0x01, 0x68, 0x00 };
                //List<byte> port4Colors = colors.GetRange(0, 192 * 3);
                //for (int i = port4Colors.Count; i < 750; i++)
                //{
                //    port4Colors.Add(0x00);
                //}
                //collectBytes.AddRange(port4Colors);
                //_deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                /*For Computex 2023 only*/
                /*Port1*/
                List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x01, 0x01, 0x68, 0x00 };
                List<byte> port1Colors = colors.GetRange(0, 94 * 3);

                for (int i = port1Colors.Count; i < 750; i++)
                {
                    port1Colors.Add(0x00);
                }

                collectBytes.AddRange(port1Colors);
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                /*Port3*/
                collectBytes = new List<byte>() { 0xff, 0xee, 0x03, 0x01, 0x68, 0x00 };
                List<byte> port3Colors = colors.GetRange(94 * 3, 188 * 3);

                for (int i = port3Colors.Count; i < 750; i++)
                {
                    port3Colors.Add(0x00);
                }
                collectBytes.AddRange(port3Colors);
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);

                /*Port4*/
                collectBytes = new List<byte>() { 0xff, 0xee, 0x04, 0x01, 0x68, 0x00 };
                List<byte> port4Colors = colors.GetRange((94 + 188) * 3, 188 * 3);
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
                USBDeviceType = USBDevices.RGBWall,
                Name = "qRGB Hallway",
            };
        }
    }
}
