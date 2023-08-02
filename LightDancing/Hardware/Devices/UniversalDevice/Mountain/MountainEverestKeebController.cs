using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Linq;

namespace LightDancing.Hardware.Devices.UniversalDevice.Mountain
{
    public class MountainEverestKeebController: ILightingControl
    {
        private const int DEVICE_VID = 0x3282;
        private const int DEVICE_PID = 0x0001;
        private const int MAX_FEATURE_LENGTH = 65;

        public List<USBDeviceBase> InitDevices()
        {
            try
            {
                List<HidStream> streams = new HidDetector().GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH);

                if (streams != null && streams.Any())
                {
                    List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                    for (int i = 0; i < streams.Count; i++)
                    {
                        MountainEverestDevice keyboard = new MountainEverestDevice(streams[i], i);
                        hardwares.Add(keyboard);
                    }

                    return hardwares;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MountainEverestKeebController/InitDevices is failed. Exception: {ex}.");
            }

            return null;
        }
    }

    public class MountainEverestDevice: USBDeviceBase
    {
        private const int MAX_SPLIT_BUFFER = 399;
        private const int COMMOND_COUNT = 8;
        private const int MAX_FEATURE_LENGTH = 65;
        private const int GET_BUFFER_SIZE = 57;
        private const int MID_KEYBOARD_XCOUNT = 19;
        private const int RL_KEYBOARD_XCOUNT = 23;
        private bool _isUIControl = false;
        private readonly HidStream _hidStrem;

        public MountainEverestDevice(HidStream deviceStream, int index) : base(deviceStream, $"Mountain Everest {index}")
        {
            _hidStrem = deviceStream;
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                USBDeviceType = USBDevices.MountainEverest,
                Name = "Mountain Everest"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return new List<LightingBase>() { new MountainEverestKeeb(_model, MountainEverestNumericPadSide.Mid, MID_KEYBOARD_XCOUNT, _serialID) }; ;
        }

        public void ChangeSide(MountainEverestNumericPadSide side)
        {
            try
            {
                List<LightingBase> hardwares = new List<LightingBase>()
                {
                    new MountainEverestKeeb(_model, side, side == MountainEverestNumericPadSide.Mid ? MID_KEYBOARD_XCOUNT : RL_KEYBOARD_XCOUNT, _serialID)
                };
                this._lightingBase = hardwares;
                HardwaresDetector.Instance.ChangeMountainSide((MountainEverestKeeb)hardwares.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MountainEverestDevice/ChangeSide is failed. Exception: {ex}.");
            }
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            try
            {
                if (!_isUIControl)
                {
                    try
                    {
                        SetCurrentLedEffectOff();
                        _isUIControl = true;
                    }
                    catch
                    {
                        Console.WriteLine("MountainEverestDevice/SendToHardware/SetCurrentLedEffectOff is failed. Waiting for next SendToHardware time.");
                        return;
                    }
                }

                if (_lightingBase == null || !_lightingBase.Any())
                {
                    Console.WriteLine("MountainEverestDevice/SendToHardware'_lightingBase is null. Waiting for next SendToHardware time.");
                    return;
                }

                var lightingBase = _lightingBase.FirstOrDefault();

                if (process)
                {
                    lightingBase.ProcessStreaming(false, brightness);
                }

                List<byte> colorList = lightingBase.GetDisplayColors();
                if (colorList != null)
                {
                    colorList.PadListWithZeros(MAX_SPLIT_BUFFER);

                    for (int i = 0; i < COMMOND_COUNT; i++)
                    {
                        List<byte> commandCollect = new List<byte>
                                {
                                    0x00,
                                    0x14,
                                    0x2c,
                                    0x00,
                                    0x01,
                                    Convert.ToByte(i),
                                    0x4b,
                                    0x00
                                };

                        int startPosistion = i * GET_BUFFER_SIZE;

                        if (startPosistion < MAX_SPLIT_BUFFER)
                        {
                            commandCollect.AddRange(colorList.GetRange(startPosistion, GET_BUFFER_SIZE));
                        }

                        commandCollect.PadListWithZeros(MAX_FEATURE_LENGTH);

                        try
                        {
                            _deviceStream.Write(commandCollect.ToArray());
                            _hidStrem.Read();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"MountainEverestDevice/SendToHardware is failed. EX:{e}.");
            }
        }

        public void SetCurrentLedEffectOff()
        {
            try
            {
                byte[] sendArray = new byte[MAX_FEATURE_LENGTH];
                sendArray[1] = 0x14;
                sendArray[5] = 0x01;
                sendArray[6] = 0x06;
                _deviceStream.Write(sendArray);
                _hidStrem.Read();

                sendArray = new byte[MAX_FEATURE_LENGTH] {0x00, 0x14, 0x2c, 0x0a, 0x00, 0xff, 0x64, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};
                _deviceStream.Write(sendArray);
                _hidStrem.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MountainEverestDevice/SetCurrentLedEffectOff is failed. EX:{ex}.");
            }
        }   
    }

    public class MountainEverestKeeb: LightingBase
    {
        /// <summary>
        /// Keyboard Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 6;

        private const int CONTINUE_ITEM = 99;

        /// <summary>
        /// Process Mapping in Task
        /// </summary>
        private readonly Dictionary<int, Tuple<int, int>> LAYOUT_MAPPING;

        /// <summary>
        /// Number Keeb in Right Side
        /// Tuple is keyboard X Axis Y Axis
        /// CONTINUE_ITEM is must be insert RGB(0,0,0) to stream posistion
        /// </summary>
        private static readonly Dictionary<int, Tuple<int, int>> RIGHT_LAYOUT = new Dictionary<int, Tuple<int, int>>()
        {
            { 0,Tuple.Create(0,0)},
            { 1,Tuple.Create(1,0)},
            { 2,Tuple.Create(2,0)},
            { 3,Tuple.Create(3,0)},
            { 4,Tuple.Create(4,0)},
            { 5,Tuple.Create(5,0)},
            { 6,Tuple.Create(2,19)},
            { 7,Tuple.Create(3,22)},
            { 8,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 9,Tuple.Create(0,2)},
            { 10,Tuple.Create(1,1)},
            { 11,Tuple.Create(2,2)},
            { 12,Tuple.Create(3,2)},
            { 13,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 14,Tuple.Create(5,1)},
            { 15,Tuple.Create(2,22)},
            { 16,Tuple.Create(2,21)},
            { 17,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 18,Tuple.Create(0,3)},
            { 19,Tuple.Create(1,2)},
            { 20,Tuple.Create(2,3)},
            { 21,Tuple.Create(3,3)},
            { 22,Tuple.Create(4,2)},
            { 23,Tuple.Create(5,2)},
            { 24,Tuple.Create(2,20)},
            { 25,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 26,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 27,Tuple.Create(0,4)},
            { 28,Tuple.Create(1,3)},
            { 29,Tuple.Create(2,4)},
            { 30,Tuple.Create(3,4)},
            { 31,Tuple.Create(4,3)},
            { 32,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 33,Tuple.Create(4,22)},
            { 34,Tuple.Create(4,19)},
            { 35,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 36,Tuple.Create(0,5)},
            { 37,Tuple.Create(1,4)},
            { 38,Tuple.Create(2,5)},
            { 39,Tuple.Create(3,5)},
            { 40,Tuple.Create(4,4)},
            { 41,Tuple.Create(5,7)},
            { 42,Tuple.Create(4,20)},
            { 43,Tuple.Create(4,21)},
            { 44,Tuple.Create(3,2)},
            { 45,Tuple.Create(0,7)},
            { 46,Tuple.Create(1,5)},
            { 47,Tuple.Create(2,7)},
            { 48,Tuple.Create(3,7)},
            { 49,Tuple.Create(4,5)},
            { 50,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 51,Tuple.Create(3,19)},
            { 52,Tuple.Create(3,20)},
            { 53,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 54,Tuple.Create(0,8)},
            { 55,Tuple.Create(1,6)},
            { 56,Tuple.Create(2,8)},
            { 57,Tuple.Create(3,8)},
            { 58,Tuple.Create(4,7)},
            { 59,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 60,Tuple.Create(3,21)},
            { 61,Tuple.Create(2,19)},
            { 62,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 63,Tuple.Create(0,9)},
            { 64,Tuple.Create(1,7)},
            { 65,Tuple.Create(2,9)},
            { 66,Tuple.Create(3,9)},
            { 67,Tuple.Create(4,9)},
            { 68,Tuple.Create(5,12)},
            { 69,Tuple.Create(2,20)},
            { 70,Tuple.Create(2,21)},
            { 71,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 72,Tuple.Create(0,10)},
            { 73,Tuple.Create(1,8)},
            { 74,Tuple.Create(2,10)},
            { 75,Tuple.Create(3,10)},
            { 76,Tuple.Create(4,10)},
            { 77,Tuple.Create(5,13)},
            { 78,Tuple.Create(5,19)},
            { 79,Tuple.Create(5,21)},
            { 80,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 81,Tuple.Create(0,12)},
            { 82,Tuple.Create(1,9)},
            { 83,Tuple.Create(2,11)},
            { 84,Tuple.Create(3,11)},
            { 85,Tuple.Create(4,11)},
            { 86,Tuple.Create(5,14)},
            { 87,Tuple.Create(1,14)},
            { 88,Tuple.Create(2,16)},
            { 89,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 90,Tuple.Create(0,13)},
            { 91,Tuple.Create(1,10)},
            { 92,Tuple.Create(2,12)},
            { 93,Tuple.Create(3,12)},
            { 94,Tuple.Create(4,12)},
            { 95,Tuple.Create(5,15)},
            { 96,Tuple.Create(1,16)},
            { 97,Tuple.Create(2,17)},
            { 98,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 99,Tuple.Create(0,14)},
            { 100,Tuple.Create(1,12)},
            { 101,Tuple.Create(2,13)},
            { 102,Tuple.Create(3,13)},
            { 103,Tuple.Create(4,13)},
            { 104,Tuple.Create(5,16)},
            { 105,Tuple.Create(1,17)},
            { 106,Tuple.Create(2,18)},
            { 107,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 108,Tuple.Create(0,15)},
            { 109,Tuple.Create(1,13)},
            { 110,Tuple.Create(2,14)},
            { 111,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 112,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 113,Tuple.Create(5,17)},
            { 114,Tuple.Create(0,17)},
            { 115,Tuple.Create(1,18)},
            { 116,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 117,Tuple.Create(0,16)},
            { 118,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 119,Tuple.Create(2,15)},
            { 120,Tuple.Create(3,15)},
            { 121,Tuple.Create(4,14)},
            { 122,Tuple.Create(5,18)},
            { 123,Tuple.Create(0,18)},
            { 124,Tuple.Create(4,17)},
        };

        /// <summary>
        /// Number Keeb in Left Side
        /// </summary>
        private static readonly Dictionary<int, Tuple<int, int>> LEFT_LAYOUT = new Dictionary<int, Tuple<int, int>>()
        {
            { 0,Tuple.Create(0,4)},
            { 1,Tuple.Create(1,4)},
            { 2,Tuple.Create(2,4)},
            { 3,Tuple.Create(3,4)},
            { 4,Tuple.Create(4,4)},
            { 5,Tuple.Create(5,4)},
            { 6,Tuple.Create(1,0)},
            { 7,Tuple.Create(2,3)},
            { 8,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 9,Tuple.Create(0,6)},
            { 10,Tuple.Create(1,5)},
            { 11,Tuple.Create(2,6)},
            { 12,Tuple.Create(3,6)},
            { 13,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 14,Tuple.Create(5,5)},
            { 15,Tuple.Create(2,3)},
            { 16,Tuple.Create(2,2)},
            { 17,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 18,Tuple.Create(0,7)},
            { 19,Tuple.Create(1,6)},
            { 20,Tuple.Create(2,7)},
            { 21,Tuple.Create(3,7)},
            { 22,Tuple.Create(4,6)},
            { 23,Tuple.Create(5,6)},
            { 24,Tuple.Create(2,1)},
            { 25,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 26,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 27,Tuple.Create(0,8)},
            { 28,Tuple.Create(1,7)},
            { 29,Tuple.Create(2,8)},
            { 30,Tuple.Create(3,8)},
            { 31,Tuple.Create(4,7)},
            { 32,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 33,Tuple.Create(4,3)},
            { 34,Tuple.Create(4,0)},
            { 35,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 36,Tuple.Create(0,9)},
            { 37,Tuple.Create(1,8)},
            { 38,Tuple.Create(2,9)},
            { 39,Tuple.Create(3,9)},
            { 40,Tuple.Create(4,8)},
            { 41,Tuple.Create(5,11)},
            { 42,Tuple.Create(4,1)},
            { 43,Tuple.Create(4,2)},
            { 44,Tuple.Create(3,6)},
            { 45,Tuple.Create(0,11)},
            { 46,Tuple.Create(1,9)},
            { 47,Tuple.Create(2,11)},
            { 48,Tuple.Create(3,11)},
            { 49,Tuple.Create(4,9)},
            { 50,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 51,Tuple.Create(3,0)},
            { 52,Tuple.Create(3,1)},
            { 53,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 54,Tuple.Create(0,12)},
            { 55,Tuple.Create(1,10)},
            { 56,Tuple.Create(2,12)},
            { 57,Tuple.Create(3,12)},
            { 58,Tuple.Create(4,11)},
            { 59,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 60,Tuple.Create(3,2)},
            { 61,Tuple.Create(2,0)},
            { 62,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 63,Tuple.Create(0,13)},
            { 64,Tuple.Create(1,11)},
            { 65,Tuple.Create(2,13)},
            { 66,Tuple.Create(3,13)},
            { 67,Tuple.Create(4,13)},
            { 68,Tuple.Create(5,16)},
            { 69,Tuple.Create(2,1)},
            { 70,Tuple.Create(2,2)},
            { 71,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 72,Tuple.Create(0,14)},
            { 73,Tuple.Create(1,12)},
            { 74,Tuple.Create(2,14)},
            { 75,Tuple.Create(3,14)},
            { 76,Tuple.Create(4,14)},
            { 77,Tuple.Create(5,17)},
            { 78,Tuple.Create(5,0)},
            { 79,Tuple.Create(5,2)},
            { 80,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 81,Tuple.Create(0,16)},
            { 82,Tuple.Create(1,13)},
            { 83,Tuple.Create(2,15)},
            { 84,Tuple.Create(3,15)},
            { 85,Tuple.Create(4,15)},
            { 86,Tuple.Create(5,18)},
            { 87,Tuple.Create(1,18)},
            { 88,Tuple.Create(2,20)},
            { 89,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 90,Tuple.Create(0,17)},
            { 91,Tuple.Create(1,14)},
            { 92,Tuple.Create(2,16)},
            { 93,Tuple.Create(3,16)},
            { 94,Tuple.Create(4,16)},
            { 95,Tuple.Create(5,19)},
            { 96,Tuple.Create(1,20)},
            { 97,Tuple.Create(2,21)},
            { 98,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 99,Tuple.Create(0,18)},
            { 100,Tuple.Create(1,16)},
            { 101,Tuple.Create(2,17)},
            { 102,Tuple.Create(3,17)},
            { 103,Tuple.Create(4,17)},
            { 104,Tuple.Create(5,20)},
            { 105,Tuple.Create(1,21)},
            { 106,Tuple.Create(2,22)},
            { 107,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 108,Tuple.Create(0,19)},
            { 109,Tuple.Create(1,17)},
            { 110,Tuple.Create(2,18)},
            { 111,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 112,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 113,Tuple.Create(5,21)},
            { 114,Tuple.Create(0,21)},
            { 115,Tuple.Create(1,22)},
            { 116,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 117,Tuple.Create(0,20)},
            { 118,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 119,Tuple.Create(2,19)},
            { 120,Tuple.Create(3,19)},
            { 121,Tuple.Create(4,18)},
            { 122,Tuple.Create(5,22)},
            { 123,Tuple.Create(0,22)},
            { 124,Tuple.Create(4,21)},
        };

        /// <summary>
        /// Number Keeb in Left Side
        /// </summary>
        private static readonly Dictionary<int, Tuple<int, int>> MID_LAYOUT = new Dictionary<int, Tuple<int, int>>()
        {
            { 0,Tuple.Create(0,0)},
            { 1,Tuple.Create(1,0)},
            { 2,Tuple.Create(2,0)},
            { 3,Tuple.Create(3,0)},
            { 4,Tuple.Create(4,0)},
            { 5,Tuple.Create(5,0)},
            { 6,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 7,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 8,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 9,Tuple.Create(0,2)},
            { 10,Tuple.Create(1,1)},
            { 11,Tuple.Create(2,2)},
            { 12,Tuple.Create(3,2)},
            { 13,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 14,Tuple.Create(5,1)},
            { 15,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 16,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 17,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 18,Tuple.Create(0,3)},
            { 19,Tuple.Create(1,2)},
            { 20,Tuple.Create(2,3)},
            { 21,Tuple.Create(3,3)},
            { 22,Tuple.Create(4,2)},
            { 23,Tuple.Create(5,2)},
            { 24,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 25,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 26,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 27,Tuple.Create(0,4)},
            { 28,Tuple.Create(1,3)},
            { 29,Tuple.Create(2,4)},
            { 30,Tuple.Create(3,4)},
            { 31,Tuple.Create(4,3)},
            { 32,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 33,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 34,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 35,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 36,Tuple.Create(0,5)},
            { 37,Tuple.Create(1,4)},
            { 38,Tuple.Create(2,5)},
            { 39,Tuple.Create(3,5)},
            { 40,Tuple.Create(4,4)},
            { 41,Tuple.Create(5,7)},
            { 42,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 43,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 44,Tuple.Create(3,2)},
            { 45,Tuple.Create(0,7)},
            { 46,Tuple.Create(1,5)},
            { 47,Tuple.Create(2,7)},
            { 48,Tuple.Create(3,7)},
            { 49,Tuple.Create(4,5)},
            { 50,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 51,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 52,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 53,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 54,Tuple.Create(0,8)},
            { 55,Tuple.Create(1,6)},
            { 56,Tuple.Create(2,8)},
            { 57,Tuple.Create(3,8)},
            { 58,Tuple.Create(4,7)},
            { 59,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 60,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 61,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 62,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 63,Tuple.Create(0,9)},
            { 64,Tuple.Create(1,7)},
            { 65,Tuple.Create(2,9)},
            { 66,Tuple.Create(3,9)},
            { 67,Tuple.Create(4,9)},
            { 68,Tuple.Create(5,12)},
            { 69,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 70,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 71,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 72,Tuple.Create(0,10)},
            { 73,Tuple.Create(1,8)},
            { 74,Tuple.Create(2,10)},
            { 75,Tuple.Create(3,10)},
            { 76,Tuple.Create(4,10)},
            { 77,Tuple.Create(5,13)},
            { 78,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 79,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 80,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 81,Tuple.Create(0,12)},
            { 82,Tuple.Create(1,9)},
            { 83,Tuple.Create(2,11)},
            { 84,Tuple.Create(3,11)},
            { 85,Tuple.Create(4,11)},
            { 86,Tuple.Create(5,14)},
            { 87,Tuple.Create(1,14)},
            { 88,Tuple.Create(2,16)},
            { 89,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 90,Tuple.Create(0,13)},
            { 91,Tuple.Create(1,10)},
            { 92,Tuple.Create(2,12)},
            { 93,Tuple.Create(3,12)},
            { 94,Tuple.Create(4,12)},
            { 95,Tuple.Create(5,15)},
            { 96,Tuple.Create(1,16)},
            { 97,Tuple.Create(2,17)},
            { 98,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 99,Tuple.Create(0,14)},
            { 100,Tuple.Create(1,12)},
            { 101,Tuple.Create(2,13)},
            { 102,Tuple.Create(3,13)},
            { 103,Tuple.Create(4,13)},
            { 104,Tuple.Create(5,16)},
            { 105,Tuple.Create(1,17)},
            { 106,Tuple.Create(2,18)},
            { 107,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 108,Tuple.Create(0,15)},
            { 109,Tuple.Create(1,13)},
            { 110,Tuple.Create(2,14)},
            { 111,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 112,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 113,Tuple.Create(5,17)},
            { 114,Tuple.Create(0,17)},
            { 115,Tuple.Create(1,18)},
            { 116,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 117,Tuple.Create(0,16)},
            { 118,Tuple.Create(CONTINUE_ITEM,CONTINUE_ITEM)},
            { 119,Tuple.Create(2,15)},
            { 120,Tuple.Create(3,15)},
            { 121,Tuple.Create(4,14)},
            { 122,Tuple.Create(5,18)},
            { 123,Tuple.Create(0,18)},
            { 124,Tuple.Create(4,17)},
        };

        public MountainEverestKeeb(HardwareModel hardwareModel, MountainEverestNumericPadSide side, int keyboardXCount, string keyBoardName) : base(KEYBOARD_YAXIS_COUNTS, keyboardXCount, hardwareModel)
        {
            this._model.Layouts.Width = keyboardXCount;
            switch (side)
            {
                case MountainEverestNumericPadSide.Mid:
                    LAYOUT_MAPPING = MID_LAYOUT;
                    break;
                case MountainEverestNumericPadSide.Right:
                    LAYOUT_MAPPING = RIGHT_LAYOUT;
                    break;
                case MountainEverestNumericPadSide.Left:
                    LAYOUT_MAPPING = LEFT_LAYOUT;
                    break;
            }
        }

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = 0, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                Type = LightingDevices.MountainEverest,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Mountain Everest"
            };
        }

        protected override void TurnOffLed()
        {
            try
            {
                _displayColorBytes = new List<byte>();
                foreach (var key in LAYOUT_MAPPING)
                {
                    _displayColorBytes.Add(0);
                    _displayColorBytes.Add(0);
                    _displayColorBytes.Add(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MountainEverestKeeb/TurnOffLed is failed. EX:{ex}.");
            }
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            try
            {
                _displayColorBytes = new List<byte>();
                foreach (var dic in LAYOUT_MAPPING)
                {
                    if (dic.Value.Item1 == CONTINUE_ITEM)//stream buffer need to add posistion to send 
                    {
                        _displayColorBytes.Add(0xff);
                        _displayColorBytes.Add(0xff);
                        _displayColorBytes.Add(0xff);
                        continue;
                    }
                    ColorRGB getColor = colorMatrix[dic.Value.Item1, dic.Value.Item2];
                    _displayColorBytes.Add(getColor.R);
                    _displayColorBytes.Add(getColor.G);
                    _displayColorBytes.Add(getColor.B);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MountainEverestKeeb/ProcessColor is failed. EX:{ex}.");
            }
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            {
            };
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors) { }
    }
}
