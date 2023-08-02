using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.Hardware;
using LightDancing.OpenTKs;
using LightDancing.OpenTKs.Animations;
using LightDancing.Syncs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using LightDancing.Hardware.Devices;
using LightDancing.Syncs.ImageFilter;
using System.Collections.Generic;
using LightDancing.Colors;
using HueApi.Entertainment.Models;
using HueApi.Entertainment;
using System.Linq;
using HueApi.ColorConverters;
using HueApi.Entertainment.Extensions;
using LightDancing.Hardware.Devices.UniversalDevice.ASUS.MotherBoard;
using System.Drawing;
using LightDancing.Hardware.Devices.Fans;
using HidSharp;
using System.Security.Cryptography;

namespace LightDancingUnitTest.Hardwares
{
    [TestClass]
    public class UnitTest
    {
        private readonly HardwaresDetector _detector = HardwaresDetector.Instance;

        [TestMethod]
        public void TestAnimationCoding2Devices()
        {
            var devices = _detector.GetLightingDevices();

            foreach (AnimationCoding type in Enum.GetValues(typeof(AnimationCoding)))
            {
                Trace.WriteLine($"Set all devices with AnimationCoding: {type}");
                foreach (LightingBase device in devices)
                {
                    _detector.SetStreaming(device, type);
                }

                Trace.WriteLine($"Test Start processing with : {type}, in {Common.ConstValues.PROCESS_MS * 2} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 200);
                Trace.WriteLine($"Test Stop processing with : {type}, in {Common.ConstValues.PROCESS_MS} sec");
                _detector.StopStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
                _detector.StopStreaming();
            }
        }

        [TestMethod]
        public void TestOpenTkSync2Devices()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            helper.ChangeShader(new AnimationGroup(BackgroundShaders.Arrow, null, null));

            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
            };

            Task.Run(() =>
            {
                helper.Start(parameters);
            });

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with OpenTk streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 300);
                _detector.StopStreaming();
            }
        }

        [TestMethod]
        public void TestScreenSync2Devices()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestSwitchScreen2Devices()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(2000);
                var monitorList = helper.GetMonitorList();

                foreach (var monitor in monitorList)
                {
                    //helper.ChangeMonitor(monitor.Key);
                    Thread.Sleep(2000);
                }

                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestScreenSyncMirror()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();

                Thread.Sleep(2000);

                _detector.SetMirror(true);

                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestScreenSyncBlur()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();

                Thread.Sleep(2000);
                helper.SetBlur(new FrameBlur());

                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestStreamingStop()
        {
            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    _detector.SetStreaming(device, AnimationCoding.Rainbow);
                }

                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
                _detector.StopStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
            }
        }

        [TestMethod]
        public void TestLedTurnOff()
        {
            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Debug.WriteLine("Turn off the LED rgb with all devices");
                foreach (var device in devices)
                {
                    _detector.SetStreaming(device, AnimationCoding.Rainbow);
                    _detector.SetLedTurnOn(device, false);
                }
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
                _detector.StopStreaming();

                Debug.WriteLine("Turn on the LED rgb with all devices");
                foreach (var device in devices)
                {
                    _detector.SetStreaming(device, AnimationCoding.Rainbow);
                    _detector.SetLedTurnOn(device, true);
                }
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS);
                _detector.StopStreaming();
            }
        }

        int SW_LU = 0;
        int SW_LB = 0;
        int SW_RU = 0;
        int SW_RB = 0;
        [TestMethod]
        public void TestScrollWheels()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                _detector.RegisterScrollWheelCallback(TestSW);

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                //_detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 9);
                //_detector.StopStreaming();
                _detector.Dispose();
                Thread.Sleep(1000);
                helper.Stop();
            }
        }

        private void TestSW()
        {
            var scrollWheel = _detector.GetScrollWheel();
            if (scrollWheel == ScrollWheelsMode.ScrollWheels_TopLeft)
            {
                SW_LU++;
                Debug.WriteLine("{0}:{1}", scrollWheel, SW_LU);
            }
            else if (scrollWheel == ScrollWheelsMode.ScrollWheels_BottomLeft)
            {
                SW_LB++;
                Debug.WriteLine("{0}:{1}", scrollWheel, SW_LB);
            }
            else if (scrollWheel == ScrollWheelsMode.ScrollWheels_TopRight)
            {
                SW_RU++;
                Debug.WriteLine("{0}:{1}", scrollWheel, SW_RU);
            }
            else if (scrollWheel == ScrollWheelsMode.ScrollWheels_BottomRight)
            {
                SW_RB++;
                Debug.WriteLine("{0}:{1}", scrollWheel, SW_RB);
            }
        }

        [TestMethod]
        public void TestScreenManualColorAdjust()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            helper.UpdateMode(new ManualAdjustment(0.9f, 0.1f));

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestGetKeyColor()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }
                _detector.RegisterKeyColorCallback(GetKeyColor);
                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3000);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        public void GetKeyColor()
        {
            var colors = _detector.GetKeyColor();
        }

        [TestMethod]
        public void TestGifSync2Devices()
        {
            GifSyncHelper helper = GifSyncHelper.Instance;

            helper.ReadFile(@"C:\Users\Jasmine Tseng\Desktop\Image\source.gif", out string fileName, out System.Drawing.Image image, out string errorMsg);

            Task.Run(() =>
            {
                helper.Start();
            });

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with OpenTk streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
            }
        }

        [TestMethod]
        public void TestScreenSyncBlueReduction()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();
            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(5000);
                helper.SetBlueFilter(true);
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestAnimateBlueReduction()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;

            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Animate,
                Animate = "ColorStrip",
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });
            Thread.Sleep(3000);

            System.Collections.Generic.List<LightDancing.Colors.ColorRGB> colorRGBs = new System.Collections.Generic.List<LightDancing.Colors.ColorRGB>()
            {
                new ColorRGB(42, 127, 161),
            new ColorRGB(98, 86, 161),
            new ColorRGB(161, 50, 140),
            new ColorRGB(161, 94, 81),
            new ColorRGB(161, 105, 42),
            new ColorRGB(141, 161, 48),
            new ColorRGB(44, 161, 38),
            };

            helper.ChangeColorSet(colorRGBs);
            helper.SetColorFilter(ColorFilter.BlueFilter, 1.0f);
            var devices = _detector.GetLightingDevices();
            
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(5000);
                helper.SetBlueFilter(true);
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestGifBlueReduction()
        {
            GifSyncHelper helper = GifSyncHelper.Instance;

            //helper.ReadFile(@"C:\Users\Jasmine Tseng\Desktop\Image\tired-boo.webm", out string fileName, out System.Drawing.Image image, out string errorMsg);
            helper.ReadFile(@"C:\Users\Jasmine Tseng\Desktop\Image\source.gif", out string fileName, out System.Drawing.Image image, out string errorMsg);

            Task.Run(() =>
            {
                helper.Start();
            });

            var devices = _detector.GetLightingDevices();

            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(5000);
                helper.SetBlueFilter(true);
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestUsbHotSwapWithScreen()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            //helper.Start();

            HardwaresDetector.Instance.Changed += ReStreaming;
            var devices = _detector.GetLightingDevices();
            var fans = _detector.GetFanDevices();
            Debug.WriteLine("fanCount "+fans.Count);
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(200000);

                _detector.StopStreaming();
                helper.Stop();
            }
        }

        private void ReStreaming(object sender, string str)
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            var devices = _detector.GetLightingDevices();
            var deviceGroup = _detector.GetFanGroups();
            var fans = _detector.GetFanDevices();
            Debug.WriteLine("fanCount " + fans.Count +"hotswap");
            if (devices != null && devices.Count > 0)
            {
                _detector.StopStreaming();
                Trace.WriteLine($"Reset all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                _detector.ProcessStreaming();
                Thread.Sleep(2000);
                
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestOTA()
        {
            HardwaresDetector.Instance.DFUChanged += DFUUpdate;
            STM32Directory.SetSTM32Directory(@"C:\\Program Files (x86)\\HYTE\\stm32cubeprogrammer");
            var devices = _detector.GetUSBDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Find CNVS");
                foreach (var device in devices)
                {
                    if(device.GetType() == typeof(CNVSV1Device))
                    {
                        var updateAbility = device.GetUpdateAbility();
                        if(updateAbility.Item1)
                        {
                            Task.Run(() =>
                            {
                                device.Update();
                                Debug.WriteLine("Device Update: {0}", device.GetModel().DeviceID);
                            });
                        }
                    }
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                Thread.Sleep(20000);
            }
        }

        private void DFUUpdate(object sender, string str)
        {
            List<DFUDevice> dfuDevices = HardwaresDetector.Instance.GetDFUDevices();

            foreach(DFUDevice device in dfuDevices)
            {
                device.RegisterStatusUpdate(GetStatus);
            }
        }

        public void GetStatus(string status)
        {
            Debug.WriteLine("Update Status : {0}", status);
        }

        [TestMethod]
        public void TestMiniHubChangeLayout()
        {
            var devices = _detector.GetUSBDevices();
            HardwaresDetector.Instance.Changed += GetDevices;

            foreach (var device in devices)
            {
                if (device.GetType() == typeof(IBPMiniHubDevice))
                {
                    var listDevice = device.GetLightingDevice();
                }
            }

//            HardwaresDetector.Instance.ChangeMiniHubLayout(MiniHubLayout.Y40);
            Thread.Sleep(1000);
        }

        public void GetDevices(object sender, string e)
        {
            var devices = _detector.GetUSBDevices();
            foreach (var device in devices)
            {
                if (device.GetType() == typeof(IBPMiniHubDevice))
                {
                    var listDevice = device.GetLightingDevice();
                    Thread.Sleep(10);
                }
            }
        }

        [TestMethod]
        public void TestCaptureSize()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();

            var devices = _detector.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = 2,
                            Height = 2,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        [TestMethod]
        public void TestMiniHubFanRPM()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();
            DateTime dateTime = DateTime.Now;
            var devices = _detector.GetUSBDevices();
            if (devices != null && devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    if(device.GetType() == typeof(IBPMiniHubDevice))
                    {
                        bool _timeron = true;

                        while (_timeron)
                        {
                            Tuple<int, int> rpms = ((IBPMiniHubDevice)device).GetFanRPM();
                            Debug.WriteLine("Rear Fan RPM: {0}, Front Fans RPM: {1}", rpms.Item1, rpms.Item2);
                            if((DateTime.Now - dateTime).TotalMilliseconds > 5000)
                            {
                                _timeron = false;
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestAlphaVideo()
        {
            ScreenSyncHelper helper = ScreenSyncHelper.Instance;
            helper.Start();
            AlphaVideo.Instance.SetupSyncWithLCD(helper);
            helper.RegisterPreviewUpdateCallback(GetBase64);
            AlphaVideo.Instance.Updated += GetAlphaVideo;
            var devices = HardwaresDetector.Instance.GetLightingDevices();
            if (devices != null && devices.Count > 0)
            {
                Trace.WriteLine($"Set all devices with ScreenSync streaming");
                foreach (var device in devices)
                {
                    var captureInfos = new CaptureInfos()
                    {
                        StartAxis = Tuple.Create(0, 0),
                        Layouts = new MatrixLayouts()
                        {
                            Width = ScreenInfo.Instance.ScaleWidth,
                            Height = ScreenInfo.Instance.ScaleHeight,
                        },
                    };

                    _detector.SetStreaming(device, helper, captureInfos);
                }

                Trace.WriteLine($"Test Start processing with OpenTk streaming, in {Common.ConstValues.PROCESS_MS * 3} sec");
                _detector.ProcessStreaming();

                Thread.Sleep(5000);
               
                Thread.Sleep(Common.ConstValues.PROCESS_MS * 3);
                _detector.StopStreaming();
                helper.Stop();
            }
        }

        private void GetBase64()
        {
            ScreenSyncHelper.Instance.GetBase64StringColor();
        }

        private void GetAlphaVideo()
        {
            Bitmap result = AlphaVideo.Instance.ResultBitmap;
        }

        [TestMethod]
        public void TestQ60()
        {
            DateTime dateTime = DateTime.Now;
            var devices = _detector.GetUSBDevices();
            if (devices != null && devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    if (device.GetType() == typeof(Q60Device))
                    {
                        bool _timeron = true;
                        var fanGroups = ((Q60Device)device).GetFanGroups();
                        ((Q60Device)device).StartGettingDeviceData();
                        while (_timeron)
                        {
                            foreach (var group in fanGroups)
                            {
                                foreach (var fan in group.DeviceBases)
                                {
                                    if (fan.GetType() == typeof(Q60))
                                    {
                                        double noise = ((Q60)fan).Noise;
                                        double tempIn = ((Q60)fan).PumpTempIn;
                                        double tempOut = ((Q60)fan).PumpTempOut;
                                        double rpm = ((Q60)fan).CurrentRPM;
                                        Debug.WriteLine("Q60 Noise {0}, TempIn {1}, TempOut {2}, rpm {3}", noise, tempIn, tempOut, rpm);
                                    }

                                    if (fan.GetType() == typeof(FT12))
                                    {
                                        double tempIn = ((FT12)fan).Temperature;
                                        double RPM = ((FT12)fan).CurrentRPM;
                                        Debug.WriteLine("FT12 temp {0}, rpm {1}",tempIn, RPM);

                                        if ((DateTime.Now - dateTime).TotalMilliseconds > 2000)
                                        {
                                            ((FT12)fan).SetSpeed(100);
                                        }
                                    }
                                }
                            }
                            
                            if ((DateTime.Now - dateTime).TotalMilliseconds > 500000)
                            {
                                _timeron = false;
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }
        private const int DEVICE_XG270Q_2710QG_VID = 0x0543;
        private const int DEVICE_XG270Q_PID = 0xA003;
        private const int DEVICE_2710QG_PID = 0xA004;
        private const int FEATURE_LENGTH_2710QG = 190;
        private const int DEVICE_271_VID = 0x0416;
        private const int DEVICE_271_PID = 0x5020;
        private const int FEATURE_LENGTH_271_XG270Q = 64;
        [TestMethod]
        public void TestViewSonic()
        {
            var ddd =  DeviceList.Local.GetHidDevices(DEVICE_XG270Q_2710QG_VID, DEVICE_XG270Q_PID);
            var dddd = DeviceList.Local.GetHidDevices(DEVICE_271_VID, DEVICE_271_PID);
            var ddddd = DeviceList.Local.GetHidDevices(DEVICE_XG270Q_2710QG_VID, DEVICE_2710QG_PID);
            //var ddd = new HidDetector().GetHidStreams(DEVICE_XG270Q_2710QG_VID, DEVICE_XG270Q_PID, FEATURE_LENGTH_271_XG270Q);
            //var dddd = new HidDetector().GetHidStreams(DEVICE_271_VID, DEVICE_271_PID, FEATURE_LENGTH_271_XG270Q);
            //var ddddd = new HidDetector().GetHidStreams(DEVICE_XG270Q_2710QG_VID, DEVICE_2710QG_PID, FEATURE_LENGTH_2710QG);
        }
        //[TestMethod]
        //public void TestNanoleaf()
        //{
        //    NanoleafShapesController nanoleafShapesController = new NanoleafShapesController();
        //    var ddd = nanoleafShapesController.InitDevices();
        //}

        //[TestMethod]
        //public void TestNanoleafReset()
        //{
        //    _detector.ResetNanoleaf("192.168.0.91");
        //}

        //[TestMethod]
        //public void TestPhilipsHueBridge()
        //{
        //    StreamingGroup stream = Task.Run(() => SetupAndReturnGroup()).Result;
        //    var baseEntLayer = stream.GetNewLayer(isBaseLayer: true);
        //    baseEntLayer.AutoCalculateEffectUpdate(new CancellationToken());
        //    CancellationTokenSource cst = new CancellationTokenSource();
        //    baseEntLayer.SetState(cst.Token, new RGBColor("F00FFF"), 0.75);
        //    cst = WaitCancelAndNext(cst);
        //    string[] color = { "FF0000", "00FF00", "0000FF" };
        //    int i = 0;
        //    while (true)
        //    {
        //        baseEntLayer.SetState(cst.Token, new RGBColor(color[i % 3]), 0.2);
        //        cst = WaitCancelAndNext(cst);
        //        i++;
        //        Thread.Sleep(50);
        //    }

        //}

        //[TestMethod]
        //public void TestPhilipsHueBridgeReset()
        //{
        //    _detector.ResetPhilipsHueBridge("192.168.50.92");
        //}

        private async Task<StreamingGroup> SetupAndReturnGroup()
        {
            string ip = "192.168.50.236";
            string key = "zczfHYB0E3uPdNHeYY86S4yQwMSbLquraluqGdmd";
            string entertainmentKey = "09C420A4A449218017566B7B697CB9A0";
            var useSimulator = false;

            //Initialize streaming client
            StreamingHueClient client = new StreamingHueClient(ip, key, entertainmentKey);

            //Get the entertainment group
            var all = await client.LocalHueApi.GetEntertainmentConfigurationsAsync();
            var group = all.Data.LastOrDefault();

            Console.WriteLine($"Using Entertainment Group {group.Id}");

            //Create a streaming group
            var stream = new StreamingGroup(group.Channels);
            stream.IsForSimulator = useSimulator;


            //Connect to the streaming group
            await client.ConnectAsync(group.Id, simulator: useSimulator);

            //Start auto updating this entertainment group
            client.AutoUpdateAsync(stream, new CancellationToken(), 50, onlySendDirtyStates: false);

            //Optional: Check if streaming is currently active
            var entArea = await client.LocalHueApi.GetEntertainmentConfigurationAsync(group.Id);
            Console.WriteLine(entArea.Data.First().Status == HueApi.Models.EntertainmentConfigurationStatus.active ? "Streaming is active" : "Streaming is not active");
            return stream;
        }

        private static CancellationTokenSource WaitCancelAndNext(CancellationTokenSource cst)
        {
            cst.Cancel();
            cst = new CancellationTokenSource();
            return cst;
        }
    }
}
