using System.Runtime.InteropServices;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public enum ESCORE_FAN_ID
    {
        ESCORE_FANID_CPU_FAN1,
        ESCORE_FANID_CPU_FAN2,
        ESCORE_FANID_CHASSIS_FAN1,
        ESCORE_FANID_CHASSIS_FAN2,
        ESCORE_FANID_CHASSIS_FAN3,
        ESCORE_FANID_CHASSIS_FAN4,
    }

    internal enum ESCORE_HWM_ITEM
    {
        ESCORE_HWM_CPU_TEMP,
        ESCORE_HWM_MB_TEMP,
        ESCORE_HWM_CPU_FAN1_SPEED,
        ESCORE_HWM_CPU_FAN2_SPEED,
        ESCORE_HWM_CHASSIS_FAN1_SPEED,
        ESCORE_HWM_CHASSIS_FAN2_SPEED,
        ESCORE_HWM_CHASSIS_FAN3_SPEED,
        ESCORE_HWM_CHASSIS_FAN4_SPEED,
    }
    public enum ESCORE_FAN_CONTROL_TYPE
    {
        ESCORE_FANCTL_MANUAL,
        ESCORE_FANCTL_SMART_FAN_1,
        ESCORE_FANCTL_RESERVED1,
        ESCORE_FANCTL_RESERVED2,
        ESCORE_FANCTL_SMART_FAN_4
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SSCORE_FAN_CONFIG
    {
        public ESCORE_FAN_CONTROL_TYPE ControlType;
        public int TargetFanSpeed;
        public int TargetTemperature;
        public int SMART_FAN4_Temp1;
        public int SMART_FAN4_Speed1;
        public int SMART_FAN4_Temp2;
        public int SMART_FAN4_Speed2;
        public int SMART_FAN4_Temp3;
        public int SMART_FAN4_Speed3;
        public int SMART_FAN4_Temp4;
        public int SMART_FAN4_Speed4;
        public int SMART_FAN4_Critical_Temp;
        public int SMART_FAN4_Temp_Source;
        public int SMART_FAN4_FanStop_Enabled;
    }

    internal class AsrockFanDll
    {
        private const string DLL_PATH = @"AsrCore.dll";

        [DllImport(DLL_PATH)]
        public static extern bool AsrLibDllInit();

        [DllImport(DLL_PATH)]
        public static extern bool AsrLibDllUnInit();

        [DllImport(DLL_PATH)]
        public static extern int AsrLibDllGetLastError();

        [DllImport(DLL_PATH)]
        public static extern unsafe bool AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM Item, double* temp);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe bool AsrLibGetFanConfig(ESCORE_FAN_ID FanId, SSCORE_FAN_CONFIG* Config);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe bool AsrLibSetFanConfig(ESCORE_FAN_ID FanId, SSCORE_FAN_CONFIG* Config);

        public static bool AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM Item, ref double temp)
        {
            unsafe
            {
                fixed (double* TempP = &temp)
                    return AsrLibGetHardwareMonitor(Item, TempP);
            }
        }

        public static bool GetAsrFanConfig(ESCORE_FAN_ID FanID, ref SSCORE_FAN_CONFIG Config)
        {
            unsafe
            {
                fixed (SSCORE_FAN_CONFIG* ConfigP = &Config)
                    return AsrLibGetFanConfig(FanID, ConfigP);
            }
        }

        public static bool SetASRockFanConfig(ESCORE_FAN_ID FanID, SSCORE_FAN_CONFIG Config)
        {
            unsafe
            {
                SSCORE_FAN_CONFIG* ConfigP = &Config;
                return AsrLibSetFanConfig(FanID, ConfigP);
            }
        }
    }

}
