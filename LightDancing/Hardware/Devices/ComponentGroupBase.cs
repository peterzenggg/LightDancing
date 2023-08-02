using LightDancing.Hardware.Devices.SmartComponents;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices
{
    public abstract class ComponentGroupBase
    {
        public int DeviceCount { get => DeviceBases.Count; }
        public List<SmartDevice> DeviceBases { get; set; } = new List<SmartDevice>();
    }
}
