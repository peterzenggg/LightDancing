using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

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


    class AsRockMotherBoard: USBDeviceBase
    {
        static ASRockFanController FanController;
        static ASRockLedController LedController;
        static bool EnableBool = false;
        List<ASRockMode> NowModeList;
        string Name;

        public AsRockMotherBoard(string MotherBoardName) : base()
        {
            Name = MotherBoardName;
            _model = InitModel();
            Enable(_model);
            _lightingBase = InitDevice();
            NowModeList = LedController.GetModeDeepList();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                Name = Name
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return LedController.ChangeCommit();
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            for (int i = 0; i < _lightingBase.Count; i++)
            {
                ASRockMode NodeState = NowModeList[i];
                LightingBase Base = _lightingBase[i];
                Base.ProcessStreaming(false, brightness);
                List<ASRLIB_LedColor> ResultCololr = GetColorList(Base.GetDisplayColors());
                while (ResultCololr.Count < NodeState.GetMaxLed())
                {
                    ResultCololr.Add(new ASRLIB_LedColor() { ColorR = 0x00, ColorG = 0x00, ColorB = 0x00 });
                }

            }
        }

        public static void Enable(HardwareModel model)
        {
            if (!EnableBool)
            {
                FanController = new ASRockFanController();
                LedController = new ASRockLedController(model);
                EnableBool = true;
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
