using HidSharp;
using LightDancing.Enums;
using LightDancing.Hardware.Devices.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace LightDancing.Hardware.Devices
{
    internal class IBPMiniHubController : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0900";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        { return null; }

        public List<USBDeviceBase> InitDevices(int port1Fans, int port1Leds, int port2Fans, int port2Leds, int port1FanSpeed, int port2FanSpeed)
        {
            SerialDetector serialDetector = new SerialDetector();
            List<Tuple<SerialStream, string>> streams = serialDetector.GetSerialStreamsAndSerialID(DEVICE_PID);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (Tuple<SerialStream, string> stream in streams)
                {
                    stream.Item1.BaudRate = BAUDRATES;
                    IBPMiniHubDevice device = new IBPMiniHubDevice(stream.Item1, stream.Item2, port1Fans, port1Leds, port2Fans, port2Leds, port1FanSpeed, port2FanSpeed);
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

    public class IBPMiniHubDevice : USBDeviceBase
    {
        public Action LayoutChanged { get; set; }
        public int _port1Fans, _port1Leds, _port2Fans, _port2Leds;

        private List<List<LightingBase>> _serialDevices;

        private bool _isLedUIControlled = false;
        private bool _isFanUIControlled = false;

        private bool _updating = false;

        public bool UpdateRequested { get; set; }

        private Tuple<bool, string> result;

        private int _rearFanSpeed = 20;
        private int _frontFanSpeed = 20;

        public IBPMiniHubDevice(SerialStream deviceStream, string serialID, int port1Fans, int port1Leds, int port2Fans, int port2Leds, int port1FanSpeed, int port2FanSpeed) : base(deviceStream, serialID, MiniHubLayout.Gen7Plus)
        {
            _serialDevices = new List<List<LightingBase>>() { new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>(), new List<LightingBase>() };
            SetFanSpeed(port1FanSpeed, port2FanSpeed); /*Set default fan speed to 20%*/
            _port1Fans = port1Fans;
            _port1Leds = port1Leds;
            _port2Fans = port2Fans;
            _port2Leds = port2Leds;
            _lightingBase = InitDevice();
        }

        /// <summary>
        /// Init devices
        /// </summary>
        /// <returns></returns>
        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            switch (_port1Fans)
            {
                case 1:
                    MiniHubPort1Fan port1Fan = new MiniHubPort1Fan(0, _model);
                    _serialDevices[0].Add(port1Fan);
                    hardwares.Add(port1Fan);
                    break;
                default:
                    break;
            }

            switch (_port2Fans)
            {
                case 1:
                    MiniHubPort2Fan frontFans = new MiniHubPort2Fan(1, _model, 5);
                    _serialDevices[1].Add(frontFans);
                    hardwares.Add(frontFans);
                    break;
                case 2:
                    frontFans = new MiniHubPort2Fan(1, _model, 10);
                    _serialDevices[1].Add(frontFans);
                    hardwares.Add(frontFans);
                    break;
                case 3:
                    frontFans = new MiniHubPort2Fan(1, _model, 15);
                    _serialDevices[1].Add(frontFans);
                    hardwares.Add(frontFans);
                    break;
                default:
                    break;
            }

            if (_port1Leds > 0)
            {
                MiniHubLedStrip leds = new MiniHubLedStrip(2, _model, _port1Leds);
                _serialDevices[2].Add(leds);
                hardwares.Add(leds);
            }

            if (_port2Leds > 0)
            {
                MiniHubLedStrip leds = new MiniHubLedStrip(3, _model, _port2Leds);
                _serialDevices[3].Add(leds);
                hardwares.Add(leds);
            }

            return hardwares;
        }

        /// <summary>
        /// Change hub layout by setting fan count and led count
        /// </summary>
        /// <param name="port1Fans">port1 fan count</param>
        /// <param name="port2Fans">port2 fan count</param>
        /// <param name="port1Leds">port1(port3) led count</param>
        /// <param name="port2Leds">port2(port4) led count</param>
        public void ChangeHubLayout(int port1Fans, int port2Fans, int port1Leds, int port2Leds)
        {
            _port1Fans = port1Fans;
            _port1Leds = port1Leds;
            _port2Fans = port2Fans;
            _port2Leds = port2Leds;
            LayoutChanged?.Invoke();
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (!_isLedUIControlled)
            {
                _isLedUIControlled = true;
                SetFanRGBMode(_isLedUIControlled);
            }
            for (int devicePort = 0; devicePort < _serialDevices.Count; devicePort++)
            {
                List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x03, (byte)(devicePort + 1), 0x01, 0x68, 0x00 };
                for (int device = 0; device < _serialDevices[devicePort].Count; device++)
                {
                    if (_serialDevices[devicePort].Count > 0)
                    {
                        if (process)
                            _serialDevices[devicePort][device].ProcessStreaming(false, brightness);
                        var colors = _serialDevices[devicePort][device].GetDisplayColors();
                        collectBytes.AddRange(colors);
                    }
                }

                if (devicePort == 3)
                {
                    while (collectBytes.Count < 307)
                    {
                        collectBytes.Add(0x00);
                    }
                }
                if (devicePort == 2)
                {
                    while (collectBytes.Count < 157)
                    {
                        collectBytes.Add(0x00);
                    }
                }
                if (devicePort == 1)
                {
                    while (collectBytes.Count < 157)
                    {
                        collectBytes.Add(0x00);
                    }
                }

                try
                {
                    _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
                }
                catch { }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _serialID;
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = deviceID,
                USBDeviceType = USBDevices.IBPMiniHub,
                Name = "IBP Mini Hub",
            };
        }

        /*No FW Version for CES*/
        private string GetFirmwareVersion()
        {
            byte[] commands = new byte[] { 0xff, 0xdd, 0x02 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { }

            Thread.Sleep(20);
            byte[] command = new byte[7];
            try
            {
                _deviceStream.Read(command);
            }
            catch { }
            return string.Format("{0}.{1}.{2}.{3}", command[3], command[4], command[5], command[6]);
        }

        public void SetFanRGBMode(bool _isUIControlled)
        {
            _isLedUIControlled = _isUIControlled;
            if (_isUIControlled)
            {
                byte[] commands = new byte[] { 0xff, 0xdd, 0x03, 0x00 };
                _deviceStream.Write(commands);
            }
            else
            {
                byte[] commands = new byte[] { 0xff, 0xdd, 0x03, 0x01 };
                _deviceStream.Write(commands);
            }
        }

        public void SetFanSpeedMode(bool _isUIControlled)
        {
            _isFanUIControlled = _isUIControlled;
            if (_isUIControlled)
            {
                byte[] commands = new byte[] { 0xff, 0xdd, 0x05, 0x00 };
                _deviceStream.Write(commands);
            }
            else
            {
                byte[] commands = new byte[] { 0xff, 0xdd, 0x05, 0x01 };
                _deviceStream.Write(commands);
            }
        }

        /// <summary>
        /// Set Fan Speed
        /// </summary>
        /// <param name="rearFanSpeed">Range from 0 to 9, 0 is the slowest, 9 is the fastest</param>
        /// <param name="frontFansSpeed">Range from 0 to 9</param>
        public void SetFanSpeed(int? rearFanSpeed, int? frontFansSpeed)
        {
            if (!_isFanUIControlled)
            {
                _isFanUIControlled = true;
                SetFanSpeedMode(_isFanUIControlled);
            }
            if (rearFanSpeed != null)
            {
                _rearFanSpeed = (int)rearFanSpeed < 10 ? 10 : (int)rearFanSpeed;
                _rearFanSpeed = _rearFanSpeed > 100 ? 100 : (int)_rearFanSpeed;
            }

            if (frontFansSpeed != null)
            {
                _frontFanSpeed = (int)frontFansSpeed < 10 ? 10 : (int)frontFansSpeed;
                _frontFanSpeed = _frontFanSpeed > 100 ? 100 : (int)_frontFanSpeed;
            }

            byte[] commands = new byte[] { 0xff, 0xdd, 0x04, 0x00, (byte)_rearFanSpeed, 0x00, (byte)_frontFanSpeed };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { }
        }

        public Tuple<int, int> GetFanRPM()
        {
            byte[] commands = new byte[] { 0xff, 0xdd, 0x06 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { }
            Thread.Sleep(20);
            byte[] feedback = new byte[9];
            try
            {
                _deviceStream.Read(feedback);
                double time_1 = (double)feedback[5] / 10;
                double time_2 = (double)feedback[8] / 10;

                int rpm_1 = (int)((60 * 1000) / (time_1 * 4));
                int rpm_2 = (int)((60 * 1000) / (time_2 * 4));

                return Tuple.Create(rpm_1, rpm_2);
            }
            catch { }
            return Tuple.Create(0, 0);
        }

        public override void TurnFwAnimationOn()
        {
            SetFanRGBMode(false);
            SetFanSpeedMode(false);
        }

        /// <summary>
        /// 1. Check if there is Hyte directory in the document folder, if no then create one
        /// 2. Compare device fw and cloud fw version
        /// 3. If update is avaliable, download fw file
        /// </summary>
        /// <returns>true and fw version, false and failure reason</returns>
        public override Tuple<bool, string> GetUpdateAbility()
        {
            if (_updating)
            {
                return Tuple.Create(false, "Updating");
            }

            string DocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string HyteDocPath = DocPath + "\\Hyte";
            if (!Directory.Exists(HyteDocPath))
            {
                try
                {
                    Directory.CreateDirectory(HyteDocPath);
                    if (!Directory.Exists(HyteDocPath))
                    {
                        throw new Exception("Cannot Create Hyte folder in Documents");
                    }
                }
                catch
                {
                    return Tuple.Create(false, "Can not Create Directory in Documents");
                }
            }

            try
            {
                WebRequest request = WebRequest.Create("https://hyte.s3.eu-west-2.amazonaws.com/fw/minihub/latest.json");
                WebResponse response = request.GetResponse();
                Stream datastream = response.GetResponseStream();
                StreamReader reader = new StreamReader(datastream);
                string htmlStr = reader.ReadToEnd();
                string Path = htmlStr.Split('"')[3];
                reader.Close();
                datastream.Close();
                response.Close();
                string version = Path.Split(@"/").Last().Split(".hex").First().Split("MiniHub_").Last();
                var model = GetModel();

                if (model.FirmwareVersion == version)
                {
                    string line = "Firmware" + version + " is up to date";
                    return Tuple.Create(false, line);
                }

                if (!HardwaresDetector.Instance.STM32_Setting)
                {
                    return Tuple.Create(false, "Didn't setup STM32 folder path");
                }

                string updateFile = HyteDocPath + "\\" + Path.Split(@"/").Last();

                if (!File.Exists(updateFile))
                {
                    WebClient mywebClient = new WebClient();
                    mywebClient.DownloadFile(Path, updateFile);
                }

                if (!File.Exists(updateFile))
                {
                    throw new Exception("Update firmware file download failed");
                }

                STM32Directory.SetUpdateBinPath(Tuple.Create(USBDevices.IBPMiniHub, updateFile));

                return Tuple.Create(true, version);
            }
            catch
            {
                return Tuple.Create(false, "Update firmware file download failed");
            }
        }

        /// <summary>
        /// Update firmware version, will add device to the updateList
        /// </summary>
        /// <returns>True and update success, false and failure reason</returns>
        public override Tuple<bool, string> Update()
        {
            if (!File.Exists(STM32Directory.UPDATE_BIN_PATH[USBDevices.IBPMiniHub]))
            {
                return Tuple.Create(false, "Update file not exits");
            }
            if (!HardwaresDetector.Instance.STM32_Setting)
            {
                return Tuple.Create(false, "Didn't setup STM32 folder path");
            }

            try
            {
                _updating = true;

                byte[] pidvid = new byte[] { 0xff, 0xdc, 0x06, 0x09, 0x00, 0x00, 0xdd };
                _deviceStream.Write(pidvid);

                byte[] commands = new byte[] { 0xff, 0xaa, 0x09, 0x08, 0x07, 0x06, 0x05 };
                _deviceStream.Write(commands);
                HardwaresDetector.Instance.AddUpdateDevice(this);
                UpdateRequested = true;

                Thread.Sleep(30);

                byte[] read = new byte[7];
                _deviceStream.Read(read);

                if (read[2] == 0x05 && read[3] == 0x06 && read[4] == 0x07 && read[5] == 0x08 && read[6] == 0x09)
                {
                    /*hang here until get update response*/
                    while (UpdateRequested)
                    {
                        Thread.Sleep(10);
                    }

                    if (result != null)
                    {
                        HardwaresDetector.Instance.RemoveUpdateDevice(GetModel().DeviceID, result.Item1);
                        _updating = false;
                    }
                    return result;
                }
                else
                {
                    UpdateRequested = false;
                    _updating = false;
                    HardwaresDetector.Instance.RemoveUpdateDevice(GetModel().DeviceID, false);
                    return Tuple.Create(false, "Stm32 Receive Not Good");
                }
            }
            catch { }
            UpdateRequested = false;
            _updating = false;
            HardwaresDetector.Instance.RemoveUpdateDevice(GetModel().DeviceID, false);
            return Tuple.Create(false, "Stm32 Receive Not Good");
        }

        public void ChangeUpdateStatus(Tuple<bool, string> status)
        {
            result = status;
        }
    }
}
