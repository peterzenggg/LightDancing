using LightDancing.Hardware.Devices.Fans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LightDancing.Hardware.Devices.Components;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public class AsrockMotherBoardModel
    {
        public double CpuTemp;
        public double MBTemp;
    }

    internal class AsrockFanController
    {
        AsrockMotherBoardModel model;
        List<FanBase> aSRockUsingFans;
        bool workBool;
        Thread stateThread;
        public AsrockFanController()
        {
            model = new AsrockMotherBoardModel();
        }

        public AsrockMotherBoardModel GetModel()
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
                AsrockFanDll.GetAsrFanConfig(id, ref Config);
                FanBase Fan = new ASRockFan(id, Config);
                aSRockUsingFans.Add(Fan);
            }
            workBool = true;
            stateThread = new Thread(new ThreadStart(getBoardStausFunction)) { IsBackground = true };
            stateThread.Start();
            return aSRockUsingFans;
        }

        public void TurnBackToFW()
        {
            foreach (ASRockFan Fan in aSRockUsingFans)
            {
                Fan.BackToFW();
            }
        }

        private void getBoardStausFunction()
        {
            double value = 0;
            while (workBool)
            {
                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_TEMP, ref model.CpuTemp);
                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_MB_TEMP, ref model.MBTemp);
                foreach (ESCORE_FAN_ID id in Enum.GetValues(typeof(ESCORE_FAN_ID)))
                {
                    FanBase basefan = aSRockUsingFans.FirstOrDefault(p => p.Name == id.ToString());
                    if (basefan != null)
                    {
                        switch (id)
                        {
                            case ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN1:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN2:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN1:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN1_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN2:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN2_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN3:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN3_SPEED, ref value);
                                break;
                            case ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN4:
                                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CHASSIS_FAN4_SPEED, ref value);
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
            return AsrockFanDll.AsrLibDllInit();
        }
 
        public void Dispose()
        {
            workBool = false;
            stateThread.Join();
            AsrockFanDll.AsrLibDllUnInit();
        }
    }
}