using System;

namespace LightDancing.Hardware.Devices.Fans
{
    public class Q60 : FanBase
    {
        public double PumpTempIn { set; get; }
        public double PumpTempOut { set; get; }
        public double Noise { set; get; }

        public Q60(USBDeviceBase usbBase) : base(usbBase)
        {
            Component = Enums.SmartComponent.Q60;
            Name = "Q60 Pump";
        }

        public override void SetRPM(int rpm)
        {
            Console.WriteLine("Cannot set rpm speed");
        }

        public override void SetSpeed(int percentage)
        {
            SpeedPercentage = percentage;
            ((Q60Device)_usbBase).SetPumpSpeed();
        }

        public void SetMBBypass()
        {
            ((Q60Device)_usbBase).SetMBBypass();
        }
    }
}
