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
                bool FanGetInit = ASRockFanController.InitFanFunction();
                uint LedGetInit = AsRockLedController.InitFunction();
                if (LedGetInit == 0 && FanGetInit)
                {
                    Debug.WriteLine("Mother Board InitFunction Work");
                    AsRockMotherBoard.Enable();
                    return null;
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
        static bool EnableBool = false;
        AsRockLedController LedController;
        string Name;

        public AsRockMotherBoard(string MotherBoardName) : base()
        {
            Name = MotherBoardName;
            _model = InitModel();
            //LedController.SetModel(_model);
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
            return null;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            
        }

        public static void Enable()
        {
            if (!EnableBool)
            {
                FanController = new ASRockFanController();
                EnableBool = true;
            }
        }
    }
}
