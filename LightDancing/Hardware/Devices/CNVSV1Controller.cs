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
    internal class CNVSV1Controller : ILightingControl
    {
        private const string DEVICE_PID = "VID_3402&PID_0B01"; //2023 April MP CNVS (Hardware version 2) 
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
                    CNVSV1Device device = new CNVSV1Device(stream.Item1, stream.Item2);
                    hardwares.Add(device);
                }
            }

            return hardwares;
        }
    }

    public class CNVSV1Device : USBDeviceBase
    {
        public bool UpdateRequested { get; set; }

        private bool _isFwAnimationON = true;

        private bool _updating = false;

        private Tuple<bool, string> result;

        private bool _turnOffStartupAnimation, _playAnimationWhenPCOff;

        public CNVSV1Device(SerialStream deviceStream, string serialID) : base(deviceStream, serialID)
        {
            GetCnvsSettingFromFW();
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            CNVSLeft cnvs = new CNVSLeft(_model);
            hardwares.Add(cnvs);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_isFwAnimationON)
            {
                TurnFwAnimationOFF();
            }

            if (_lightingBase.Count != 1)
            {
                Trace.WriteLine($"CNVSV1 device count error");
            }
            else
            {
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
                    Trace.WriteLine($"False to streaming on CNVSV1");
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = _serialID,
                USBDeviceType = USBDevices.CNVSv1,
                Name = "HYTE CNVS V1"
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
                Trace.WriteLine($"False to set startup animation settings on CNVSv1");
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
                WebRequest request = WebRequest.Create("https://hyte.s3.eu-west-2.amazonaws.com/fw/cnvsv1/latest.json");
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

                STM32Directory.SetUpdateBinPath(Tuple.Create(USBDevices.CNVSv1, updateFile));

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
}
