using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HidSharp;

namespace LightDancing.Hardware.Devices.UniversalDevice.Corsair.Hub
{
    public class CorsairLightingCommanderCoreController : ILightingControl
    {
        private const int DEVICE_VID = 0x1b1c;
        private const int DEVICE_PID = 0x0C32;
        private const int PUMP_DEVICE_PID = 0x0C39;

        public List<USBDeviceBase> InitDevices()
        {
            try
            {
                var deviceList = DeviceList.Local;
                var deviceInfo = deviceList.GetHidDevices(DEVICE_VID, DEVICE_PID);

                if (deviceInfo != null && deviceInfo.Any())
                {
                    var hubDeviceInfo = deviceInfo.Aggregate((max, x) => (max == null || x.MaxOutputReportLength > max.MaxOutputReportLength) ? x : max);

                    hubDeviceInfo.TryOpen(out HidStream stream);

                    if (stream != null)
                    {
                        var pumpDeviceInfo = deviceList.GetHidDevices(DEVICE_VID, PUMP_DEVICE_PID);
                        if (pumpDeviceInfo != null && pumpDeviceInfo.Any())
                        {
                            pumpDeviceInfo.FirstOrDefault().TryOpen(out HidStream pumpStream);
                            LightingCommanderCoreConfig.IS_CONNECT_PUMP = pumpStream != null;
                        }

                        return new List<USBDeviceBase>() { new CorsairLightingCommanderCoreDevice(stream) };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CorsairLightingCommanderCoreController/InitDevices is failed. Exception: {ex}.");
            }

            return null;
        }
    }

    public class CorsairLightingCommanderCoreDevice : USBDeviceBase
    {
        private bool _isSteamIsLinking = false;
        private bool _isSteamLink = false;
        private DelayedTask _delayedTask;
        private const int HUB_OUTPUT_COUNT = 7; // There are one output of pump and six outputs of fans
        private readonly LightingCommanderCoreHandleHandler _handleHandler;
        private readonly LightingCommanderCoreFanTypeHandler _fanTypeHandler;
        private readonly LightingCommanderCoreLightHandler _lightHandler;

        public CorsairLightingCommanderCoreDevice(DeviceStream deviceStream) : base(deviceStream)
        {
            _handleHandler = new LightingCommanderCoreHandleHandler((HidStream)_deviceStream);
            _fanTypeHandler = new LightingCommanderCoreFanTypeHandler((HidStream)_deviceStream);
            _lightHandler = new LightingCommanderCoreLightHandler((HidStream)_deviceStream);
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                //USBDeviceType = USBDevices.CorsairLightingCommanderCore,
                Name = "Corsair Lighting Commander Core"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();

            for (int i = 0; i < HUB_OUTPUT_COUNT; i++)
            {
                CorsairLightingCommanderCoreLighting keyboard = new CorsairLightingCommanderCoreLighting(1, 1, _model, i.ToString());
                hardwares.Add(keyboard);
            }

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            ThrottledAction throttledAction = new ThrottledAction(64);

            throttledAction.Invoke(() =>
            {
                if (_isSteamIsLinking)
                {
                    return;
                }

                if (!_isSteamLink)
                {
                    StremLink();
                    _delayedTask = new DelayedTask(() =>
                    {
                        _isSteamLink = false;
                    }, TimeSpan.FromSeconds(30));
                    return;
                }

                try
                {
                    var targetBytes = new List<byte>();

                    //sendRGB
                    for (int i = 0; i < HUB_OUTPUT_COUNT; i++)
                    {
                        if (process)
                        {
                            _lightingBase[i].ProcessStreaming(false, brightness);
                        }

                        if (i == 0)
                        {
                            targetBytes.AddRange(LightingCommanderCoreLightHandler.GetPumpLedData(_lightingBase[i].GetHttpClientColorData()));
                            continue;
                        }

                        targetBytes.AddRange(LightingCommanderCoreLightHandler.GetFanLedData(_lightingBase[i].GetHttpClientColorData()));
                    }

                    _lightHandler.SendRGBData(targetBytes.ToArray());
                }
                catch
                {
                }

                _delayedTask.Restart();
            });
        }

        private void StremLink()
        {
            _isSteamIsLinking = true;

            _handleHandler.CloseAllHandle();
            _fanTypeHandler.SetProperty();
            _handleHandler.OpenHandle(Handles.Lighting, Endpoints.LightingController);
            Thread.Sleep(200);
            _handleHandler.OpenHandle(Handles.Background, Endpoints.LedCount_4Pin);
            _fanTypeHandler.SetFanType();
            _handleHandler.CloseHandle(Handles.Background);

            _isSteamIsLinking = false;
            _isSteamLink = true;
        }
    }

    public class CorsairLightingCommanderCoreLighting : LightingBase
    {
        private readonly int _yAxis;

        private readonly int _xAxis;

        public CorsairLightingCommanderCoreLighting(int yAxis, int xAxis, HardwareModel hardwareModel, string lightID) : base(yAxis, xAxis, hardwareModel)
        {
            _yAxis = yAxis;
            _xAxis = xAxis;
            this._model.Name = $"Corsair_Lighting_Commander_Core_Output_{lightID}";
            this._model.DeviceID = $"{_usbModel.DeviceID}_{lightID}";
            this._model.Layouts.Width = _xAxis;
            this._model.Layouts.Height = _yAxis;
        }

        protected override LightingModel InitModel()
        {
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                //Type = LightingDevices.CorsairLightingCommanderCore,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Corsair Lighting Commander Core"
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            if (colorMatrix.Length < 1)
            {
                return;
            }

            _httpClientColorData = colorMatrix[0, 0].GetRGBStringStartWith0x();
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }

        protected override void SetKeyLayout()
        {
        }

        protected override void TurnOffLed()
        {
        }
    }

    internal static class LightingCommanderCoreConfig
    {
        public const byte CONNECTION_TYPE = 8;
        public const int FAN_TYPE_SQL = 6;
        public static bool IS_CONNECT_PUMP = false;
    }

    internal class LightingCommanderCoreHandleHandler
    {
        private static HidStream _stream;

        public LightingCommanderCoreHandleHandler(HidStream stream)
        {
            _stream = stream;
        }

        public void CloseAllHandle()
        {
            foreach (Handles handle in Enum.GetValues(typeof(Handles)))
            {
                if (IsHandleOpen(handle))
                {
                    CloseHandle(handle);
                }
            }
        }

        public bool IsHandleOpen(Handles handle)
        {
            byte[] packet = new byte[] { 0x00, LightingCommanderCoreConfig.CONNECTION_TYPE, (byte)CommandIds.CheckHandle, (byte)handle, 0x00 };

            _stream.Write(packet.ToArray());
            packet = _stream.Read();

            bool isOpen = packet[3] != 3;
            return isOpen;
        }

        public void CloseHandle(Handles handle)
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.CloseHandle,
                1,
                (byte)handle
            };

            _stream.Write(packet.ToArray());
        }

        public void OpenHandle(Handles handle, Endpoints endpoint)
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.OpenEndpoint,
                (byte)handle,
                (byte)endpoint
            };

            _stream.Write(packet.ToArray());
        }
    }

    internal class LightingCommanderCoreLightHandler
    {
        private static HidStream _stream;
        private static int _ledByteCount;

        public LightingCommanderCoreLightHandler(HidStream stream)
        {
            _stream = stream;
            _ledByteCount = FindBufferLength();
        }

        public static byte[] GetPumpLedData(string color)
        {
            if (!LightingCommanderCoreConfig.IS_CONNECT_PUMP)
            {
                return new byte[0];
            }

            List<byte> RGBData = new List<byte>();

            for (int iIdx = 0; iIdx < EliteLCDCooler.Mapping.Count; iIdx++)
            {
                List<byte> mxPxColor;

                //find colors
                mxPxColor = HexToRgb(color);

                //set colors
                RGBData.Add(mxPxColor[0]);
                RGBData.Add(mxPxColor[1]);
                RGBData.Add(mxPxColor[2]);
            }

            return RGBData.ToArray();
        }

        public static byte[] GetFanLedData(string color)
        {
            List<byte> RGBData = new List<byte>();

            for (int iIdx = 0; iIdx < 34; iIdx++)
            {
                List<byte> mxPxColor;

                //find colors
                mxPxColor = HexToRgb(color);

                //set colors
                RGBData.Add(mxPxColor[0]);
                RGBData.Add(mxPxColor[1]);
                RGBData.Add(mxPxColor[2]);
            }

            return RGBData.ToArray();
        }

        private static List<byte> HexToRgb(string hex)
        {
            hex = hex.Replace("0x", "");
            var rgb = new List<byte>();

            for (int i = 0; i < 3; i++)
            {
                rgb.Add(byte.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber));
            }

            return rgb;
        }

        public void SendRGBData(byte[] rgbData)
        {
            const int initialHeaderSize = 8;
            const int headerSize = 4;

            rgbData = new byte[] { 0x12, 0x00 }.Concat(rgbData).ToArray();

            int totalBytes = rgbData.Length;
            int initialPacketSize = _ledByteCount - initialHeaderSize;

            WriteLighting(rgbData.Length, Splice(ref rgbData, initialPacketSize));

            totalBytes -= initialPacketSize;

            while (totalBytes > 0)
            {
                int bytesToSend = Math.Min(_ledByteCount - headerSize, totalBytes);
                StreamLighting(Splice(ref rgbData, bytesToSend));

                totalBytes -= bytesToSend;
            }
        }

        private byte[] Splice(ref byte[] list, int count)
        {
            var taken = list.Take(count).ToArray();
            list = list.Skip(count).ToArray();
            return taken;
        }

        private int FindBufferLength()
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.PingDevice,
            };

            _stream.Write(packet.ToArray());
            var result = _stream.Read();
            return result.Length;
        }

        private void WriteLighting(int ledCount, byte[] rgbData)
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.WriteEndpoint,
                0x00,
                (byte)(ledCount & 0xFF),
                (byte)(ledCount >> 8),
                0x00,
                0x00
            };
            packet.AddRange(rgbData);

            _stream.Write(packet.ToArray());
        }

        private void StreamLighting(byte[] rgbData)
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.StreamEndpoint,
                0x00
            };
            packet.AddRange(rgbData);

            _stream.Write(packet.ToArray());
        }
    }

    internal class LightingCommanderCoreFanTypeHandler
    {
        static HidStream _stream;

        public LightingCommanderCoreFanTypeHandler(HidStream stream)
        {
            _stream = stream;
        }

        public void SetProperty()
        {
            var packet = new List<byte>
            {
                0x00,
                LightingCommanderCoreConfig.CONNECTION_TYPE,
                (byte)CommandIds.SetProperty,
                (byte)PropertyNames.Mode,
                0x00,
                (byte)(2 & 0xFF),
                (byte)((2 >> 8) & 0xFF),
                (byte)((2 >> 16) & 0xFF)
            };

            _stream.Write(packet.ToArray());
        }

        public void SetFanType()
        {
            // Configure Fan Ports to use QL Fan size grouping. 34 Leds
            List<byte> fanSettings = Enumerable.Repeat((byte)0, 25).ToList();
            fanSettings[0] = 0x00;
            fanSettings[1] = LightingCommanderCoreConfig.CONNECTION_TYPE;
            fanSettings[2] = 0x06;
            fanSettings[3] = 0x01;
            fanSettings[4] = 0x11;
            fanSettings[8] = 0x0D;
            fanSettings[10] = 0x07;
            int offset = 11;

            for (int iIdx = 0; iIdx < 7; iIdx++)
            {
                fanSettings[offset + iIdx * 2] = 0x01;
                fanSettings[offset + iIdx * 2 + 1] = (byte)(iIdx == 0 ? 0x01 : LightingCommanderCoreConfig.FAN_TYPE_SQL); // 1 for nothing, 0x08 for pump?
            }

            _stream.Write(fanSettings.ToArray());
        }
    }

    internal static class EliteLCDCooler
    {
        public static List<List<int>> Positioning { get; } = new List<List<int>>
        {
            new List<int>{6, 0},
            new List<int>{5, 1},
            new List<int>{7, 1},
            new List<int>{4, 2},
            new List<int>{8, 2},
            new List<int>{3, 3},
            new List<int>{9, 3},
            new List<int>{2, 4},
            new List<int>{10, 4},
            new List<int>{1, 5},
            new List<int>{11, 5},
            new List<int>{0, 6},
            new List<int>{12, 6},
            new List<int>{1, 7},
            new List<int>{11, 7},
            new List<int>{2, 8},
            new List<int>{10, 8},
            new List<int>{4, 10},
            new List<int>{8, 10},
            new List<int>{3, 9},
            new List<int>{9, 9},
            new List<int>{5, 11},
            new List<int>{7, 11},
            new List<int>{6, 12}
        };

        public static List<int> Mapping { get; } = new List<int>
        {
            6,
            5, 7,
            4, 8,
            3, 9,
            2, 10,
            1, 11,
            0, 12,
            23, 13,
            22, 14,
            21, 15,
            20, 16,
            19, 17,
            18
        };

        public static List<string> LedNames { get; } = new List<string>
        {
            "Led 1", "Led 2", "Led 3", "Led 4", "Led 5", "Led 6", "Led 7", "Led 8", "Led 9", "Led 10", "Led 11", "Led 12", "Led 13", "Led 14", "Led 15", "Led 16",
            "Led 17", "Led 18", "Led 19", "Led 20", "Led 21", "Led 22", "Led 23", "Led 24"
        };

        public static string DisplayName { get; } = "Elite LCD Cooler";
        public static int LedCount { get; } = 24;
        public static int Width { get; } = 13;
        public static int Height { get; } = 13;
    }
}
