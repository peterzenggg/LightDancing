using System.Runtime.InteropServices;

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
    internal struct ASRLIB_ControllerInfo
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

    internal enum ASRLIB_ControllerType
    {
        RGB_CONTROLLER_MB = 0,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ASRLIB_ChannelConfig
    {
        public int MaxLeds;
        public byte RGSwap;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ASRLIB_LedPattern
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

    internal class ASRockLedDll
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

        public static uint Polychrome_GetLedControllerInfo(ref ASRLIB_ControllerInfo info)
        {
            unsafe
            {
                fixed (ASRLIB_ControllerInfo* infoPointer = &info)
                {
                    return Polychrome_GetLedControllerInfo(infoPointer);
                }
            }
        }

        public static uint Polychrome_GetLedPattern(uint channelId, ref ASRLIB_LedPattern ledPattern)
        {
            unsafe
            {
                fixed (ASRLIB_LedPattern* patternPointer = &ledPattern)
                {
                    return Polychrome_GetLedPattern(channelId, patternPointer);
                }
            }
        }

        public static uint Polychrome_SetLedPattern(uint channelId, ref ASRLIB_LedPattern ledPattern)
        {
            unsafe
            {
                fixed (ASRLIB_LedPattern* patternPointer = &ledPattern)
                {
                    return Polychrome_SetLedPattern(channelId, patternPointer);
                }
            }
        }
    }
}
