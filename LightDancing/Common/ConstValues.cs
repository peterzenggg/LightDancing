using LightDancing.Enums;
using System.Collections.Generic;

namespace LightDancing.Common
{
    public class ConstValues
    {
        public const int TWENTY_MS = 20;
        /// <summary>
        /// This value is the size of the RGBA size from opentk
        /// </summary>
        public const int RGBA_BYTE = 4;
        public const int EACH_HZ_OF_INDEX = 46; // SampleRate / FFTlength (48000 / 1024 = 46)
        public const int NEGATIVE_3DB_INTENSITY = 83; // Most of the audio max volume is -3DB so that we test on -3DB of the sine wave the intensity is around 83.
        public const double NEGATIVE_3DB_VOLUME = 1;

        public Dictionary<USBDevices, UniversalHardwareInfo> UniversalDeviceInfo = new Dictionary<USBDevices, UniversalHardwareInfo>()
        {
            {USBDevices.CorsairK100RGBKeyboard, new UniversalHardwareInfo("Corsair K100 RGB Keyboard", "1.0.3", "2023/02/06",  175)},
            {USBDevices.CorsairK60ProLowProfile,  new UniversalHardwareInfo("Corsair K60 pro Low profile keyboard", "1.1.0", "2023/03/05", 106)},
            {USBDevices.CorsairK70MKII,  new UniversalHardwareInfo("Corsair K70 MKII keyboard", "1.1.5", "2023/03/05", 115)},
            {USBDevices.DygmaRaise,  new UniversalHardwareInfo("Dygma Raise", "1.0.5", "2023/03/15", 139)},
            {USBDevices.HyperXAlloyCore,  new UniversalHardwareInfo("HyperX Alloy Core", "1.0.7", "2023/03/15", 102)},
            {USBDevices.RazerBlackwidowV3MiniKeyboard,  new UniversalHardwareInfo("Razer BlackWidow V3 Mini Keyboard", "2.0.1", "2023/02/11", 80)},
            {USBDevices.RazerHuntsmanV2TKL,  new UniversalHardwareInfo("Razer Huntsman V2 TKL", "1.7.0", "2023/03/15", 102)},
            {USBDevices.WhirlwindFxElement,  new UniversalHardwareInfo("WhirlwindFx Element", "1.0.2", "2023/03/15", 104)},
            {USBDevices.CNVS,  new UniversalHardwareInfo("HYTE qRGB CNVS (CES)", "1.0.5", "2023/03/20", 50)},
            {USBDevices.CNVSLeft,  new UniversalHardwareInfo("HYTE qRGB CNVS (MP1)", "1.0.5", "2023/03/20", 50)},
            {USBDevices.CNVSv1,  new UniversalHardwareInfo("HYTE qRGB CNVS (MP2)", "1.0.5", "2023/05/05", 50)},
            {USBDevices.IBPMiniHub,  new UniversalHardwareInfo("iBP Mini Hub", "1.1.5", "2023/01/20", 0)},
            {USBDevices.RazerBlackwidowV3Keyboard,  new UniversalHardwareInfo("Razer BlackWidow V3", "1.1.2", "2023/02/20", 104)},
            {USBDevices.RedragonKeeb,  new UniversalHardwareInfo("iBP KM7 Keyboard", "1.0.3", "2023/02/25", 24)},
            {USBDevices.Redragon_MK4,  new UniversalHardwareInfo("iBP MK4 Keyboard", "1.3.0", "2023/05/11", 126)},
            {USBDevices.RedragonMouse,  new UniversalHardwareInfo("iBP KM7 Mouse", "1.0.3", "2023/02/25", 12)},
            {USBDevices.MountainEverest,  new UniversalHardwareInfo("Mountain Everest Max", "1.1.0", "2023/07/06", 105)},
            //{USBDevices.SteelSeriesApex5,  new UniversalHardwareInfo("SteelSeries Apex5", "1.0.2", "2023/04/25", 106)},
            //{USBDevices.NanoleafShapes,  new UniversalHardwareInfo("Nanoleaf Shapes", "1.0.3", "2023/05/02", 0)},
            {USBDevices.RazerStriderChroma,  new UniversalHardwareInfo("Razer Strider Chroma", "1.3.1", "2023/05/15", 19)},
            //{USBDevices.PhilipsHueBridge,  new UniversalHardwareInfo("Philips Hue Bridge", "1.0.3", "2023/05/15", 0)},
            //{USBDevices.ASUSZ790A,  new UniversalHardwareInfo("Asus Z790A", "1.0.3", "2023/05/15", 1)},
            {USBDevices.CorsairLT100Tower,  new UniversalHardwareInfo("Corsair LT100 Tower", "1.0.0", "2023/07/06", 54)},
            {USBDevices.RazerBaseStationChroma,  new UniversalHardwareInfo("Razer Base Station Chroma", "1.0.0", "2023/07/06", 16)},
            {USBDevices.RazerMambaElite,  new UniversalHardwareInfo("Razer Mamba Elite", "1.0.0", "2023/07/06", 20)},
            {USBDevices.ViewSonicXG270QG,  new UniversalHardwareInfo("ViewSonic XG270QG", "1.0.0", "2023/07/14", 20)},
            {USBDevices.ViewSonicXG271QG,  new UniversalHardwareInfo("ViewSonic XG271QG", "1.0.0", "2023/07/14", 20)},
        };
    }

    public class ErrorWords
    {
        public const string INIT_FREQUENCY_FAILD = "Init Frequency Failed: The {0} must include {1} frequency band of colors.";
        public const string SET_FREQUENCY_FAILD = "Set Frequency Failed: The {0} of argument must contain {1} frequency band.";
        public const string LAYOUT_NOT_FOUND = "Init Frequency Faild: The {0} of the layout not found.";
        //public const string WITHOUT_POSITION = "Init Frequency Failed: The {0} set the position without 1~5.";
        public const string FULL_SET_LAYOUT_FAILD = "Init FullSetLayout Failed: The {0} must include 5 position, the argument only have {1} position.";
        //public const string INIT_PANORAMA_FREQUENCY_FAILD = "Init Panorama Frequency Failed: The {0} must include {1} frequency band of colors.";
        public const string SET_COLORS_FAILED = "Set Colors Failed: In the {0} layout only receive {2} colors ";
        public const string ENUM_NOT_HANDLE = "The Enum of {0} not handle in the method : {1}";
        public const string FREQUENCY_INVALID = "Frequency needs to be greater than 0, Frequency: {0}";
        public const string DICTIONARY_NOT_FOUND = "Dictionary not found with {0} from source {1}";
    }

    public class UniversalHardwareInfo
    {
        public string NAME { private set; get; }
        public string FIRMWARE_VERSION { private set; get; }
        public string RELEASE_DATE { private set; get; }
        public int LED_COUNT { private set; get; }

        public UniversalHardwareInfo(string name, string firmwareVersion, string releaseDate, int ledCount)
        {
            NAME = name;
            FIRMWARE_VERSION = firmwareVersion;
            RELEASE_DATE = releaseDate;
            LED_COUNT = ledCount;
        }
    }
}
