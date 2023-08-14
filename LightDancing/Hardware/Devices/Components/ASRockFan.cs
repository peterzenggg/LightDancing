using LightDancing.Hardware.Devices.Fans;
using LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard;
using System;
using System.Diagnostics; 

namespace LightDancing.Hardware.Devices.Components
{
    public class ASRockFan : FanBase
    {
        ESCORE_FAN_ID ch;
        SSCORE_FAN_CONFIG config;
        public ASRockFan(ESCORE_FAN_ID ch, SSCORE_FAN_CONFIG config) : base()
        {
            this.ch = ch;
            this.config = config;
            Name = ch.ToString();
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
            config.ControlType = ESCORE_FAN_CONTROL_TYPE.ESCORE_FANCTL_MANUAL;
            config.TargetFanSpeed = Target;
            AsrockFanDll.SetASRockFanConfig(ch, config);
        }

        public override void SetRPM(int rpm)
        {
            Debug.WriteLine("Not Include SetRPM Function");
        }

        public void BackToFW()
        {
            config.ControlType = ESCORE_FAN_CONTROL_TYPE.ESCORE_FANCTL_SMART_FAN_4;
            config.SMART_FAN4_Temp1 = 30;
            config.SMART_FAN4_Temp2 = 40;
            config.SMART_FAN4_Temp3 = 50;
            config.SMART_FAN4_Temp4 = 60;
            config.SMART_FAN4_Critical_Temp = 80;
            config.SMART_FAN4_Speed1 = 0x40;
            config.SMART_FAN4_Speed2 = 0x60;
            config.SMART_FAN4_Speed3 = 0x80;
            config.SMART_FAN4_Speed4 = 0xA0;
            config.SMART_FAN4_Temp_Source = 0x01; // MB Temperature = 1,  Cpu Temperature = 0
            AsrockFanDll.SetASRockFanConfig(ch, config);
        }

    }
}
