using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.Hardware.Devices;
using LightDancing.Hardware.Devices.UniversalDevice.Corsair.Keyboards;
using LightDancing.Hardware.Devices.UniversalDevice.Corsair.Tower;
using LightDancing.Hardware.Devices.UniversalDevice.Dygma;
using LightDancing.Hardware.Devices.UniversalDevice.HyperX.Keyboards;
using LightDancing.Hardware.Devices.UniversalDevice.Razer.Keyboards;
using LightDancing.Hardware.Devices.UniversalDevice.Razer.MousePads;
using LightDancing.Hardware.Devices.UniversalDevice.Razer.Tower;
using LightDancing.Hardware.Devices.UniversalDevice.Razer.Mouse;
using LightDancing.Hardware.Devices.UniversalDevice.WhirlwindFx.Keyboards;
using LightDancing.Hardware.Devices.UniversalDevice.Mountain;
using System.Collections.Generic;
using LightDancing.Hardware.Devices.UniversalDevice.ViewSonic.Monitor;
using System;
using LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard;

namespace LightDancing.Hardware
{
    /// <summary>
    /// The factory to create the device control
    /// MEMO: If add new devices, you must implement here too.
    /// </summary>
    public class LightingControlFactory
    {
        public ILightingControl GetControl(USBDevices type)
        {
            ILightingControl result = type switch
            {
                USBDevices.IBPMiniHub => new IBPMiniHubController(),
                //USBDevices.HotKeyboard => new HotKeebController(),
                USBDevices.CNVS => new CNVSController(),
                USBDevices.CNVSv1 => new CNVSV1Controller(),
                USBDevices.RedragonKeeb => new RedragonKeebController(),
                USBDevices.RedragonMouse => new RedragonMouseController(),
                USBDevices.RGBWall => new RGBWallController(),
                USBDevices.qRGBDemo => new QRGBDemoController(),
                USBDevices.NP50 => new NP50Controller(),
                USBDevices.CNVSLeft => new CNVSLeftController(),
                USBDevices.Suoai_MK4 => new SuoaiMK4Controller(),
                USBDevices.Redragon_MK4 => new RedragonMK4Controller(),
                USBDevices.RazerBlackwidowV3Keyboard => new RazerBlackwidowV3KeyboardController(),
                USBDevices.CorsairK100RGBKeyboard => new CorsairK100RGBKeyboardController(),
                USBDevices.CorsairK60ProLowProfile => new CorsairK60ProLowProfileController(),
                USBDevices.CorsairK70MKII => new CorsairK70mkIIController(),
                USBDevices.DygmaRaise => new DygmaRaiseController(),
                USBDevices.RazerHuntsmanV2TKL => new RazerHuntsmanV2TKLController(),
                USBDevices.WhirlwindFxElement => new WhirlwindFxElementKeyboardController(),
                USBDevices.HyperXAlloyCore => new HyperXAlloyCoreKeebController(),
                USBDevices.RazerBlackwidowV3MiniKeyboard => new RazerBlackwidowV3miniKeyboardController(),
                //USBDevices.NanoleafShapes => new NanoleafShapesController(),
                USBDevices.MountainEverest => new MountainEverestKeebController(),
                //USBDevices.SteelSeriesApex5 => new SteelSeriesApex5KeebController(),
                USBDevices.RazerStriderChroma => new RazerStriderChromaController(),
                //USBDevices.PhilipsHueBridge => new PhilipsHueBridgeController(),
                //USBDevices.ASUSZ790A => new Z790AController(),
                //USBDevices.CorsairLightingCommanderCore => new CorsairLightingCommanderCoreController(),
                USBDevices.CorsairLT100Tower => new CorsairLT100TowerController(),
                USBDevices.RazerBaseStationChroma => new RazerBaseStationChromaController(),
                USBDevices.ViewSonicXG270QG => new ViewSonicXG270QGController(),
                USBDevices.ViewSonicXG271QG => new ViewSonicXG271QGController(),
                USBDevices.RazerMambaElite => new RazerMambaEliteController(),
                USBDevices.SuoaiKeebTKL => new SuoaiKeebTKLController(),
                USBDevices.Q60 => new Q60Controller(),
                USBDevices.CoyaLedStrip => new CoyaController(),
                USBDevices.AsrockMotherBoard=>new ASRockMotherBoardController(),
                _ => throw new Exception($"Not handle with {type}, please implement the controller with it."),
            };

            return result;
        }
    }

    public interface ILightingControl
    {
        public List<USBDeviceBase> InitDevices();
    }

    public class LightingModel : HardwareModel
    {
        /// <summary>
        /// Device Width and Height
        /// </summary>
        public MatrixLayouts Layouts { get; set; }

        /// <summary>
        /// Device Type
        /// </summary>
        public LightingDevices Type;
    }
}
