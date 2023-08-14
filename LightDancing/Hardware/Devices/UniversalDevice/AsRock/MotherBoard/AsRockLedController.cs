using LightDancing.Hardware.Devices.Components;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public class AsrockMode
    {
        private ASRockType type;
        public ASRockType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        public int SettingLed
        {
            get
            {
                return settingLed;
            }
            set
            {
                if (value < maxLed)
                {
                    settingLed = value;
                }
                else
                {
                    settingLed = maxLed;
                }
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

        public AsrockMode(int channel, ASRockType type, int maxLed, int settingLed)
        {
            this.channel = channel;
            this.maxLed = maxLed;
            this.settingLed = settingLed;
            Type = type;
        }

    }

    internal class AsrockRGBChannel
    {
        LightingBase lightBase;
        AsrockMode mode;

        public AsrockRGBChannel(int channel, ASRLIB_ChannelConfig config)
        {
            mode = new AsrockMode(channel, ASRockType.LedStrip, config.MaxLeds, config.MaxLeds);
        }

        public AsrockMode GetMode()
        {
            return mode;
        }

        public void ChangeMode(HardwareModel usbModel)
        {
            switch (mode.Type)
            {
                case ASRockType.LedStrip:
                    lightBase = new AsrockLedStrip(mode.SettingLed, mode.Channel, usbModel);
                    break;
                case ASRockType.Fan:
                    lightBase = new AsrockLightFan(mode.Channel, usbModel);
                    break;
                default:
                    lightBase = null;
                    break;
            }
        }

        public LightingBase GetLightingBase()
        {
            return lightBase;
        }
        
    }

    public class AsrockLedController
    {
        private List<AsrockRGBChannel> _channelController;
        private List<AsrockMode> _modeList;
        private HardwareModel _model;

        public AsrockLedController(HardwareModel model)
        {
            _channelController = new List<AsrockRGBChannel>();
            _modeList = new List<AsrockMode>();
            _model = model;
            ASRLIB_ControllerInfo info = new ASRLIB_ControllerInfo();
            AsrockLedDll.Polychrome_GetLedControllerInfo(ref info);
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
                if (enable)
                {
                    AsrockRGBChannel channel = new AsrockRGBChannel(i, channelConfigs[i]);
                    _channelController.Add(channel);
                    _modeList.Add(channel.GetMode());
                }
            }
        }

        public List<AsrockMode> GetModeDeepList()
        {
            List<AsrockMode> Result = new List<AsrockMode>();
            foreach (AsrockMode mode in _modeList)
            {
                if (mode.Type != ASRockType.None)
                    Result.Add(new AsrockMode(mode.Channel, mode.Type, mode.MaxLed, mode.SettingLed));
            }
            return Result;
        }

        public List<AsrockMode> GetModeList()
        {
            return _modeList;
        }

        public void SetModeList(List<AsrockMode> SettingList)
        {
            _modeList = SettingList;
        }

        public List<LightingBase> ChangeCommit()
        {
            List<LightingBase> results = new List<LightingBase>();
            foreach (AsrockRGBChannel rGBChannel in _channelController)
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
        public void TurnBackToFW()
        {
            ASRLIB_LedPattern Pattern = new ASRLIB_LedPattern();
            Pattern.PatternId = 0x0E;
            Pattern.ApplyAll = 1;
            AsrockLedDll.Polychrome_SetLedPattern(0, ref Pattern);
        }


        public void Dispose()
        {
            //DLL.Polychrome_BackToDefault();
            AsrockLedDll.Polychrome_SDKRelease();
        }

        public static uint InitFunction()
        {
            return AsrockLedDll.Polychrome_SDKInit();
        }

        public static uint Polychrome_SetLedColorConfig(uint ChannelId, ASRLIB_LedColor[] LedColor, uint LedSize)
        {
            return AsrockLedDll.Polychrome_SetLedColorConfig(ChannelId, LedColor, LedSize, 100);
        }

        public static uint Polychrome_SetLedColors()
        {
            return AsrockLedDll.Polychrome_SetLedColors();
        }

    }
}
