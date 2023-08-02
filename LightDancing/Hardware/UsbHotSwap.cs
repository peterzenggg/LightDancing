using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;

namespace LightDancing.Hardware
{
    public class UsbHotSwap
    {
        private int _previousDeviceCount = 0;
        private int _previousDFUCount = 0;
        public readonly Action DeviceChange;
        public readonly Action DFUChange;
        private readonly object _locker = new object();
        private const string DFU_VIDPID = @"\\" + "VID_3402&PID_0A00" + @"\\";
        private readonly List<string> PRODUCT_ID_LIST = new List<string>()
        {
        };

        private readonly Dictionary<USBDevices, List<string>> USBContllerList = new Dictionary<USBDevices, List<string>>()
        {
            {USBDevices.CNVS,           new List<string>(){@"\\" + "VID_3402&PID_0BFF" + @"\\"}},
            {USBDevices.CNVSv1,         new List<string>(){@"\\" + "VID_3402&PID_0B01" + @"\\"}},
            {USBDevices.CNVSLeft,       new List<string>(){@"\\" + "VID_3402&PID_0B00" + @"\\"}},
            {USBDevices.RedragonKeeb,   new List<string>(){@"\\" + "VID_3402&PID_0301" + @"\\"}},
            {USBDevices.RedragonMouse,  new List<string>(){@"\\" + "VID_3402&PID_0200" + @"\\"}},
            {USBDevices.IBPMiniHub,     new List<string>(){@"\\" + "VID_3402&PID_0900" + @"\\"}},
            {USBDevices.RGBWall,        new List<string>(){@"\\" + "VID_3402&PID_0902" + @"\\"}},
            {USBDevices.qRGBDemo,       new List<string>(){@"\\" + "VID_3402&PID_0903" + @"\\"}},
            {USBDevices.Suoai_MK4,      new List<string>(){@"\\" + "VID_258A&PID_0144" + @"\\"}},
            {USBDevices.Redragon_MK4,   new List<string>(){@"\\" + "VID_258A&PID_0145" + @"\\"}},
            {USBDevices.RazerBlackwidowV3Keyboard,      new List<string>(){@"\\" + "VID_1532&PID_024E" + @"\\"}},
            {USBDevices.CorsairK100RGBKeyboard,         new List<string>(){@"\\" + "VID_1B1C&PID_1B7C" + @"\\", @"\\" + "VID_1B1C&PID_1B7D" + @"\\" , @"\\" + "VID_1B1C&PID_1BC5" + @"\\" } },
            {USBDevices.CorsairK60ProLowProfile,        new List<string>(){@"\\" + "VID_1B1C&PID_1BAD" + @"\\"}},
            {USBDevices.CorsairK70MKII,                 new List<string>(){@"\\" + "VID_1B1C&PID_1B49" + @"\\" }},
            {USBDevices.DygmaRaise,             new List<string>(){@"\\" + "VID_1209&PID_2201" + @"\\" }},
            {USBDevices.RazerHuntsmanV2TKL,     new List<string>(){@"\\" + "VID_1532&PID_026B" + @"\\" }},
            {USBDevices.WhirlwindFxElement,     new List<string>(){@"\\" + "VID_0483&PID_A33E" + @"\\" }},
            {USBDevices.HyperXAlloyCore,        new List<string>(){@"\\" + "VID_03F0&PID_098F" + @"\\"}},
            {USBDevices.RazerBlackwidowV3MiniKeyboard,  new List<string>(){@"\\" + "VID_1532&PID_0258" + @"\\" }},
            {USBDevices.MountainEverest,new List<string>(){ @"\\" + "VID_3282&PID_0001" + @"\\" } },
            //{USBDevices.SteelSeriesApex5,new List<string>(){ @"\\" + "VID_1038&PID_161C" + @"\\" } },
            //{USBDevices.NanoleafShapes,new List<string>() },
            {USBDevices.RazerStriderChroma,new List<string>(){ @"\\" + "VID_1532&PID_0C05" + @"\\" }},
            //{USBDevices.PhilipsHueBridge,new List<string>() },
            //{USBDevices.ASUSZ790A,new List<string>() },
            {USBDevices.CorsairLT100Tower,new List<string>(){ @"\\" + "VID_1B1C&PID_0C23" + @"\\" }},
            {USBDevices.RazerBaseStationChroma,new List<string>(){ @"\\" + "VID_1532&PID_0F08" + @"\\" }},
            {USBDevices.ViewSonicXG270QG,new List<string>(){ @"\\" + "VID_0416&PID_5020" + @"\\" } },
            {USBDevices.ViewSonicXG271QG,new List<string>(){ @"\\" + "VID_0543&PID_A004" + @"\\" } },
            {USBDevices.RazerMambaElite,new List<string>(){ @"\\" + "VID_1532&PID_006C" + @"\\" } },
            //{USBDevices.CorsairLightingCommanderCore,new List<string>(){ @"\\" + "VID_1b1c&PID_0C32" + @"\\" } },
            {USBDevices.SuoaiKeebTKL, new List<string>(){ @"\\" + "VID_3402&PID_0300" + @"\\" }},
            //{USBDevices.HotKeyboard,  @"\\" + "VID_3402&PID_0301" + @"\\"},
            {USBDevices.Q60,  new List<string>(){@"\\" + "VID_3402&PID_0400" + @"\\" } },
            {USBDevices.NP50,  new List<string>(){@"\\" + "VID_3402&PID_0901" + @"\\" } },
            {USBDevices.CoyaLedStrip,  new List<string>(){@"\\" + "VID_3402&PID_0933" + @"\\" } },
        };

        /// <summary>
        /// Init UsbHotSwap with register actions and init PRODUCT_ID_LIST
        /// </summary>
        /// <param name="deviceChange">Function to refresh device list</param>
        /// <param name="dFUChange">Function to refresh DFU device list</param>
        /// <param name="uSBDevices">List of USBDevices that will be detected</param>
        public UsbHotSwap(Action deviceChange, Action dFUChange, List<USBDevices> uSBDevices)
        {
            try
            {
                ChangeUSBDeviceControllerList(uSBDevices);
                DeviceChange = deviceChange;
                DFUChange = dFUChange;
                InitDeviceCount();
                new System.Threading.Thread(CheckFunction) { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UsbHotSwap/Construct is failed. Exception: {ex}");
            }
        }

        private void InitDeviceCount()
        {
            using var USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            int NewCount = 0;
            int DFUCount = 0;

            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {
                    string Dependent = (USBControllerDevice["Dependent"] as string).Split(new char[] { '=' })[1];

                    lock (_locker)
                    {
                        /*Find USB Devices*/
                        if (PRODUCT_ID_LIST.Count > 0)
                        {
                            foreach (string PIDVID in PRODUCT_ID_LIST)
                            {
                                Match match = Regex.Match(Dependent, PIDVID);
                                if (match.Success)
                                {
                                    NewCount += 1;
                                    break;
                                }
                            }
                        }
                    }

                    /*Find DFU Devices*/
                    Match dfuMatch = Regex.Match(Dependent, DFU_VIDPID);
                    if (dfuMatch.Success)
                    {
                        DFUCount++;
                    }
                }
                _previousDeviceCount = NewCount;
                _previousDFUCount = DFUCount;
            }
        }

        private void CheckFunction()
        {
            while (true)
            {
                try
                {
                    int NewCount = 0;
                    int DFUCount = 0;
                    using var USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
                    if (USBControllerDeviceCollection != null)
                    {
                        foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                        {
                            string Dependent = (USBControllerDevice["Dependent"] as string).Split(new char[] { '=' })[1];
                            USBControllerDevice.Dispose();

                            lock (_locker)
                            {
                                if (PRODUCT_ID_LIST.Count > 0)
                                {
                                    foreach (string PIDVID in PRODUCT_ID_LIST)
                                    {
                                        Match match = Regex.Match(Dependent, PIDVID);
                                        if (match.Success)
                                        {
                                            NewCount += 1;
                                            break;
                                        }
                                    }
                                }
                            }

                            /*Find DFU Devices*/
                            Match dfuMatch = Regex.Match(Dependent, DFU_VIDPID);
                            if (dfuMatch.Success)
                            {
                                DFUCount++;
                            }
                        }

                        if (_previousDeviceCount != NewCount)
                        {
                            _previousDeviceCount = NewCount;
                            DeviceChange?.Invoke();
                        }

                        if (_previousDFUCount != DFUCount)
                        {
                            _previousDFUCount = DFUCount;
                            DFUChange?.Invoke();
                        }
                        //GC.Collect();

                    }
                    USBControllerDeviceCollection.Dispose();

                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UsbHotSwap/CheckFunction is failed. Waiting for next time. Exception: {ex}");
                }
            }
        }

        /// <summary>
        /// Change USB Devices Detecting List in USBHotSwap
        /// </summary>
        /// <param name="uSBDevices">List of USBDevices</param>
        public void ChangeUSBDeviceControllerList(List<USBDevices> uSBDevices)
        {
            lock (_locker)
            {
                PRODUCT_ID_LIST.Clear();

                foreach (var device in uSBDevices)
                {
                    if (USBContllerList[device].Count > 0)
                    {
                        foreach (var idString in USBContllerList[device])
                        {
                            PRODUCT_ID_LIST.Add(idString);
                        }
                    }
                }
            }
        }
        public static string GetBoardName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            string result = "";
            foreach (ManagementObject obj in searcher.Get())
            {
                result = obj["Product"].ToString();
            }
            return result;
        }
    }
}
