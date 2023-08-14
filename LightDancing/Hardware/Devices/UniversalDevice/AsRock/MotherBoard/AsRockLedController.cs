using LightDancing.Hardware.Devices.Components;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard
{
    public class ASRockMode
    {
        private ASRockType _type;
        public ASRockType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public int SettingLed
        {
            get
            {
                return _settingLed;
            }
            set
            {
                if (value < _maxLed)
                {
                    _settingLed = value;
                }
                else
                {
                    _settingLed = _maxLed;
                }
            }
        }

        private int _settingLed;
        private readonly int _channel;
        public int Channel
        {
            get
            {
                return _channel;
            }
        }

        private readonly int _maxLed;
        public int MaxLed
        {
            get
            {
                return _maxLed;
            }
        }

        public ASRockMode(int channel, ASRockType type, int maxLed, int settingLed)
        {
            _channel = channel;
            _maxLed = maxLed;
            _settingLed = settingLed;
            _type = type;
        }

    }

    internal class ASRockRGBChannel
    {
        private readonly ASRockMode _mode;
        private LightingBase _lightBase;

        public ASRockRGBChannel(int channel, ASRLIB_ChannelConfig config)
        {
            _mode = new ASRockMode(channel, ASRockType.LedStrip, config.MaxLeds, config.MaxLeds);
        }

        public ASRockMode GetMode()
        {
            return _mode;
        }

        public void ChangeMode(HardwareModel usbModel)
        {
            switch (_mode.Type)
            {
                case ASRockType.LedStrip:
                    _lightBase = new ASRockLedStrip(_mode.SettingLed, _mode.Channel, usbModel);
                    break;
                case ASRockType.Fan:
                    _lightBase = new ASRockLightFan(_mode.Channel, usbModel);
                    break;
                default:
                    _lightBase = null;
                    break;
            }
        }

        public LightingBase GetLightingBase()
        {
            return _lightBase;
        }

    }

    public class ASRockLedController
    {
        private readonly List<ASRockRGBChannel> _channelController;
        private readonly HardwareModel _model;
        private List<ASRockMode> _modeList;

        public ASRockLedController(HardwareModel model)
        {
            _channelController = new List<ASRockRGBChannel>();
            _modeList = new List<ASRockMode>();
            _model = model;
            ASRLIB_ControllerInfo info = new ASRLIB_ControllerInfo();
            ASRockLedDll.Polychrome_GetLedControllerInfo(ref info);
            List<ASRLIB_ChannelConfig> channelConfigs = new List<ASRLIB_ChannelConfig>
            {
                info.ch0,
                info.ch1,
                info.ch2,
                info.ch3,
                info.ch4,
                info.ch5,
                info.ch6,
                info.ch7
            };
            for (int i = 0; i < channelConfigs.Count; i++)
            {
                bool enable = ((info.ActiveChannel >> i) & 0x01) == 1;
                if (enable)
                {
                    ASRockRGBChannel channel = new ASRockRGBChannel(i, channelConfigs[i]);
                    _channelController.Add(channel);
                    _modeList.Add(channel.GetMode());
                }
            }
        }

        public List<ASRockMode> GetModeDeepList()
        {
            List<ASRockMode> result = new List<ASRockMode>();
            foreach (ASRockMode mode in _modeList)
            {
                if (mode.Type != ASRockType.None)
                    result.Add(new ASRockMode(mode.Channel, mode.Type, mode.MaxLed, mode.SettingLed));
            }
            return result;
        }

        public List<ASRockMode> GetModeList()
        {
            return _modeList;
        }

        public void SetModeList(List<ASRockMode> settingList)
        {
            _modeList = settingList;
        }

        public List<LightingBase> ChangeCommit()
        {
            List<LightingBase> results = new List<LightingBase>();
            foreach (ASRockRGBChannel rGBChannel in _channelController)
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
            ASRLIB_LedPattern pattern = new ASRLIB_LedPattern();
            pattern.PatternId = 0x0E;
            pattern.ApplyAll = 1;
            ASRockLedDll.Polychrome_SetLedPattern(0, ref pattern);
        }

        public void Dispose()
        {
            //DLL.Polychrome_BackToDefault();
            ASRockLedDll.Polychrome_SDKRelease();
        }

        public static uint InitFunction()
        {
            return ASRockLedDll.Polychrome_SDKInit();
        }

        public static uint Polychrome_SetLedColorConfig(uint ChannelId, ASRLIB_LedColor[] LedColor, uint LedSize)
        {
            return ASRockLedDll.Polychrome_SetLedColorConfig(ChannelId, LedColor, LedSize, 100);
        }

        public static uint Polychrome_SetLedColors()
        {
            return ASRockLedDll.Polychrome_SetLedColors();
        }

    }
}
