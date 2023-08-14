using LightDancing.Hardware.Devices.Fans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LightDancing.Hardware.Devices.Components;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public class ASRockMotherBoardModel
    {
        public double CPUTemperature;
        public double MBTemperature;
    }

    internal class ASRockFanController
    {
        private ASRockMotherBoardModel _model;
        private List<FanBase> _aSRockUsingFans;
        private bool _workBool;
        private Thread _stateThread;
        public ASRockFanController()
        {
            _model = new ASRockMotherBoardModel();
        }

        public ASRockMotherBoardModel GetModel()
        {
            return _model;
        }

        public List<FanBase> GetFanList(List<ESCORE_FAN_ID> usingList)
        {
            if (_workBool)
            {
                _workBool = false;
                _stateThread.Join();
            }
            _aSRockUsingFans = new List<FanBase>();
            foreach (ESCORE_FAN_ID fanID in usingList)
            {
                SSCORE_FAN_CONFIG config = new SSCORE_FAN_CONFIG();
                AsrockFanDll.GetAsrFanConfig(fanID, ref config);
                _aSRockUsingFans.Add(new ASRockFan(fanID, config));
            }
            _workBool = true;
            _stateThread = new Thread(new ThreadStart(GetBoardStausFunction)) { IsBackground = true };
            _stateThread.Start();
            return _aSRockUsingFans;
        }

        public void TurnBackToFW()
        {
            foreach (ASRockFan Fan in _aSRockUsingFans)
            {
                Fan.BackToFW();
            }
        }

        private void GetBoardStausFunction()
        {
            double value = 0;
            while (_workBool)
            {
                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_CPU_TEMP, ref _model.CPUTemperature);
                AsrockFanDll.AsrLibGetHardwareMonitor(ESCORE_HWM_ITEM.ESCORE_HWM_MB_TEMP, ref _model.MBTemperature);
                foreach (ESCORE_FAN_ID fanID in Enum.GetValues(typeof(ESCORE_FAN_ID)))
                {
                    FanBase baseFan = _aSRockUsingFans.FirstOrDefault(p => p.Name == fanID.ToString());
                    if (baseFan != null)
                    {
                        switch (fanID)
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
                        baseFan.CurrentRPM = Convert.ToInt16(value);
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
            _workBool = false;
            _stateThread.Join();
            AsrockFanDll.AsrLibDllUnInit();
        }
    }
}