using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard.ASRockLedController;

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
        public byte RGSwap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ASRLIB_LedPattern
    {
        public byte PatternId;
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
        public byte Speed;    // 0 ~ 255 (Fast ~ Slow) Only for Asrock MB.
        public byte ApplyAll;			// Only for Asrock MB.
    }

    public enum ASRockType
    {
        LedStrip,
        Fan,
        None,
    }

    public class ASRockMode
    {
        public ASRockType Type;
        public int SettingLed
        {
            get
            {
                return settingLed;
            }
            set
            {
                settingLed = value;
            }
        }
        private int settingLed;
        private int channel;
        public int Channel
        {
            get
            {
                return channel;
            }
        }
        private int maxLed;
        public int MaxLed
        {
            get
            {
                return maxLed;
            }
        }

        public ASRockMode(int channel, ASRockType type, int maxLed, int settingLed)
        {
            this.channel = channel;
            this.maxLed = maxLed;
            this.settingLed = settingLed;
            Type = type;
        }

        public int GetChanel()
        {
            return channel;
        }

        public void ChangeType(ASRockType Type)
        {
            this.Type = Type;
        }

        public void SetLed(int LedSet)
        {
            if (LedSet < maxLed)
            {
                //SettingLed = LedSet;
            }
            else
            {
                //SettingLed = maxLed;
            }
        }

        public int GetMaxLed()
        {
            return maxLed;
        }

    }

    internal class ASrockRGBChannel
    {
        bool enable;
        LightingBase lightBase;
        ASRockMode mode;

        public ASrockRGBChannel(int channel, ASRLIB_ChannelConfig config, bool enable)
        {
            this.enable = enable;
            if (enable)
            {
                mode = new ASRockMode(channel, ASRockType.LedStrip, config.MaxLeds, config.MaxLeds);
            }
        }

        public ASRockMode GetMode()
        {
            if (enable)
            {
                return mode;
            }
            else
            {
                return null;
            }
        }

        public void ChangeMode(HardwareModel usbModel)
        {
            if (enable)
            {
                switch (mode.Type)
                {
                    case ASRockType.LedStrip:
                        lightBase = new ASRockLedStrip(mode.SettingLed, mode.GetChanel(), usbModel);
                        break;
                    case ASRockType.Fan:
                        lightBase = new AsRockLightFan(mode.GetChanel(), usbModel);
                        break;
                    default:
                        lightBase = null;
                        break;
                }
            }
        }

        public LightingBase GetLightingBase()
        {
            if (enable)
            {
                return lightBase;
            }
            return null;
        }

    }
    public class ASRockLedController
    {
        private List<ASrockRGBChannel> _channelController;
        private List<ASRockMode> _modeList;
        private HardwareModel _model;

        public ASRockLedController(HardwareModel model)
        {
            _channelController = new List<ASrockRGBChannel>();
            _modeList = new List<ASRockMode>();
            _model = model;
            ASRLIB_ControllerInfo info = new ASRLIB_ControllerInfo();
            DLL.Polychrome_GetLedControllerInfo(ref info);
            List<ASRLIB_ChannelConfig> channelConfigs = new List<ASRLIB_ChannelConfig>();
            channelConfigs.Add(info.ch0);
            channelConfigs.Add(info.ch1);
            channelConfigs.Add(info.ch2);
            channelConfigs.Add(info.ch3);
            channelConfigs.Add(info.ch4);
            channelConfigs.Add(info.ch5);
            channelConfigs.Add(info.ch6);
            channelConfigs.Add(info.ch7);
            for (int i = 0; i < channelConfigs.Count; i++)
            {
                bool enable = ((info.ActiveChannel >> i) & 0x01) == 1;
                ASrockRGBChannel channel = new ASrockRGBChannel(i, channelConfigs[i], enable);
                _channelController.Add(channel);
            }
            foreach (ASrockRGBChannel channel in _channelController)
            {
                ASRockMode mode = channel.GetMode();
                if (mode != null)
                {
                    _modeList.Add(channel.GetMode());
                }
            }
        }

        public static uint Polychrome_SetLedColorConfig(uint ChannelId, ASRLIB_LedColor[] LedColor, uint LedSize)
        {
            return DLL.Polychrome_SetLedColorConfig(ChannelId, LedColor, LedSize, 100);
        }

        public static uint Polychrome_SetLedColors()
        {
            return DLL.Polychrome_SetLedColors();
        }

        public List<ASRockMode> GetModeDeepList()
        {
            List<ASRockMode> Result = new List<ASRockMode>();
            foreach (ASRockMode mode in _modeList)
            {
                if (mode.Type != ASRockType.None)
                    Result.Add(new ASRockMode(mode.GetChanel(), mode.Type, mode.GetMaxLed(), mode.SettingLed));
            }
            return Result;
        }

        public List<ASRockMode> GetModeList()
        {
            return _modeList;
        }

        public void SetModeList(List<ASRockMode> SettingList)
        {
            _modeList = SettingList;
        }

        public List<LightingBase> ChangeCommit()
        {
            List<LightingBase> results = new List<LightingBase>();
            foreach (ASrockRGBChannel rGBChannel in _channelController)
            {
                rGBChannel.ChangeMode(_model);
                LightingBase result = rGBChannel.GetLightingBase();
                if (result != null)
                {
                    results.Add(result);
                }
            }
            return results;
        }

        public void Dispose()
        {
            //DLL.Polychrome_BackToDefault();
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
            public static extern unsafe uint Polychrome_GetLedPattern(uint ChannelId, ASRLIB_LedPattern* LedPattern);

            [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe uint Polychrome_SetLedPattern(uint ChannelId, ASRLIB_LedPattern* LedPattern);

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
                    fixed (ASRLIB_ControllerInfo* InfoPointer = &Info)
                    {
                        return Polychrome_GetLedControllerInfo(InfoPointer);
                    }
                }

            }

            public static uint Polychrome_GetLedPattern(uint ChannelId, ref ASRLIB_LedPattern LedPattern)
            {
                unsafe
                {
                    fixed (ASRLIB_LedPattern* PatternPointer = &LedPattern)
                    {
                        return Polychrome_GetLedPattern(ChannelId, PatternPointer);
                    }
                }
            }

            public static uint Polychrome_SetLedPattern(uint ChannelId, ref ASRLIB_LedPattern LedPattern)
            {
                unsafe
                {
                    fixed (ASRLIB_LedPattern* PatternPointer = &LedPattern)
                    {
                        return Polychrome_SetLedPattern(ChannelId, PatternPointer);
                    }
                }
            }
        }
        #endregion


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

            private int channel;

            public ASRockLedStrip(int XCount, int channel, HardwareModel usbModel) : base(KEYBOARD_YAXIS_COUNTS, XCount, usbModel)
            {
                this.channel = channel;
                KEYBOARD_XAXIS_COUNTS = XCount;
                _model = InitModel();
            }

            protected override LightingModel InitModel()
            {
                SetKeyLayout();
                return new LightingModel()
                {
                    Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                    FirmwareVersion = _usbModel.FirmwareVersion,
                    DeviceID = _usbModel.DeviceID + channel,
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
                List<byte> collectBytes = new List<byte>();
                for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
                {
                    ColorRGB colorRGB = colorMatrix[0, i];
                    collectBytes.Add(colorRGB.R);
                    collectBytes.Add(colorRGB.G);
                    collectBytes.Add(colorRGB.B);
                }
                _displayColorBytes = collectBytes;
            }

            protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
            {
            }

            protected override void TurnOffLed()
            {
                _displayColorBytes.Clear();
                for (int i = 0; i < KEYBOARD_XAXIS_COUNTS; i++)
                {
                    _displayColorBytes.Add(0x00);
                    _displayColorBytes.Add(0x00);
                    _displayColorBytes.Add(0x00);
                }
            }

        }

        internal class AsRockLightFan : LightingBase
        {
            /// <summary>
            /// The Y-Axis count of led board
            /// </summary>
            private const int KEYBOARD_YAXIS_COUNTS = 5;

            /// <summary>
            /// The X-Axis count of led board
            /// </summary>
            /// </summary>
            private const int KEYBOARD_XAXIS_COUNTS = 4;

            private int channel;
            private readonly Tuple<int, int>[] KEYS_LAYOUTS = new Tuple<int, int>[]
            {
                Tuple.Create(0, 3),
                Tuple.Create(0, 2),
                Tuple.Create(0, 1),
                Tuple.Create(0, 0),
                Tuple.Create(1, 0),
                Tuple.Create(2, 0),
                Tuple.Create(3, 0),
                Tuple.Create(4, 0),
                Tuple.Create(4, 1),
                Tuple.Create(4, 2),
                Tuple.Create(4, 3),
                Tuple.Create(3, 3),
                Tuple.Create(2, 3),
                Tuple.Create(1, 3),
                Tuple.Create(0, 3),
                Tuple.Create(0, 3),
            };

            public AsRockLightFan(int channel, HardwareModel usbModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, usbModel)
            {
                this.channel = channel;
                _model = InitModel();
            }

            protected override LightingModel InitModel()
            {
                SetKeyLayout();
                return new LightingModel()
                {
                    Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                    FirmwareVersion = _usbModel.FirmwareVersion,
                    DeviceID = _usbModel.DeviceID + channel,
                    USBDeviceType = _usbModel.USBDeviceType,
                    Type = LightingDevices.SmartLedStrip,
                    Name = "Fan" + channel,
                };
            }

            protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
            {
            }

            protected override void ProcessColor(ColorRGB[,] colorMatrix)
            {
                List<byte> collectBytes = new List<byte>();
                foreach (Tuple<int, int> item in KEYS_LAYOUTS)
                {
                    ColorRGB colorRGB = colorMatrix[item.Item1, item.Item2];
                    collectBytes.Add(colorRGB.R);
                    collectBytes.Add(colorRGB.G);
                    collectBytes.Add(colorRGB.B);
                }
                _displayColorBytes = collectBytes;
            }

            protected override void SetKeyLayout()
            {
                KeyboardLayout = new List<List<Keyboard>>();
            }

            protected override void TurnOffLed()
            {
                List<byte> collectBytes = new List<byte>();
                foreach (Tuple<int, int> item in KEYS_LAYOUTS)
                {
                    collectBytes.Add(0x00);
                    collectBytes.Add(0x00);
                    collectBytes.Add(0x00);
                }
                _displayColorBytes = collectBytes;
            }
        }

    }
}
