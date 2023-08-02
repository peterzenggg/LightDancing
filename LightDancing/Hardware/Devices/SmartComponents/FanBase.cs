using LightDancing.Enums;
using LightDancing.Hardware.Devices.SmartComponents;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.Fans
{
    public abstract class FanBase : SmartDevice
    {
        public virtual int SpeedPercentage { get; set; }
        public int TargetRPM { get; set; }
        public int CurrentRPM { get; set; }
        public FanOrientation FanDirection { get; set; }
        public bool IsTouch { get; set; } = false;

        public readonly USBDeviceBase _usbBase;

        public WindDirection WindDirection { get; set; }

        public string Name { get; set; }

        public FanBase(USBDeviceBase usbBase) 
        {
            _usbBase = usbBase;
        }
        public FanBase()
        {
            
        }

        public abstract void SetSpeed(int percentage);

        public abstract void SetRPM(int rpm);
    }

    public enum FanOrientation
    {
        FaceDown,
        VerticalStrait,
        VerticalUpsideDown,
        FaceUp,
    }

    public enum WindDirection
    {
        UnKnown,
        Inhale,
        Exhale,
    }

    public class FanGroup : ComponentGroupBase
    {
        public SmartComponent ComponentType { get; set; }
        public FanOrientation FanDirection { get; set; }


    }
}
