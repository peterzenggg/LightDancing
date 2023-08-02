using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ASRLIB_LedColor
    {
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ASRLIB_ControllerInfo
    {
        uint ControllerId;
        ASRLIB_ControllerType Type;
        uint FirmwareVersion;
        uint FirmwareDate;
        uint MaxLedChannels;
        public uint ActiveChannel;
        public ASRLIB_ChannelConfig ch0;
        public ASRLIB_ChannelConfig ch1;
        public ASRLIB_ChannelConfig ch2;
        public ASRLIB_ChannelConfig ch3;
        public ASRLIB_ChannelConfig ch4;
        public ASRLIB_ChannelConfig ch5;
        public ASRLIB_ChannelConfig ch6;
        public ASRLIB_ChannelConfig ch7;
    }
    public enum ASRLIB_ControllerType
    {
        RGB_CONTROLLER_MB = 0,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ASRLIB_ChannelConfig
    {
        public int MaxLeds;
        public bool RGSwap;
    }


    public enum ASRockType
    {
        LedStrip,
        Fan,
    }
    public class ASRockMode
    {
        ASRockType Type;
        int SettingLed;
        public ASRockMode(ASRockType Type)
        {
            this.Type = Type;
            SettingLed = 0;
        }
        public ASRockMode(ASRockType Type, int MaxLed)
        {
            this.Type = Type;
            this.SettingLed = MaxLed;
        }
    }
    internal class AsRockLedController
    {
        List<AsrockRGBChannel> ChannelController;
        HardwareModel model;
        public AsRockLedController(List<ASRockMode> Setting)
        {
            ChannelController = new List<AsrockRGBChannel>();
            ASRLIB_ControllerInfo Info = new ASRLIB_ControllerInfo();
            DLL.Polychrome_GetLedControllerInfo(ref Info);
            List<ASRLIB_ChannelConfig> ChConfig = new List<ASRLIB_ChannelConfig>();
            ChConfig.Add(Info.ch0);
            ChConfig.Add(Info.ch1);
            ChConfig.Add(Info.ch2);
            ChConfig.Add(Info.ch3);
            ChConfig.Add(Info.ch4);
            ChConfig.Add(Info.ch5);
            ChConfig.Add(Info.ch6);
            ChConfig.Add(Info.ch7);
            for (int i = 0; i < ChConfig.Count; i++)
            {
                bool enable = ((Info.ActiveChannel >> i) & 0x01) == 1;
                AsrockRGBChannel ch = new AsrockRGBChannel(i, ChConfig[i], enable);
                if(enable)
                    ChannelController.Add(ch);
            }

        }
        public void SetModel(HardwareModel model)
        {
            this.model = model;

        }

        public string GetMaxLed(int ch)
        {
            if (ChannelController.Count > ch)
            {
                return ChannelController[ch].GetMaxLed().ToString();
            }
            else
            {
                return "";
            }
        }


        public void Dispose()
        {
            DLL.Polychrome_BackToDefault();
            DLL.Polychrome_SDKRelease();
        }

        public static uint InitFunction()
        {
            return DLL.Polychrome_SDKInit();
        }

        #region DLL
        class DLL
        {
            private const string DLL_PATH = @"AsrPolychromeSDK64.dll";

            [DllImport(DLL_PATH)]
            public static extern uint Polychrome_SDKInit();
            [DllImport(DLL_PATH)]
            public static extern uint Polychrome_SDKRelease();

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe uint Polychrome_GetLedControllerCount(uint* Count);

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe uint Polychrome_GetLedControllerInfo(ASRLIB_ControllerInfo* Info);

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe uint Polychrome_SetLedColorConfig();

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint Polychrome_SetLedColors();

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint Polychrome_BackToDefault();

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint Polychrome_SaveUserData();

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint Polychrome_SetLedColorConfig(uint ChannelId, ASRLIB_LedColor[] LedColor, uint LedSize, uint Brightness);

            public static uint Polychrome_GetLedControllerInfo(ref ASRLIB_ControllerInfo Info)
            {
                unsafe
                {
                    fixed (ASRLIB_ControllerInfo* InfoP = &Info)
                    {
                        return Polychrome_GetLedControllerInfo(InfoP);
                    }
                }

            }
        }
        #endregion

        class AsrockRGBChannel
        {
            int Channel;
            bool Enable;
            int MaxLed;
            LightingBase LightingBase;

            public AsrockRGBChannel(int Channel, ASRLIB_ChannelConfig Config, bool Enable)
            {
                this.Channel = Channel;
                this.Enable = Enable;
                MaxLed = Config.MaxLeds;
            }

            public LightingBase GetLightingBase()
            {
                return LightingBase;
            }

            public void SetLightBase(LightingBase LightingBase)
            {
                this.LightingBase = LightingBase;
            }

            public int GetMaxLed()
            {
                return MaxLed;
            }
            public bool GetEnable()
            {
                return Enable;
            }
        }

        internal class ASRockLedStrip : LightingBase
        {
            /// <summary>
            /// The Y-Axis count of led board
            /// </summary>
            private const int KEYBOARD_YAXIS_COUNTS = 1;

            /// <summary>
            /// The X-Axis count of led board
            /// </summary>
            /// </summary>
            private int KEYBOARD_XAXIS_COUNTS;

            private int ch;

            List<ColorRGB> displayColors = new List<ColorRGB>();
            public ASRockLedStrip(HardwareModel USBModel, int XCount, int ch) : base(KEYBOARD_YAXIS_COUNTS, XCount, USBModel)
            {
                this.ch = ch;
                _model = InitModel();
            }

            protected override LightingModel InitModel()
            {
                SetKeyLayout();
                return new LightingModel()
                {
                    Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                    FirmwareVersion = _usbModel.FirmwareVersion,
                    DeviceID = _usbModel.DeviceID + ch,
                    USBDeviceType = _usbModel.USBDeviceType,
                    Type = LightingDevices.SmartLedStrip,
                    Name = "LedStrip" + KEYBOARD_XAXIS_COUNTS,
                };
            }
            protected override void SetKeyLayout()
            {
                KeyboardLayout = new List<List<Keyboard>>();
            }
            protected override void ProcessColor(ColorRGB[,] colorMatrix)
            {
                displayColors.Clear();
                for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
                {
                    displayColors.Add(colorMatrix[0, i]);
                }
            }
            protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
            {
            }
            protected override void TurnOffLed()
            {
                displayColors.Clear();
                for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
                {
                    displayColors.Add(ColorRGB.Black());
                }

            }
        }

        internal class AsRockFan : LightingBase
        {
            /// <summary>
            /// The Y-Axis count of led board
            /// </summary>
            private const int KEYBOARD_YAXIS_COUNTS = 5;

            /// <summary>
            /// The X-Axis count of led board
            /// </summary>
            /// </summary>
            private const int KEYBOARD_XAXIS_COUNTS = 5;

            private int ch;


            List<ColorRGB> displayColors = new List<ColorRGB>();

            private readonly Tuple<int, int>[] KEYS_LAYOUTS = new Tuple<int, int>[]
            {
                Tuple.Create(2, 4),
                Tuple.Create(3, 3),
                Tuple.Create(4, 2),
                Tuple.Create(3, 1),
                Tuple.Create(2, 0),
                Tuple.Create(1, 1),
                Tuple.Create(0, 2),
                Tuple.Create(1, 3),
            };
            public AsRockFan(HardwareModel USBModel, int ch) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, USBModel)
            {
                this.ch = ch;
                _model = InitModel();
            }
            protected override LightingModel InitModel()
            {
                SetKeyLayout();
                return new LightingModel()
                {
                    Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                    FirmwareVersion = _usbModel.FirmwareVersion,
                    DeviceID = _usbModel.DeviceID + ch,
                    USBDeviceType = _usbModel.USBDeviceType,
                    Type = LightingDevices.SmartLedStrip,
                    Name = "Fan" + ch,
                };
            }
            protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
            {
            }
            protected override void ProcessColor(ColorRGB[,] colorMatrix)
            {
                displayColors.Clear();
                foreach (Tuple<int, int> item in KEYS_LAYOUTS)
                {
                    displayColors.Add(colorMatrix[item.Item1, item.Item2]);
                }
            }
            protected override void SetKeyLayout()
            {
                KeyboardLayout = new List<List<Keyboard>>();
            }

            protected override void TurnOffLed()
            {
                displayColors.Clear();
                foreach (Tuple<int, int> item in KEYS_LAYOUTS)
                {
                    displayColors.Add(ColorRGB.Black());
                }
            }
        }

    }
}
