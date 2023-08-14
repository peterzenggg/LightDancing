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
                bool fanGetInit = ASRockFanController.InitFunction();
                uint ledGetInit = ASRockLedController.InitFunction();
                if (ledGetInit == 0 && fanGetInit)
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
        private readonly ASRockFanController _fanController;
        private readonly ASRockLedController _ledController;
        private readonly string _name;
        private List<ESCORE_FAN_ID> _settingList = new List<ESCORE_FAN_ID>();
        private List<ASRockMode> _nowModeList;
        public AsRockMotherBoard(string MotherBoardName) : base()
        {
            _name = MotherBoardName;
            _model = InitModel();
            _fanController = new ASRockFanController();
            _ledController = new ASRockLedController(_model);
            ChangeLightDevice();
        }

        public void ChangeLightDevice()
        {
            _lightingBase = InitDevice();
            _nowModeList = _ledController.GetModeDeepList();
        }

        public void SetFanList(List<ESCORE_FAN_ID> fanList)
        {
            _settingList = fanList;
        }

        public void SetModeList(List<ASRockMode> settingList)
        {
            _ledController.SetModeList(settingList);
        }

        public List<FanGroup> GetFanGroups()
        {
            List<FanGroup> result = new List<FanGroup>();
            List<FanBase> fans = new List<FanBase>();
            fans = _fanController.GetFanList(_settingList);
            foreach (FanBase baseFan in fans)
            {
                FanGroup fangroup = new FanGroup();
                fangroup.DeviceBases.Add(baseFan);
                result.Add(fangroup);
            }
            return result;
        }

        public ASRockMotherBoardModel GetTemperatureMode()
        {
            return _fanController.GetModel();
        }

        public List<ASRockMode> GetLedControlList()
        {
            return _ledController.GetModeList();
        }

        public override void TurnFwAnimationOn()
        {
            _fanController.TurnBackToFW();
            _ledController.TurnBackToFW();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                Name = _name
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return _ledController.ChangeCommit();
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            uint result = 0;
            for (int i = 0; i < _lightingBase.Count; i++)
            {
                ASRockMode nodeState = _nowModeList[i];
                LightingBase lightBase = _lightingBase[i];
                lightBase.ProcessStreaming(false, brightness);
                List<ASRLIB_LedColor> resultCololr = GetColorList(lightBase.GetDisplayColors());
                while (resultCololr.Count < nodeState.MaxLed)
                {
                    resultCololr.Add(new ASRLIB_LedColor() { ColorR = 0x00, ColorG = 0x00, ColorB = 0x00 });
                }
                result = ASRockLedController.Polychrome_SetLedColorConfig(Convert.ToUInt32(nodeState.Channel), resultCololr.ToArray(), Convert.ToUInt32(nodeState.MaxLed));
                if (result != 0)
                {
                    Debug.WriteLine("ASRockLedController.Polychrome_SetLedColorConfig Error");
                }
            }
            result = ASRockLedController.Polychrome_SetLedColors();
            if (result != 0)
            {
                Debug.WriteLine("ASRockLedController.Polychrome_SetLedColors Error");
            }
        }

        private List<ASRLIB_LedColor> GetColorList(List<byte> byteList)
        {
            List<ASRLIB_LedColor> Results = new List<ASRLIB_LedColor>();
            for (int i = 0; i < byteList.Count; i += 3)
            {
                Results.Add(new ASRLIB_LedColor() { ColorR = byteList[i], ColorG = byteList[i + 1], ColorB = byteList[i + 2] });
            }
            return Results;
        }
    }
}
