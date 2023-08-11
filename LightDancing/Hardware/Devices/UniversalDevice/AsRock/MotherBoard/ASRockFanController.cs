using LightDancing.Hardware.Devices.Fans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public class ASRockFan: FanBase
    {
        ESCORE_FAN_ID ch;
        SSCORE_FAN_CONFIG config;
        public ASRockFan(ESCORE_FAN_ID ch, SSCORE_FAN_CONFIG config) :base()
        {
            this.ch = ch;
            this.config = config;
            Name = ch.ToString();
        }
        public override int SpeedPercentage
        {
            get
            {
                return Convert.ToInt16(CurrentRPM/ 255.0 * 100);
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
            ASRockFanController.DLL.SetASRockFanConfig(ch, config);
        }

        public override void SetRPM(int rpm)
        {
            Debug.WriteLine("Not Include SetRPM Function");
        }

    }

     public enum ESCORE_FAN_ID
    {
        ESCORE_FANID_CPU_FAN1,
        ESCORE_FANID_CPU_FAN2,
        ESCORE_FANID_CHASSIS_FAN1,
        ESCORE_FANID_CHASSIS_FAN2,
        ESCORE_FANID_CHASSIS_FAN3,
        ESCORE_FANID_CHASSIS_FAN4,
    }

    public class ASRockMotherBoardModel
    {
        public double CpuTemp;
        public double MBTemp;
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

    internal class ASRockFanController
    {
        ASRockMotherBoardModel model;
        List<FanBase> aSRockUsingFans;
        bool workBool;
        Thread stateThread;
        public ASRockFanController()
        {
            model = new ASRockMotherBoardModel();
        }

        public ASRockMotherBoardModel GetModel()
        {
            return model;
        }

        public List<FanBase> GetFanList(List<ESCORE_FAN_ID> List)
        {
            if(workBool)
            {
                workBool = false;
                stateThread.Join();
            }
            aSRockUsingFans = new List<FanBase>();
            foreach (ESCORE_FAN_ID id in List)
            {
                SSCORE_FAN_CONFIG Config = new SSCORE_FAN_CONFIG();
                DLL.GetAsrFanConfig(id, ref Config);
                FanBase Fan = new ASRockFan(id, Config);
                aSRockUsingFans.Add(Fan);
            }
            workBool = true;
            stateThread = new Thread(new ThreadStart(getTempFunction)) { IsBackground = true };
            stateThread.Start();
            return aSRockUsingFans;
        }

        private void getTempFunction()
        {
            double value = 0;
            while (workBool)
            {
                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_TEMP, ref model.CpuTemp);
                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_MB_TEMP, ref model.MBTemp);
                foreach (ESCORE_FAN_ID id in Enum.GetValues(typeof(ESCORE_FAN_ID)))
                {
                    FanBase basefan = aSRockUsingFans.FirstOrDefault(p => p.Name == id.ToString());
                    if (basefan != null)
                    {
                        switch (id)
                        {
                            case ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN1:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN2:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN1:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN2:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN2_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN3:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN3_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN4:
                                DLL.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN4_SPEED, ref value);
                                break;
                        }
                        basefan.CurrentRPM = Convert.ToInt16(value);
                    }
                }
                Thread.Sleep(300);
            }
        }
        public static bool InitFunction()
        {
            return DLL.AsrLibDllInit();
        }
 
        public void Dispose()
        {
            workBool = false;
            stateThread.Join();
            DLL.AsrLibDllUnInit();
        }

        #region DLLIMPORT
       internal class DLL
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
        #endregion
    }
}
