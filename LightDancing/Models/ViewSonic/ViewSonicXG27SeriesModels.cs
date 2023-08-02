using LightDancing.Enums;

namespace LightDancing.Models.ViewSonic
{
    public class ViewSonicXG27SeriesConfigModels
    {
        public string Name { get; set; }
        public int MaxFeatureLength { get; set; }
        public USBDevices USBDeviceType { get; set; }
        public LightingDevices LightingDevicesType { get; set; }
    }
}