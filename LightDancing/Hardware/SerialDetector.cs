using HidSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace LightDancing.Hardware
{
    public class SerialDetector
    {
        /// <summary>
        /// According the recognizeID to get the info of serial devices.
        /// </summary>
        /// <param name="recognizeID"></param>
        /// <returns></returns>
        public List<SerialStream> GetSerialStreams(string recognizeID)
        {
            List<SerialStream> result = null;
            var portsName = GetPortFriendlyName(recognizeID);

            if (portsName != null && portsName.Count > 0)
            {
                foreach (SerialDevice serialDevice in DeviceList.Local.GetSerialDevices())
                {
                    try
                    {
                        string portName = serialDevice.GetFriendlyName();
                        if (portsName.Contains(portName))
                        {
                            int errorCount = 0;
                            bool OpenResult = false;
                            if (result == null)
                            {
                                result = new List<SerialStream>();
                            }

                            while (!OpenResult)
                            {
                                OpenResult = serialDevice.TryOpen(out SerialStream stream);
                                if (OpenResult)
                                {
                                    result.Add(stream);
                                    break;
                                }

                                Debug.WriteLine($"Open failed with {recognizeID}, the devices is been occupy!");
                                Thread.Sleep(500);
                                errorCount++;
                                if (errorCount > 10)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //MEMO: Need to implement error handle and log
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Recognize the serial devices from iBuypower, ref by win32
        /// </summary>
        /// <returns>iBuypower's com port devices</returns>
        private List<string> GetPortFriendlyName(string recognizeID)
        {
            List<string> result = null;
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["DeviceID"].ToString().Contains(recognizeID))
                    {
                        if (result == null)
                            result = new List<string>();

                        var friendlyName = queryObj["Name"].ToString();
                        result.Add(friendlyName);
                    }
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        public List<Tuple<SerialStream, string>> GetSerialStreamsAndSerialID(string recognizeID)
        {
            List<Tuple<SerialStream, string>> result = null;
            var portsName = GetPortFriendlyNameAndSerialID(recognizeID);

            if (portsName.Item1 != null && portsName.Item1.Count > 0)
            {
                foreach (SerialDevice serialDevice in DeviceList.Local.GetSerialDevices())
                {
                    try
                    {
                        string portName = serialDevice.GetFriendlyName();
                        if (portsName.Item1.Contains(portName))
                        {
                            int errorCount = 0;
                            bool openResult = false;
                            if (result == null)
                            {
                                result = new List<Tuple<SerialStream, string>>();
                            }
                            while (!openResult)
                            {
                                openResult = serialDevice.TryOpen(out SerialStream stream);
                                if (openResult)
                                {
                                    int count = portsName.Item1.FindIndex(x => x.Contains(portName));
                                    result.Add(Tuple.Create(stream, portsName.Item2[count]));

                                    break;
                                }
                                Debug.WriteLine($"Open failed with {recognizeID}, the devices is been occupy!");
                                Thread.Sleep(500);
                                errorCount++;
                                if (errorCount > 10)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //MEMO: Need to implement error handle and log
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }

            return result;
        }


        private Tuple<List<string>, List<string>> GetPortFriendlyNameAndSerialID(string recognizeID)
        {
            Tuple<List<string>, List<string>> result;
            List<string> names = null;
            List<string> serialIDs = null;
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["DeviceID"].ToString().Contains(recognizeID))
                    {
                        if (names == null)
                            names = new List<string>();
                        if (serialIDs == null)
                            serialIDs = new List<string>();


                        var friendlyName = queryObj["Name"].ToString();
                        string serialID = queryObj["DeviceID"].ToString().Split(recognizeID + "\\")[1];
                        names.Add(friendlyName);
                        serialIDs.Add(serialID);
                    }
                }

                result = Tuple.Create(names, serialIDs);
            }
            catch
            {
                return null;
            }

            return result;
        }
    }
}
