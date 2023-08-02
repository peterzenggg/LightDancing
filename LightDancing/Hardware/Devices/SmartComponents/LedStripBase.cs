using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.SmartComponents
{
    public abstract class SmartDevice
    {
        public SmartComponent Component { get; set; }
        public bool IsConnected { get; set; } = false;
    }

    public abstract class LedStripBase : SmartDevice
    {
        public int LedCount { get; set; }
    }

    public class PQ10 : LedStripBase
    {
        public PQ10() 
        {
            LedCount = 20;
            Component = SmartComponent.PQ10;
        }
    }

    public class PQ30 : LedStripBase
    {
        public PQ30()
        {
            LedCount = 62;
            Component = SmartComponent.PQ10;
        }
    }

    public class LedGroup : ComponentGroupBase
    {
        public int TotalLedCount { get; set; }

        public int GetLedCount()
        {
            TotalLedCount = 0;
            foreach(var device in DeviceBases)
            {
                TotalLedCount += ((LedStripBase)device).LedCount;
            }

            return TotalLedCount;
        }
    }

}
