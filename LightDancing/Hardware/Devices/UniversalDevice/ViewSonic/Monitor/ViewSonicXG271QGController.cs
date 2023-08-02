using HidSharp;
using System.Collections.Generic;
using LightDancing.Enums;
using LightDancing.Models.ViewSonic;

namespace LightDancing.Hardware.Devices.UniversalDevice.ViewSonic.Monitor
{
    internal class ViewSonicXG271QGController : ILightingControl
    {
        private const int DEVICE_VID = 0x0543;
        private const int DEVICE_PID = 0xA004;
        private readonly ViewSonicXG27SeriesConfigModels _config = new ViewSonicXG27SeriesConfigModels() { 
            Name = "ViewSonic XG271QG",
            MaxFeatureLength = 190,
            USBDeviceType = USBDevices.ViewSonicXG270QG,
            LightingDevicesType = LightingDevices.ViewSonicXG270QG
        };

        public List<USBDeviceBase> InitDevices()
        {
            var hardwares = new List<USBDeviceBase>();

            AddDevices(hardwares, new HidDetector().GetHidStreams(DEVICE_VID, DEVICE_PID, _config.MaxFeatureLength));

            return hardwares.Count > 0 ? hardwares : null;
        }

        private void AddDevices(List<USBDeviceBase> hardwares, List<HidStream> streams)
        {
            if (streams != null)
            {
                foreach (var stream in streams)
                {
                    ViewSonicXG27SeriesDevice device = new ViewSonicXG27SeriesDevice(stream, _config);
                    hardwares.Add(device);
                }
            }
        }
    }
}
