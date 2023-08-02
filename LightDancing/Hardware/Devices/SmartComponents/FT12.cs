using LightDancing.Enums;

namespace LightDancing.Hardware.Devices.Fans
{
    public class FT12 : FanBase
    {
        public ControlMode controlMode;

        public SmartComponent ComponentType { get; set; }

        public double Temperature { get; set; }

        public FT12(USBDeviceBase usbBase, string name) : base(usbBase) 
        {
            Name = name;
            Component = SmartComponent.FT12;
        }

        public override void SetRPM(int rpm)
        {
            controlMode = ControlMode.RPM;
            TargetRPM = rpm;
            ((Q60Device)_usbBase).SetFanSpeed();
        }

        public override void SetSpeed(int percentage)
        {
            controlMode = ControlMode.Percentage;
            SpeedPercentage = percentage;
            ((Q60Device)_usbBase).SetFanSpeed();
        }
    }

    public enum ControlMode
    {
        Percentage,
        RPM,
    }
}
