using System;
using System.Collections.Generic;
using HidSharp;
using LightDancing.Common;
using LightDancing.Colors;
using LightDancing.Enums;
using System.Threading;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace LightDancing.Hardware.Devices
{
    internal class CNVSLeftController : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0B00"; //2023 March MP CNVS (Hardware version 1)
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<Tuple<SerialStream, string>> streams = serialDetector.GetSerialStreamsAndSerialID(DEVICE_PID);

            List<USBDeviceBase> hardwares = null;

            if (streams != null && streams.Count > 0)
            {
                if (hardwares == null)
                {
                    hardwares = new List<USBDeviceBase>();
                }

                foreach (Tuple<SerialStream, string> stream in streams)
                {
                    stream.Item1.BaudRate = BAUDRATES;
                    CNVSLeftDevice device = new CNVSLeftDevice(stream.Item1, stream.Item2);
                    hardwares.Add(device);
                }
            }

            return hardwares;
        }
    }

    public class CNVSLeftDevice : USBDeviceBase
    {
        public bool UpdateRequested { get; set; }

        private bool _isFwAnimationON = true;

        private bool _updating = false;

        private Tuple<bool, string> result;

        private bool _turnOffStartupAnimation, _playAnimationWhenPCOff;

        private bool _isOff = false;

        public CNVSLeftDevice(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
            GetCnvsSettingFromFW();
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CNVSLeft cnvs = new CNVSLeft(_model);
            hardwares.Add(cnvs);
            cnvs._LedTurnOff += SendCommand;

            return hardwares;
        }

        private void SendCommand()
        {
            SendToHardware(false, 0f);
            _isOff = true;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_isFwAnimationON)
            {
                TurnFwAnimationOFF();
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"CNVS device count error");
            }
            else
            {
                if (_isOff == _lightingBase[0].IsLedOn())
                {
                    _isOff = !_lightingBase[0].IsLedOn();
                }

                if (process)
                {
                    _lightingBase[0].ProcessStreaming(false, brightness);
                }
                byte[] commands = _lightingBase[0].GetDisplayColors().ToArray();
                try
                {
                    _deviceStream.Write(commands);
                }
                catch
                {
                    Trace.WriteLine($"False to streaming on CNVS");
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = _serialID,
                USBDeviceType = USBDevices.CNVSLeft,
                Name = "HYTE qRGB CNVS"
            };
        }

        /// <summary>
        /// 2023/02/16 Protocol
        /// FF DD 02 __ __ __ __ => fw version large, mid, minor, hardware version
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Change CNVS startup animation and pc off behavior setting
        /// </summary>
        /// <param name="turnOffStartupAnimation">true: turn off start up animation</param>
        /// <param name="playAnimationWhenPCOff">true: keep playing animation when pc is off</param>
        public void ChangeCnvsSetting(bool? turnOffStartupAnimation, bool? playAnimationWhenPCOff)
        {
            if (turnOffStartupAnimation != null)
            {
                _turnOffStartupAnimation = (bool)turnOffStartupAnimation;
            }

            if (playAnimationWhenPCOff != null)
            {
                _playAnimationWhenPCOff = (bool)playAnimationWhenPCOff;
            }

            byte playStartup = (byte)(_turnOffStartupAnimation ? 0x01 : 0x00);
            byte turnOffAnimation = (byte)(_playAnimationWhenPCOff ? 0x01 : 0x00);

            byte[] commands = new byte[] { 0xff, 0xdc, 0x07, playStartup, turnOffAnimation };
            try
            {
                _deviceStream.Write(commands);
            }
            catch
            {
                Trace.WriteLine($"False to set startup animation settings on CNVS");
            }
        }

        /// <summary>
        /// Get CNVS setting of "play startup animation" and "turn off when pc is off" parameter from firmware  
        /// Turn off startup animation = true (0x01), false (0x00)
        /// Play background when PC is off = true (0x01), false (0x00)
        /// </summary>
        private void GetCnvsSettingFromFW()
        {
            byte[] commands = new byte[] { 0xff, 0xdc, 0x08 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch { }
            Thread.Sleep(20);
            byte[] command = new byte[9];
            try
            {
                _deviceStream.Read(command);
                _turnOffStartupAnimation = command[3] == 0x01;
                _playAnimationWhenPCOff = command[4] == 0x01;
            }
            catch { }
        }

        /// <summary>
        /// For UI to get cnvs settings
        /// Default should be false(0x00) and false(0x00) 
        /// </summary>
        /// <returns>Tuple of turnOffStartupAnimation and playAnimationWhenPCOff</returns>
        public Tuple<bool, bool> GetCnvsSetting()
        {
            return Tuple.Create(_turnOffStartupAnimation, _playAnimationWhenPCOff);
        }

        public void ChangeUpdateStatus(Tuple<bool, string> status)
        {
            result = status;
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

            /*Check if document folder contains Hyte folder*/
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
                WebRequest request = WebRequest.Create("https://hyte.s3.eu-west-2.amazonaws.com/fw/cnvs/latest.json");
                WebResponse response = request.GetResponse();
                Stream datastream = response.GetResponseStream();
                StreamReader reader = new StreamReader(datastream);
                string htmlStr = reader.ReadToEnd();
                string Path = htmlStr.Split('"')[3];
                reader.Close();
                datastream.Close();
                response.Close();
                string version = Path.Split(@"/").Last().Split(".hex").First().Split("CNVS_").Last();
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

                STM32Directory.SetUpdateBinPath(Tuple.Create(USBDevices.CNVSLeft, updateFile));

                return Tuple.Create(true, version);
            }
            catch
            {
                return Tuple.Create(false, "Update firmware file download failed");
            }
        }

        /// <summary>
        /// Update firmware version, will add device to the updateList
        /// 1. ff dc 06 0b 00 00 dd, to write 0b0000dd to eeprom, if didn't write dd, will not go to dfu mode
        /// 2. ff aa 09 08 07 06 05, to go to dfu mode
        /// 3. Add deivce in updateDeviceList (since it will be disapear when hotswap
        /// 4. Hang in while until update process done
        /// 5. Remove device from UpdateList 
        /// </summary>
        /// <returns>True and update success, false and failure reason</returns>
        public override Tuple<bool, string> Update()
        {
            if (!File.Exists(STM32Directory.UPDATE_BIN_PATH[USBDevices.CNVSLeft]))
            {
                return Tuple.Create(false, "Update file not exits");
            }
            if (!HardwaresDetector.Instance.STM32_Setting)
            {
                return Tuple.Create(false, "Didn't setup STM32 folder path");
            }

            /*Wait 15s to make sure that startup animation is off*/
            Thread.Sleep(15000);
            /*Turn off deivce before OTA mode*/
            _lightingBase[0].SetLedTurnOn(false);

            while (!_isOff)
            {
                Thread.Sleep(100);
            }

            try
            {
                _updating = true;
                HardwaresDetector.Instance.AddUpdateDevice(this);
                byte[] pidvid = new byte[] { 0xff, 0xdc, 0x06, 0x0b, 0x00, 0x00, 0xdd };
                _deviceStream.Write(pidvid);

                byte[] commands = new byte[] { 0xff, 0xaa, 0x09, 0x08, 0x07, 0x06, 0x05 };
                _deviceStream.Write(commands);
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

        public override void TurnFwAnimationOn()
        {
            List<byte> offByte = new List<byte>() { 0xff, 0xdc, 0x02 };
            try
            {
                _deviceStream.Write(offByte.ToArray(), 0, offByte.Count);
            }
            catch { }

            List<byte> collectBytes = new List<byte>() { 0xff, 0xdc, 0x05, 0x01 };
            try
            {
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
            }
            catch { }
            _isFwAnimationON = true;
        }

        private void TurnFwAnimationOFF()
        {
            List<byte> collectBytes = new List<byte>() { 0xff, 0xdc, 0x05, 0x00 };
            try
            {
                _deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
            }
            catch { }
            _isFwAnimationON = false;
        }
    }

    /// <summary>
    /// 2022/11/14 Change to 50 leds layout
    /// Control box right hand side version
    /// </summary>
    public class CNVSLeft : LightingBase
    {
        /// <summary>
        /// The keyboard of Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 7;

        /// <summary>
        /// The keyboard of X-Axis count
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 22;

        public CNVSLeft(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                USBDeviceType = _usbModel.USBDeviceType,
                Type = LightingDevices.CNVS,
                Name = "HYTE qRGB CNVS"
            };
        }

        /// <summary>
        /// key = keyboard's key, Value = 2D position on Keyboard (ex: ESC = (0, 0)), item1 = y position, item2 = x position
        /// </summary>
        private readonly Dictionary<Keyboard, Tuple<int, int>> ColorToLayout = new Dictionary<Keyboard, Tuple<int, int>>()
        {
            { Keyboard.TopLED1, Tuple.Create(0, 1)},
            { Keyboard.TopLED2, Tuple.Create(0, 2)},
            { Keyboard.TopLED3, Tuple.Create(0, 3)},
            { Keyboard.TopLED4, Tuple.Create(0, 4)},
            { Keyboard.TopLED5, Tuple.Create(0, 5)},
            { Keyboard.TopLED6, Tuple.Create(0, 6)},
            { Keyboard.TopLED7, Tuple.Create(0, 7)},
            { Keyboard.TopLED8, Tuple.Create(0, 8)},
            { Keyboard.TopLED9, Tuple.Create(0, 9)},
            { Keyboard.TopLED10, Tuple.Create(0, 10)},
            { Keyboard.TopLED11, Tuple.Create(0, 11)},
            { Keyboard.TopLED12, Tuple.Create(0, 12)},
            { Keyboard.TopLED13, Tuple.Create(0, 13)},
            { Keyboard.TopLED14, Tuple.Create(0, 14)},
            { Keyboard.TopLED15, Tuple.Create(0, 15)},
            { Keyboard.TopLED16, Tuple.Create(0, 16)},
            { Keyboard.TopLED17, Tuple.Create(0, 17)},
            { Keyboard.TopLED18, Tuple.Create(0, 18)},
            { Keyboard.TopLED19, Tuple.Create(0, 19)},
            { Keyboard.TopLED20, Tuple.Create(0, 20)},

            { Keyboard.RightLED1, Tuple.Create(1, 21)},
            { Keyboard.RightLED2, Tuple.Create(2, 21)},
            { Keyboard.RightLED3, Tuple.Create(3, 21)},
            { Keyboard.RightLED4, Tuple.Create(4, 21)},
            { Keyboard.RightLED5, Tuple.Create(5, 21)},

            { Keyboard.BottomLED20, Tuple.Create(6, 20)},
            { Keyboard.BottomLED19, Tuple.Create(6, 19)},
            { Keyboard.BottomLED18, Tuple.Create(6, 18)},
            { Keyboard.BottomLED17, Tuple.Create(6, 17)},
            { Keyboard.BottomLED16, Tuple.Create(6, 16)},
            { Keyboard.BottomLED15, Tuple.Create(6, 15)},
            { Keyboard.BottomLED14, Tuple.Create(6, 14)},
            { Keyboard.BottomLED13, Tuple.Create(6, 13)},
            { Keyboard.BottomLED12, Tuple.Create(6, 12)},
            { Keyboard.BottomLED11, Tuple.Create(6, 11)},
            { Keyboard.BottomLED10, Tuple.Create(6, 10)},
            { Keyboard.BottomLED9, Tuple.Create(6, 9)},
            { Keyboard.BottomLED8, Tuple.Create(6, 8)},
            { Keyboard.BottomLED7, Tuple.Create(6, 7)},
            { Keyboard.BottomLED6, Tuple.Create(6, 6)},
            { Keyboard.BottomLED5, Tuple.Create(6, 5)},
            { Keyboard.BottomLED4, Tuple.Create(6, 4)},
            { Keyboard.BottomLED3, Tuple.Create(6, 3)},
            { Keyboard.BottomLED2, Tuple.Create(6, 2)},
            { Keyboard.BottomLED1, Tuple.Create(6, 1)},

            { Keyboard.LeftLED5, Tuple.Create(5, 0)},
            { Keyboard.LeftLED4, Tuple.Create(4, 0)},
            { Keyboard.LeftLED3, Tuple.Create(3, 0)},
            { Keyboard.LeftLED2, Tuple.Create(2, 0)},
            { Keyboard.LeftLED1, Tuple.Create(1, 0)},
        };

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>
            {
                new List<Keyboard>()
                {
                    Keyboard.TopLED1,
                    Keyboard.TopLED2,
                    Keyboard.TopLED3,
                    Keyboard.TopLED4,
                    Keyboard.TopLED5,
                    Keyboard.TopLED6,
                    Keyboard.TopLED7,
                    Keyboard.TopLED8,
                    Keyboard.TopLED9,
                    Keyboard.TopLED10,
                    Keyboard.TopLED11,
                    Keyboard.TopLED12,
                    Keyboard.TopLED13,
                    Keyboard.TopLED14,
                    Keyboard.TopLED15,
                    Keyboard.TopLED16,
                    Keyboard.TopLED17,
                    Keyboard.TopLED18,
                    Keyboard.TopLED19,
                    Keyboard.TopLED20,

                    Keyboard.RightLED1,
                    Keyboard.RightLED2,
                    Keyboard.RightLED3,
                    Keyboard.RightLED4,
                    Keyboard.RightLED5,

                    Keyboard.BottomLED20,
                    Keyboard.BottomLED19,
                    Keyboard.BottomLED18,
                    Keyboard.BottomLED17,
                    Keyboard.BottomLED16,
                    Keyboard.BottomLED15,
                    Keyboard.BottomLED14,
                    Keyboard.BottomLED13,
                    Keyboard.BottomLED12,
                    Keyboard.BottomLED11,
                    Keyboard.BottomLED10,
                    Keyboard.BottomLED9,
                    Keyboard.BottomLED8,
                    Keyboard.BottomLED7,
                    Keyboard.BottomLED6,
                    Keyboard.BottomLED5,
                    Keyboard.BottomLED4,
                    Keyboard.BottomLED3,
                    Keyboard.BottomLED2,
                    Keyboard.BottomLED1,

                    Keyboard.LeftLED5,
                    Keyboard.LeftLED4,
                    Keyboard.LeftLED3,
                    Keyboard.LeftLED2,
                    Keyboard.LeftLED1,
                }
            };
        }

        /// <summary>
        /// Byte 4(0x00) & 5(0x32) is for led count = 50 led
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            _displayColorBytes = new List<byte>();
            keyColor = new Dictionary<Keyboard, ColorRGB>();
            List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x02, 0x01, 0x00, 0x32, 0x00 };
            foreach (KeyValuePair<Keyboard, Tuple<int, int>> led in ColorToLayout)
            {
                ColorRGB color = colorMatrix[led.Value.Item1, led.Value.Item2];
                keyColor.Add(led.Key, color);
                color.CNVSBrightnessAdjustment();
                byte[] grb = new byte[] { color.G, color.R, color.B };
                collectBytes.AddRange(grb);
            }

            _displayColorBytes.AddRange(collectBytes);
        }

        /// <summary>
        /// Byte 4(0x00) & 5(0x32) is for led count = 50 led
        /// </summary>
        protected override void TurnOffLed()
        {
            _displayColorBytes = new List<byte>();
            List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x02, 0x01, 0x00, 0x32, 0x00 };
            foreach (KeyValuePair<Keyboard, Tuple<int, int>> led in ColorToLayout)
            {
                byte[] dark = new byte[] { 0x00, 0x00, 0x00 };
                collectBytes.AddRange(dark);
            }

            _displayColorBytes.AddRange(collectBytes);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            /*Not implemented for CNVS since current UI didn't have this feature*/
        }
    }
}
