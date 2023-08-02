using LightDancing.Hardware.Devices.Fans;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightDancing.Hardware.Devices.SmartComponents
{
    public class NP50 : FanBase
    {
        public double Temperature { set; get; }
        public double Noise { set; get; }

        public NP50(USBDeviceBase usbBase) : base(usbBase)
        {
            Component = Enums.SmartComponent.NP50;
            Name = "NP50";
        }

        public override void SetRPM(int rpm)
        {
            Console.WriteLine("Cannot set rpm speed");
        }

        public override void SetSpeed(int percentage)
        {
            SpeedPercentage = percentage;
            ((NP50Device)_usbBase).SetPumpSpeed();
        }

        public void SetMBBypass()
        {
            ((NP50Device)_usbBase).SetMBBypass();
        }
    }
}
