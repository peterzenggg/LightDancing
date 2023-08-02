using HidSharp;
using LightDancing.Enums;
using LightDancing.Hardware.Devices.Components;
using LightDancing.Hardware.Devices.Fans;
using LightDancing.Hardware.Devices.SmartComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LightDancing.Hardware.Devices
{
    internal class NP50Controller : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0901";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<Tuple<SerialStream, string>> streams = serialDetector.GetSerialStreamsAndSerialID(DEVICE_PID);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (Tuple<SerialStream, string> stream in streams)
                {
                    stream.Item1.BaudRate = BAUDRATES;
                    NP50Device device = new NP50Device(stream.Item1, stream.Item2);
                    hardwares.Add(device);
                }
                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class NP50Device : USBDeviceBase
    {
        private readonly List<List<SmartDevice>> _serialDevices = new List<List<SmartDevice>>() { new List<SmartDevice>(), new List<SmartDevice>(), new List<SmartDevice>(), new List<SmartDevice>() };
        private List<LightingBase> _hardwares = new List<LightingBase>();
        private List<FanGroup> _fanDevices = new List<FanGroup>();
        private List<List<LightingBase>> _lightingBases = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };
        private const int CHANNEL_MAX_DEVICE_COUNT = 3;
        private bool _isGettingDeviceData = false;
        private readonly FanGroup _thisNP50 = new FanGroup();
        private readonly object _lock = new object();
        private const int CHANNEL_COMMAND_INDEX = 12;
        private bool _firstInit = true;

        public NP50Device(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            NP50 thisNp50 = new NP50(this);
            _thisNP50.DeviceBases.Add(thisNp50);
            _fanDevices = new List<FanGroup> { _thisNP50 };
            CountChannelDevices();

            CheckNP50BoardInfo();
            StartDetectingDeviceHotswap();

            return _hardwares;
        }

        private void CountChannelDevices()
        {
            bool deviceChanged = false;
            for (int channel = 1; channel <= 3; channel++)
            {
                deviceChanged = deviceChanged || CheckChannelInfo(channel);
            }

            if (deviceChanged || _firstInit)
            {
                /*Get Device To List*/
                _lightingBases = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };
                _hardwares = new List<LightingBase>();
                _fanDevices = new List<FanGroup> { _thisNP50 };

                HR60Logo hr60logo = new HR60Logo("1_1", _model);
                _hardwares.Add(hr60logo);
                _lightingBases[0].Add(hr60logo);

                for (int channel = 1; channel <= 3; channel++)
                {
                    int count = 0;
                    List<SmartDevice> smartDevices = new List<SmartDevice>();
                    for (int i = 0; i < _serialDevices[channel - 1].Count; i++)
                    {
                        count++;
                        smartDevices.Add(_serialDevices[channel - 1][i]);

                        if (!_serialDevices[channel - 1][i].IsConnected || i == _serialDevices[channel - 1].Count - 1)
                        {
                            if (smartDevices[0].GetType() == typeof(PQ10) || smartDevices[0].GetType() == typeof(PQ30))
                            {
                                LedGroup ledGroup = new LedGroup();
                                List<int> ledCounts = new List<int>();
                                foreach (var ledStip in smartDevices)
                                {
                                    ledGroup.DeviceBases.Add(ledStip);
                                    ledCounts.Add(((LedStripBase)ledStip).LedCount);
                                }

                                /*Create lightingbase*/
                                int ledCount = ledGroup.GetLedCount();
                                LedStrip ledStrip = new LedStrip(channel + "_" + ledCount + count, _model, ledCounts);
                                _hardwares.Add(ledStrip);
                                _lightingBases[channel - 1].Add(ledStrip);
                                smartDevices.Clear();
                            }
                            else if (smartDevices[0].GetType() == typeof(FT12))
                            {
                                FanGroup fanGroup = new FanGroup();
                                foreach (var fan in smartDevices)
                                {
                                    fanGroup.DeviceBases.Add(fan);
                                }
                                _fanDevices.Add(fanGroup);
                                smartDevices.Clear();
                            }
                        }
                    }
                }

                _lightingBase = _hardwares;
                if (!_firstInit)
                {
                    HardwaresDetector.Instance.SmartDeviceHotswap();
                }
                _firstInit = false;
            }
        }

        public List<FanGroup> GetFanGroups()
        {
            Debug.WriteLine("_fanDevices count " + _fanDevices.Count);
            return _fanDevices;
        }

        private void StartDetectingDeviceHotswap()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    CountChannelDevices();
                }
            });
        }

        /// <summary>
        /// Start getting device data from NP50
        /// </summary>
        public void StartGettingDeviceData()
        {
            if (!_isGettingDeviceData)
            {
                _isGettingDeviceData = true;
            }
        }

        private void GetSmartDeviceData(int channel, int index, byte[] data)
        {
            if (_serialDevices[channel - 1][index].GetType() == typeof(FT12))
            {
                ((FT12)_serialDevices[channel - 1][index]).Temperature = (data[index * CHANNEL_COMMAND_INDEX + 6] + data[index * CHANNEL_COMMAND_INDEX + 7] / (double)100);
                ((FT12)_serialDevices[channel - 1][index]).CurrentRPM = GetFanRPM(data[index * CHANNEL_COMMAND_INDEX + 8], data[index * CHANNEL_COMMAND_INDEX + 9]);
                ((FT12)_serialDevices[channel - 1][index]).FanDirection = (FanOrientation)data[index * CHANNEL_COMMAND_INDEX + 10];
            }
        }

        private double GetTempFromBytes(double data)
        {
            double rt = (data * 1800) / (3.3 - data);
            double rp = 10000;
            double t2 = 273.15 + 25;
            float bx = 3950;
            double ka = 273.15;
            var num = 1 / (1 / t2 + Math.Log(rt / rp) / bx) - ka + 0.5;
            return num;
        }

        public void StopGettingDeviceData()
        {
            _isGettingDeviceData = false;
        }

        private bool _isFirstRead = true;
        private void CheckNP50BoardInfo()
        {
            if (_isFirstRead)
            {
                byte[] commandsRead = new byte[13] { 0xFF, 0xCC, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0xB8, 0x00, 0x00, 0x00 };
                try
                {
                    _deviceStream.Write(commandsRead);
                }
                catch
                {
                    Console.WriteLine("Fail to check NP50 info.");
                }

                _isFirstRead = false;
            }

            byte[] commands = new byte[4] { 0xFF, 0xCC, 0x01, 0x00 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { Console.WriteLine("Fail to check NP50 info."); }
            Thread.Sleep(100);
            byte[] data = new byte[13];
            try
            {
                _deviceStream.Read(data);
            }
            catch { Console.WriteLine("Fail to read NP50 info."); }

            if (data[0] == 0xFF && data[1] == 0xCC)
            {
                //read noise
                double noise = GetTempFromBytes(data[3] / (double)10 + data[4] / (double)1000);
                //read temp
                double internalTemp = (data[5] + data[6] / (double)10);
                //read pump temp
                double pumpTemp = (data[7] + data[8] / (double)10);
                //read pump rpm
                int pumpRpm = GetRPM(data[9], data[10]);

                ((NP50)_thisNP50.DeviceBases[0]).Noise = noise;
                ((NP50)_thisNP50.DeviceBases[0]).Temperature = pumpTemp;
            }
            else
            {
                Console.WriteLine("Fail to read info from NP50");
            }
        }

        /// <summary>
        /// Check if devices are different
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private bool CheckChannelInfo(int channel)
        {
            byte[] data = SendGetInfoCommands(channel);
            bool devicesChanged = false;

            if (data[0] == 0xFE && data[1] == 0xCC) /*If channel has any device*/
            {
                int fanCount = 0;
                //GetDeviceType
                for (int i = 0; i < CHANNEL_MAX_DEVICE_COUNT; i++)
                {
                    if (data[i * CHANNEL_COMMAND_INDEX + 3] != 0x00)
                    {
                        SmartComponent thisComponent = data[i * CHANNEL_COMMAND_INDEX + 3] switch
                        {
                            1 => SmartComponent.PQ10,
                            2 => SmartComponent.PQ30,
                            3 => SmartComponent.FT12,
                        };

                        if (data[i * CHANNEL_COMMAND_INDEX + 3] == 3)
                        {
                            fanCount++;
                        }

                        /*_serialDevices length is shorter*/
                        if (_serialDevices[channel - 1].Count > i)
                        {
                            if ((thisComponent == SmartComponent.PQ10 && _serialDevices[channel - 1][i].GetType() != typeof(PQ10)) || (thisComponent == SmartComponent.FT12 && _serialDevices[channel - 1][i].GetType() != typeof(FT12)) || (thisComponent == SmartComponent.PQ30 && _serialDevices[channel - 1][i].GetType() != typeof(PQ30)))
                            {
                                SmartDevice device = CreateNewDeviceObject(data[i * CHANNEL_COMMAND_INDEX + 3], data[i * CHANNEL_COMMAND_INDEX + 11], channel, fanCount);
                                _serialDevices[channel - 1][i] = device;
                                devicesChanged = true;
                            }
                        }
                        else
                        {
                            SmartDevice device = CreateNewDeviceObject(data[i * CHANNEL_COMMAND_INDEX + 3], data[i * CHANNEL_COMMAND_INDEX + 11], channel, fanCount);
                            _serialDevices[channel - 1].Add(device);
                            devicesChanged = true;
                        }

                        //Get device data
                        if (_isGettingDeviceData)
                        {
                            GetSmartDeviceData(channel, i, data);
                        }
                    }
                    else
                    {
                        if (_serialDevices[channel - 1].Count > i)
                        {
                            _serialDevices[channel - 1].RemoveAt(i);
                            devicesChanged = true;
                        }
                    }
                }
            }
            else
            {
                if (_serialDevices[channel - 1].Count > 0)
                {
                    _serialDevices[channel - 1].Clear();
                    devicesChanged = true;
                }
            }

            if (_isGettingDeviceData)
            {
                CheckNP50BoardInfo();
            }

            return devicesChanged;
        }

        private SmartDevice CreateNewDeviceObject(byte deviceType, byte isConnected, int channel, int fanCount)
        {
            SmartDevice device = deviceType switch
            {
                1 => new PQ10(),
                2 => new PQ30(),
                3 => new FT12(this, "NP50 Channel" + channel + "Fan" + fanCount),
                _ => null,
            };
            device.IsConnected = isConnected == 0x00;

            return device;
        }

        private byte[] SendGetInfoCommands(int channel)
        {
            byte[] commands = new byte[4] { 0xFF, 0xCC, 0x01, (byte)channel };
            try
            {
                lock (_lock)
                {
                    _deviceStream.Write(commands);
                }
            }
            catch { Console.WriteLine("Fail to check NP50 channel " + channel + " info."); }
            Thread.Sleep(100);
            byte[] data = new byte[240]; /*For 20 devices*/
            try
            {
                _deviceStream.Read(data);
            }
            catch { Console.WriteLine("Fail to read NP50 channel " + channel + " info."); }

            return data;
        }

        private int GetRPM(byte speedH, byte speedL)
        {
            return (int)(60 * 1000 / (speedH * 10 + (float)speedL / 10)) / 4;
        }

        private int GetFanRPM(byte speedH, byte speedL)
        {
            return (int)(60 * 1000 / (speedH + (float)speedL / 100)) / 4;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            for (int devicePort = 0; devicePort < _lightingBases.Count; devicePort++)
            {
                List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x01, (byte)(devicePort + 1), 0x01, 0x68, 0x00 };
                for (int device = 0; device < _lightingBases[devicePort].Count; device++)
                {
                    if (process)
                        _lightingBases[devicePort][device].ProcessStreaming(false, brightness);

                    if (_lightingBases[devicePort][device].GetType() != null)
                    {
                        var colors = _lightingBases[devicePort][device].GetDisplayColors();
                        if (colors != null)
                        {
                            collectBytes.AddRange(colors);
                        }
                    }
                }

                if (devicePort == 3)
                {
                    for (int i = collectBytes.Count; i < 90; i++)
                    {
                        collectBytes.Add(0x00);
                    }
                }
                lock (_lock)
                {
                    _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _serialID;
            return new HardwareModel()
            {
                FirmwareVersion = "N/A",
                DeviceID = deviceID,
                USBDeviceType = USBDevices.NP50,
                Name = "NP50"
            };
        }

        /// <summary>
        /// For FT12 device to call 
        /// </summary>
        public void SetFanSpeed()
        {
            byte[] commands = new byte[31];
            commands[0] = 0xFF;
            commands[1] = 0xCC;
            commands[2] = 0x02;

            for (int channel = 1; channel <= 2; channel++)
            {
                commands[3] = (byte)channel;
                for (int i = 0; i < _serialDevices[channel - 1].Count; i++)
                {
                    if (_serialDevices[channel - 1][i].GetType() == typeof(FT12))
                    {
                        commands[i * 9 + 4] = (byte)i;
                        commands[i * 9 + 7] = (byte)((FT12)_serialDevices[channel - 1][i]).controlMode;
                        if (((FT12)_serialDevices[channel - 1][i]).controlMode == ControlMode.Percentage)
                        {
                            commands[i * 9 + 6] = (byte)((FT12)_serialDevices[channel - 1][i]).SpeedPercentage;
                        }
                        else
                        {
                            commands[i * 9 + 8] = (byte)((FT12)_serialDevices[channel - 1][i]).TargetRPM;
                        }
                    }
                }

                try
                {
                    lock (_lock)
                    {
                        Thread.Sleep(100);
                        _deviceStream.Write(commands);
                    }
                }
                catch
                {
                    Console.WriteLine("Fail to set fan speed of NP50 channel " + channel);
                }
            }
        }

        /// <summary>
        /// For pump to call
        /// </summary>
        public void SetPumpSpeed()
        {
            byte[] commands = new byte[31];
            commands[0] = 0xFF;
            commands[1] = 0xCC;
            commands[2] = 0x02;
            commands[3] = 0x00;
            commands[4] = 0x00; //on
            commands[5] = (byte)((NP50)_fanDevices.FirstOrDefault().DeviceBases[0]).SpeedPercentage;

            try
            {
                Thread.Sleep(100);
                _deviceStream.Write(commands);
            }
            catch
            {
                Console.WriteLine("Fail to set NP50 pump speed");
            }
        }

        public void SetMBBypass()
        {
            byte[] commands = new byte[31];
            commands[0] = 0xFF;
            commands[1] = 0xCC;
            commands[2] = 0x02;
            commands[3] = 0x00;
            commands[4] = 0x01; //On

            try
            {
                Thread.Sleep(100);
                _deviceStream.Write(commands);
                _isFirstRead = true;
            }
            catch
            {
                Console.WriteLine("Fail to set NP50 to MB mode");
            }
        }

        public override void TurnFwAnimationOn()
        {
            SetMBBypass();
        }
    }
}
