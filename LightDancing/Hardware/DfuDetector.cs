using LightDancing.Enums;
using LightDancing.Hardware.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LightDancing.Hardware
{
    public static class DfuDetector
    {
        public static List<DFUDevice> _updatingDevices = new List<DFUDevice>();

        public static Action DfuDevicesChanged;

        /// <summary>
        /// Only update 1 dfu device per hotswap
        /// </summary>
        /// <param name="usbDevices">The devices that has been update requested</param>
        /// <returns></returns>
        public static List<DFUDevice> GetDfuData(List<USBDeviceBase> usbDevices)
        {
            if (!STM32Directory.STM32_Setting)
            {
                Trace.WriteLine("STM32 path file not set yet");
            }

            int dfuCount = GetDFUCount();
            List<DFUDevice> dfuDevices = new List<DFUDevice>();

            int updateTarget = 0;
            updateTarget = dfuCount + 1;

            for (int i = 1; i <= dfuCount; i++)
            {
                /*Check if the dfu device is our product*/
                Tuple<bool, USBDevices> checkProduct = CheckDFUPidVid(i);

                if (checkProduct != null && checkProduct.Item1)
                {
                    if (updateTarget > i)
                    {
                        updateTarget = i;
                    }
                    /*Get Dfu Serial ID*/
                    string serialID = GetDFUSerialID(i);
                    /*Create DFUDevice*/
                    DFUDevice dFUDevice = new DFUDevice(serialID, i);
                    /*Match the DFUDevice with CNVS lightingBase and the update status*/
                    foreach (USBDeviceBase usbBase in usbDevices)
                    {
                        if (usbBase.GetModel().DeviceID == serialID)
                        {
                            if (usbBase.GetType() == typeof(CNVSLeftDevice))
                            {
                                dFUDevice.UpdateRequested = ((CNVSLeftDevice)usbBase).UpdateRequested;
                                dFUDevice.UsbDeviceBase = usbBase;
                                break;
                            }
                            else if (usbBase.GetType() == typeof(CNVSV1Device))
                            {
                                dFUDevice.UpdateRequested = ((CNVSV1Device)usbBase).UpdateRequested;
                                dFUDevice.UsbDeviceBase = usbBase;
                                break;
                            }
                            else if (usbBase.GetType() == typeof(IBPMiniHubDevice))
                            {
                                dFUDevice.UpdateRequested = ((IBPMiniHubDevice)usbBase).UpdateRequested;
                                dFUDevice.UsbDeviceBase = usbBase;
                                break;
                            }
                        }
                    }

                    dFUDevice.Type = checkProduct.Item2;
                    dfuDevices.Add(dFUDevice);
                }
            }

            List<DFUDevice> result = new List<DFUDevice>();
            result.AddRange(dfuDevices);
            result.AddRange(_updatingDevices);

            /*Only Update one dfu device for each hotswap*/
            if (updateTarget > 0 && updateTarget <= dfuCount)
            {
                _updatingDevices.Add(dfuDevices[updateTarget - 1]);
                
                Task.Run(() =>
                {
                    bool updateFinish = false;

                    string serialId = dfuDevices[updateTarget - 1].SerialID;
                    while (!updateFinish)
                    {
                        if(dfuDevices[updateTarget - 1].CallBackSet)
                            updateFinish = ProcessDfuUpdate(dfuDevices[updateTarget - 1]);
                    }
                    int thisDevice = _updatingDevices.FindIndex(x => x.SerialID == serialId);
                    if (thisDevice >= 0)
                    {
                        _updatingDevices.RemoveAt(thisDevice);
                    }
                });
            }
            return result;
        }

        /// <summary>
        /// Below cmd command is to show the list of DFU devices
        /// STM32_Programmer_CLI.exe -c port=usb1 PID=0x0A00 VID=0x3402 -l
        /// </summary>
        /// <returns></returns>
        private static int GetDFUCount()
        {
            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb1 PID=0x0A00 VID=0x3402 -l";
            process.RunCommand(CheckStr);

            while (!process.ProcessObject.StandardOutput.EndOfStream)
            {
                string c = process.ProcessObject.StandardOutput.ReadToEnd();
                if (c.Contains("Total number of available STM32 device in DFU mode:"))
                {
                    var rr = c.Split("Total number of available STM32 device in DFU mode: ");
                    var rrr = rr[rr.Length - 1].ToString();
                    process.ProcessObject.Close();
                    return Convert.ToInt32(rrr[0].ToString());
                }
                else
                {
                    return 0;
                }
            }
            process.ProcessObject.Close();
            return 0;
        }

        /// <summary>
        /// Below cmd command is to get dfu information, -c as connect
        /// string CheckStr = @"""" + ChangeObject.CLI_Path + @"""" + " -c port=usb" + count.ToString() + " PID=0x0A00 VID=0x3402";
        /// </summary>
        /// <param name="usbNum">usb number of this dfu device</param>
        /// <returns></returns>
        private static string GetDFUSerialID(int usbNum)
        {
            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb" + usbNum.ToString() + " PID=0x0A00 VID=0x3402";
            process.RunCommand(CheckStr);

            while (!process.ProcessObject.StandardOutput.EndOfStream)
            {
                string c = process.ProcessObject.StandardOutput.ReadToEnd();

                /*SN is the serial number*/
                if (c.Contains("SN"))
                {
                    var readLine = c.Split("SN          : ")[1];
                    var snLine = readLine.ToString().Split("\r");
                    process.ProcessObject.Close();
                    return snLine[0];
                }
                else
                {
                    return null;
                }
            }
            process.ProcessObject.Close();
            return null;
        }

        /// <summary>
        /// Below cmd command is to check the data in EEprome which the address is 0x0801F000
        /// @"""" + ChangeObject.CLI_Path + @"""" + " -c port=usb" + count.ToString() + " PID=0x0A00 VID=0x3402 -r32 0x0801F000 0x1000"
        /// if the data is = 0x0801FFF0 : 12345AAA 12345678, it is our product
        /// </summary>
        /// <param name="usbNum">usb number of this dfu device</param>
        /// <returns>true and the device type or null for not our device</returns>
        public static Tuple<bool, USBDevices> CheckDFUPidVid(int usbNum)
        {
            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb" + usbNum.ToString() + " PID=0x0A00 VID=0x3402 -r32 0x0801FFF0 0x10";
            process.RunCommand(CheckStr);

            while (!process.ProcessObject.StandardOutput.EndOfStream)
            {
                string c = process.ProcessObject.StandardOutput.ReadToEnd();

                /*CNVS Left*/
                if (c.Contains("0x0801FFF0 : 0B0000DD"))
                {
                    process.ProcessObject.Close();
                    return Tuple.Create(true, USBDevices.CNVSLeft);
                }
                /*CNVS V1*/
                else if (c.Contains("0x0801FFF0 : 0B0100DD"))
                {
                    process.ProcessObject.Close();
                    return Tuple.Create(true, USBDevices.CNVSv1);
                }
                /*Mini Hub*/
                else if (c.Contains("0x0801FFF0 : 090000DD"))
                {
                    process.ProcessObject.Close();
                    return Tuple.Create(true, USBDevices.IBPMiniHub);
                }
                else
                {
                    process.ProcessObject.Close();
                    return null;
                }
            }
            process.ProcessObject.Close();

            return null;
        }

        /// <summary>
        /// Process DFU update (latest update or return default version)
        /// </summary>
        /// <param name="dfuDevice"></param>
        /// <returns>true when update is success and back to usb mode or when the device is plugged off</returns>
        public static bool ProcessDfuUpdate(DFUDevice dfuDevice)
        {
            if (dfuDevice.UpdateRequested)
            {
                return Update(dfuDevice);
            }
            else
            {
                return SetToDefault(dfuDevice);
            }
        }

        /// <summary>
        /// To update the firmware to the latest fw version on the cloud
        /// </summary>
        /// <param name="dfuDevice"></param>
        /// <returns>true when update is success and back to usb mode or when the device is plugged off</returns>
        private static bool Update(DFUDevice dfuDevice)
        {
            /*If the device is update requested but cannot find the update bin file*/
            if (dfuDevice.Type == USBDevices.CNVSLeft || dfuDevice.Type == USBDevices.CNVSv1 || dfuDevice.Type == USBDevices.IBPMiniHub)
            {
                /*If the device is update requested but cannot find the update bin file*/
                if (!System.IO.File.Exists(STM32Directory.UPDATE_BIN_PATH[dfuDevice.Type]))
                {
                    //_updateStatus?.Invoke("UPdate_Bin Not exit");
                    dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "UPdate_Bin Not exit"));
                    Finish(dfuDevice);
                }
            }
            else
            {
                Trace.WriteLine("dfuDevice did not exist");
            }

            dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Start Updating"));

            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb" + dfuDevice.UsbNumber.ToString() + " PID=0x0A00 VID=0x3402 -w " + @"""" + STM32Directory.UPDATE_BIN_PATH[dfuDevice.Type] + @"""" + " 0x0800C000 -v";
            process.RunCommand(CheckStr);
            Thread.Sleep(10);

            while (true)
            {
                var c = process.ProcessObject.StandardOutput.ReadLineAsync();
                if (c.Wait(10000))
                {
                    /*This error statement needs to be infront of any other errors*/
                    if (c.Result.Contains("Error: Target device not found"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Device not found"));
                        if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSLeftDevice))
                            ((CNVSLeftDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                        else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSV1Device))
                            ((CNVSV1Device)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                        else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(IBPMiniHubDevice))
                            ((IBPMiniHubDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                        process.ProcessObject.Close();

                        return true;
                    }

                    if (c.Result.Contains("Error"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Update Error"));
                        dfuDevice.UpdateRequested = false;
                        process.ProcessObject.Close();

                        return false;
                    }

                    if (c.Result.Contains("Start operation achieved successfully") || c.Result.Contains("Download verified successfully"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(true, "Update Success"));
                        process.ProcessObject.Close();

                        return Finish(dfuDevice);
                    }
                }
                else
                {
                    dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Update Error"));
                    dfuDevice.UpdateRequested = false;
                    process.ProcessObject.Close();

                    return false;
                }
            }
        }

        /// <summary>
        /// Set the firmware back to default version
        /// </summary>
        /// <param name="dfuDevice"></param>
        /// <returns>true when update is success and back to usb mode or when the device is plugged off</returns>
        private static bool SetToDefault(DFUDevice dfuDevice)
        {
            dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Start Setting to Default"));

            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb" + dfuDevice.UsbNumber.ToString() + " PID=0x0A00 VID=0x3402 -w " + @"""" + STM32Directory.DEFAULT_BIN_PATH[dfuDevice.Type] + @"""" + " 0x0800C000 -v";
            process.RunCommand(CheckStr);

            while (true)
            {
                var c = process.ProcessObject.StandardOutput.ReadLineAsync();

                if (c.Wait(10000))
                {
                    if (c.Result.Contains("Start operation achieved successfully") || c.Result.Contains("Download verified successfully"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(true, "Set to Default Success"));

                        process.ProcessObject.Close();
                        return Finish(dfuDevice);
                    }

                    if (c.Result.Contains("Error: Target device not found"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Device not found"));
                        if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSLeftDevice))
                            ((CNVSLeftDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                        else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSV1Device))
                            ((CNVSV1Device)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                        else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(IBPMiniHubDevice))
                            ((IBPMiniHubDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;

                        process.ProcessObject.Close();
                        return true;
                    }

                    if (c.Result.Contains("Error"))
                    {
                        dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Set to Default Error"));

                        process.ProcessObject.Close();
                        return false;
                    }
                }
                else
                {
                    dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Set to Default Timeout Error"));

                    process.ProcessObject.Close();
                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dfuDevice"></param>
        /// <returns>true when device is return successfully</returns>
        private static bool Finish(DFUDevice dfuDevice)
        {
            CmdProcess process = new CmdProcess();
            string CheckStr = @"""" + STM32Directory.CLI_PATH + @"""" + " -c port=usb" + dfuDevice.UsbNumber.ToString() + " PID=0x0A00 VID=0x3402 -e 63 63 -s 0x0800C000";
            process.RunCommand(CheckStr);

            while (!process.ProcessObject.StandardOutput.EndOfStream)
            {
                string c = process.ProcessObject.StandardOutput.ReadToEnd();

                if (c.Contains("Start operation achieved successfully"))
                {
                    dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Return Success"));
                    if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSLeftDevice))
                        ((CNVSLeftDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSV1Device))
                        ((CNVSV1Device)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(IBPMiniHubDevice))
                    {
                        ((IBPMiniHubDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    }

                    process.ProcessObject.Close();
                    return true;
                }

                if (c.Contains("Error: Target device not found"))
                {
                    dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Device not found"));
                    if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSLeftDevice))
                        ((CNVSLeftDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(CNVSV1Device))
                        ((CNVSV1Device)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    else if (dfuDevice.UsbDeviceBase != null && dfuDevice.UsbDeviceBase.GetType() == typeof(IBPMiniHubDevice))
                    {
                        ((IBPMiniHubDevice)dfuDevice.UsbDeviceBase).UpdateRequested = false;
                    }

                    process.ProcessObject.Close();
                    return true;
                }
            }

            dfuDevice.ChangeUpdateStatus(Tuple.Create(false, "Return Error"));
            process.ProcessObject.Close();
            return false;
        }
    }

    public class DFUDevice
    {
        public bool UpdateRequested { get; set; }
        public string SerialID { get; set; }
        public int UsbNumber { get; set; }

        public string UpdateStatus { get; private set; }

        public Action<string> StatusUpdateStr;

        public USBDeviceBase UsbDeviceBase { get; set; }

        public USBDevices Type { get; set; }

        public bool CallBackSet { get; set; }

        public DFUDevice(string serialID, int usbNumber)
        {
            UpdateRequested = false;
            CallBackSet = false;
            SerialID = serialID;
            UsbNumber = usbNumber;
            ChangeUpdateStatus(Tuple.Create(false, "Initiated"));
        }

        public void ChangeUpdateStatus(Tuple<bool, string> updates)
        {
            if (UsbDeviceBase != null && UsbDeviceBase.GetType() == typeof(IBPMiniHubDevice))
            {
                ((IBPMiniHubDevice)UsbDeviceBase).ChangeUpdateStatus(updates);
            }
            else if (UsbDeviceBase != null && UsbDeviceBase.GetType() == typeof(CNVSLeftDevice))
            {
                ((CNVSLeftDevice)UsbDeviceBase).ChangeUpdateStatus(updates);
            }
            else if (UsbDeviceBase != null && UsbDeviceBase.GetType() == typeof(CNVSV1Device))
            {
                ((CNVSV1Device)UsbDeviceBase).ChangeUpdateStatus(updates);
            }

            UpdateStatus = updates.Item2;
            StatusUpdateStr?.Invoke(updates.Item2); ;
        }

        /// <summary>
        /// For UI to register for update device status
        /// </summary>
        /// <param name="func"></param>
        public void RegisterStatusUpdate(Action<string> func)
        {
            CallBackSet = true;
            StatusUpdateStr = func;
        }
    }

    public class CmdProcess
    {
        public Process ProcessObject { get; }

        public CmdProcess()
        {
            ProcessObject = new Process();
            ProcessObject.StartInfo.FileName = "cmd.exe";
            ProcessObject.StartInfo.UseShellExecute = false;
            ProcessObject.StartInfo.RedirectStandardInput = true;
            ProcessObject.StartInfo.RedirectStandardOutput = true;
            ProcessObject.StartInfo.RedirectStandardError = true;
            ProcessObject.StartInfo.CreateNoWindow = true;
        }

        public void RunCommand(string returnString)
        {
            ProcessObject.Start();
            ProcessObject.StandardInput.WriteLine(returnString);
            ProcessObject.StandardInput.Flush();
            ProcessObject.StandardInput.Close();
        }
    }
}
