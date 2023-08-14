using LightDancing.Hardware.Devices.Fans;
using LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard;
using System;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.Components
{
    public class ASRockFan : FanBase
    {
        private readonly ESCORE_FAN_ID _channel;
        private SSCORE_FAN_CONFIG _config;
        public ASRockFan(ESCORE_FAN_ID channel, SSCORE_FAN_CONFIG config) : base()
        {
            _channel = channel;
            _config = config;
            Name = channel.ToString();
        }
        public override int SpeedPercentage
        {
            get
            {
                return Convert.ToInt16(CurrentRPM / 255.0 * 100);
            }
        }

        public override void SetSpeed(int percentage)
        {
            int Target = 50;
            if (percentage > 0 && percentage <= 100)
            {
                Target = Convert.ToInt16((percentage / 100.0) * 255.0);
            }
            _config.ControlType = ESCORE_FAN_CONTROL_TYPE.ESCORE_FANCTL_MANUAL;
            _config.TargetFanSpeed = Target;
            ASRockFanDll.SetASRockFanConfig(_channel, _config);
        }

        public override void SetRPM(int rpm)
        {
            Debug.WriteLine("Not Include SetRPM Function");
        }

        public void BackToFW()
        {
            _config.ControlType = ESCORE_FAN_CONTROL_TYPE.ESCORE_FANCTL_SMART_FAN_4;
            _config.SMART_FAN4_Temp1 = 30;
            _config.SMART_FAN4_Temp2 = 40;
            _config.SMART_FAN4_Temp3 = 50;
            _config.SMART_FAN4_Temp4 = 60;
            _config.SMART_FAN4_Critical_Temp = 80;
            _config.SMART_FAN4_Speed1 = 0x40;
            _config.SMART_FAN4_Speed2 = 0x60;
            _config.SMART_FAN4_Speed3 = 0x80;
            _config.SMART_FAN4_Speed4 = 0xA0;
            _config.SMART_FAN4_Temp_Source = 0x01; // MB Temperature = 1,  Cpu Temperature = 0
            ASRockFanDll.SetASRockFanConfig(_channel, _config);
        }

    }
}
