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
                bool FanGetInit = ASRockFanController.InitFunction();
                uint LedGetInit = ASRockLedController.InitFunction();
                if (LedGetInit == 0 && FanGetInit)
                {
                    Debug.WriteLine("Mother Board InitFunction Work");
                    List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                    AsRockMotherBoard MotherBoard = new AsRockMotherBoard(MOTHERBOARDNAME);
                    hardwares.Add(MotherBoard);
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
        private static ASRockFanController fanController;
        private static ASRockLedController ledController;
        private static bool enableBool = false;
        private List<ASRockMode> nowModeList;
        private string name;

        public AsRockMotherBoard(string MotherBoardName) : base()
        {
            name = MotherBoardName;
            _model = InitModel();
            Enable(_model);
            _lightingBase = InitDevice();
            nowModeList = ledController.GetModeDeepList();
        }

        public List<FanBase> GetFanList(List<ESCORE_FAN_ID> List)
        {
            return fanController.GetFanList(List);
        }

        public ASRockMotherBoardModel GetTempMode()
        {
            return fanController.GetModel();
        }

        public List<ASRockMode> GetLedControlList()
        {
            return ledController.GetModeList();
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
                ASRockMode nodeState = nowModeList[i];
                LightingBase lightBase = _lightingBase[i];
                lightBase.ProcessStreaming(false, brightness);
                List<ASRLIB_LedColor> resultCololr = GetColorList(lightBase.GetDisplayColors());
                while (resultCololr.Count < nodeState.GetMaxLed())
                {
                    resultCololr.Add(new ASRLIB_LedColor() { ColorR = 0x00, ColorG = 0x00, ColorB = 0x00 });
                }
                result = ASRockLedController.Polychrome_SetLedColorConfig(Convert.ToUInt32(nodeState.GetChanel()), resultCololr.ToArray(), Convert.ToUInt32(nodeState.GetMaxLed()));
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

        private static void Enable(HardwareModel model)
        {
            if (!enableBool)
            {
                fanController = new ASRockFanController();
                ledController = new ASRockLedController(model);
                enableBool = true;
            }
        }

        private List<ASRLIB_LedColor> GetColorList(List<byte> byteList)
        {
            List<ASRLIB_LedColor> Results = new List<ASRLIB_LedColor>();
            int count = byteList.Count / 3;
            for (int i = 0; i < count; i++)
            {
                Results.Add(new ASRLIB_LedColor() { ColorR = byteList[i], ColorG = byteList[i + 1], ColorB = byteList[i + 2] });
            }
            return Results;
        }
    }
}
