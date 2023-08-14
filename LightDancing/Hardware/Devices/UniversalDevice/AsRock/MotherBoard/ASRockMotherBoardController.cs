using LightDancing.Hardware.Devices.Fans;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    internal class ASRockMotherBoardController : ILightingControl
    {
        const string MOTHERBOARDNAME = "B760M Pro RS/D4";
        public List<USBDeviceBase> InitDevices()
        {
            if (UsbHotSwap.GetBoardName() == MOTHERBOARDNAME)
            {
                bool FanGetInit = AsrockFanController.InitFunction();
                uint LedGetInit = AsrockLedController.InitFunction();
                if (LedGetInit == 0 && FanGetInit)
                {
                    List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                    if (AsRockMotherBoard.Instance == null)
                    {
                        AsRockMotherBoard.Instance = new AsRockMotherBoard(MOTHERBOARDNAME);
                    }
                    else
                    {
                        AsRockMotherBoard.Instance.ChangeLightDevice();
                    }
                    Debug.WriteLine("Mother Board InitFunction Work");
                    hardwares.Add(AsRockMotherBoard.Instance);
                    return hardwares;
                }
                else
                {
                    Debug.WriteLine("Mother Board InitFunction not work");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    class AsRockMotherBoard : USBDeviceBase
    {
        public static AsRockMotherBoard Instance;
        private AsrockFanController fanController;
        private AsrockLedController ledController;
        private List<ESCORE_FAN_ID> settingList = new List<ESCORE_FAN_ID>()
        {
            ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN1,
            ESCORE_FAN_ID.ESCORE_FANID_CPU_FAN2,
            ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN1,
            ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN2,
            ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN3,
            ESCORE_FAN_ID.ESCORE_FANID_CHASSIS_FAN4,
        };

        private List<AsrockMode> nowModeList;
        private string name;
        public AsRockMotherBoard(string MotherBoardName) : base()
        {
            name = MotherBoardName;
            _model = InitModel();
            fanController = new AsrockFanController();
            ledController = new AsrockLedController(_model);
            ChangeLightDevice();
        }

        public void ChangeLightDevice()
        {
            _lightingBase = InitDevice();
            nowModeList = ledController.GetModeDeepList();
        }

        public void SetFanList(List<ESCORE_FAN_ID> List)
        {
            settingList = List;
        }

        public List<FanGroup> GetFanGroups()
        {
            List<FanGroup> result = new List<FanGroup>();
            List<FanBase> fans = new List<FanBase>();
            fans = fanController.GetFanList(settingList);
            foreach (FanBase baseFan in fans)
            {
                FanGroup fangroup = new FanGroup();
                fangroup.DeviceBases.Add(baseFan);
                result.Add(fangroup);
            }
            return result;
        }

        public AsrockMotherBoardModel GetTempMode()
        {
            return fanController.GetModel();
        }

        public List<AsrockMode> GetLedControlList()
        {
            return ledController.GetModeList();
        }

        public override void TurnFwAnimationOn()
        {
            fanController.TurnBackToFW();
            ledController.TurnBackToFW();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                Name = name
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return ledController.ChangeCommit();
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            uint result = 0;
            for (int i = 0; i < _lightingBase.Count; i++)
            {
                AsrockMode nodeState = nowModeList[i];
                LightingBase lightBase = _lightingBase[i];
                lightBase.ProcessStreaming(false, brightness);
                List<ASRLIB_LedColor> resultCololr = GetColorList(lightBase.GetDisplayColors());
                while (resultCololr.Count < nodeState.MaxLed)
                {
                    resultCololr.Add(new ASRLIB_LedColor() { ColorR = 0x00, ColorG = 0x00, ColorB = 0x00 });
                }
                result = AsrockLedController.Polychrome_SetLedColorConfig(Convert.ToUInt32(nodeState.Channel), resultCololr.ToArray(), Convert.ToUInt32(nodeState.MaxLed));
                if (result != 0)
                {
                    Debug.WriteLine("ASRockLedController.Polychrome_SetLedColorConfig Error");
                }
            }
            result = AsrockLedController.Polychrome_SetLedColors();
            if (result != 0)
            {
                Debug.WriteLine("ASRockLedController.Polychrome_SetLedColors Error");
            }
        }

        private List<ASRLIB_LedColor> GetColorList(List<byte> byteList)
        {
            List<ASRLIB_LedColor> Results = new List<ASRLIB_LedColor>();
            for (int i = 0; i < byteList.Count; i+=3)
            {
                Results.Add(new ASRLIB_LedColor() { ColorR = byteList[i], ColorG = byteList[i + 1], ColorB = byteList[i + 2] });
            }
            return Results;
        }
    }
}
