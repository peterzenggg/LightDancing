using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightDancing.Hardware.Devices
{
    public class NanoleafShapesController: ILightingControl
    {
        public List<USBDeviceBase> InitDevices()
        {
            return InitDevicesRealDo();
        }

        public List<USBDeviceBase> InitDevicesRealDo(string ipAddress = "", bool isAskIPAddress = false)
        {
            try
            {
                Console.WriteLine($"NanoleafShapesController/InitDevices is starting...");

                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                List<NanoleafConfig> NanoleafConfigs = new List<NanoleafConfig>();

                var appConfigManager = new AppConfigManager();
                AppConfig config = appConfigManager.LoadConfig();

                if (config.NanoleafConfigs != null)
                {
                    foreach (var nonoleafConfig in config.NanoleafConfigs)
                    {
                        Console.WriteLine($"NanoleafShapesController/InitDevices find a config in file. Start to link... IP:{nonoleafConfig.IPAddress}, Token:{nonoleafConfig.AuthToken}");
                        var nanoleafShapesAPIHelper = new NanoleafShapesAPIHelper(nonoleafConfig.IPAddress, nonoleafConfig.AuthToken);
                        var layout = nanoleafShapesAPIHelper.GetLayout();
                        if (layout == null)
                        {
                            Console.WriteLine($"NanoleafShapesController/InitDevices Link is faile! IP:{nonoleafConfig.IPAddress}, Token:{nonoleafConfig.AuthToken}");
                            continue;
                        }

                        Console.WriteLine($"NanoleafShapesController/InitDevices Link is success! IP:{nonoleafConfig.IPAddress}, Token:{nonoleafConfig.AuthToken}");
                        hardwares.Add(new NanoleafShapesDevice(nonoleafConfig.IPAddress, nonoleafConfig.AuthToken));
                        NanoleafConfigs.Add(new NanoleafConfig() { IPAddress = nonoleafConfig.IPAddress, AuthToken = nonoleafConfig.AuthToken });
                    }
                }

                List<string> nanoleafDevicesIPs = new List<string>();
                if (isAskIPAddress)
                {
                    var nanoleafFinder = new MDNSFinder("_nanoleafapi._tcp.local.");
                    Console.WriteLine("NanoleafShapesController/InitDevices Start to use mDNS search Nanoleaf...");
                    var nanoleafFindResult = Task.Run(() => nanoleafFinder.FindDevicesAsync()).Result;
                    if (nanoleafFindResult != null)
                    {
                        nanoleafDevicesIPs = nanoleafFindResult.Select(x => x.IPAddress).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(ipAddress) && !nanoleafDevicesIPs.Contains(ipAddress))
                {
                    nanoleafDevicesIPs.Add(ipAddress);
                }
                Console.WriteLine($"NanoleafShapesController/InitDevices Device count:{nanoleafDevicesIPs.Count()}");

                if (nanoleafDevicesIPs.Count() > 0)
                {
                    var configIPs = NanoleafConfigs.Select(x => x.IPAddress);
                    foreach (var deviceIPs in nanoleafDevicesIPs)
                    {
                        Console.WriteLine($"NanoleafShapesController/InitDevices Try link to nanoleaf... IP:{deviceIPs}");
                        if (configIPs.Contains(deviceIPs))
                        {
                            Console.WriteLine($"NanoleafShapesController/InitDevices IP:{deviceIPs} is include in config file. Continue...");
                            continue;
                        }

                        var nanoleafShapesAPIHelper = new NanoleafShapesAPIHelper(deviceIPs);
                        var newUser = nanoleafShapesAPIHelper.GetNewUser();

                        if (newUser == null)
                        {
                            Console.WriteLine($"NanoleafShapesController/InitDevices GetNewUser is faile. IP:{deviceIPs}");
                            continue;
                        }

                        Console.WriteLine($"NanoleafShapesController/InitDevices Link to nanoleaf is success!. IP:{deviceIPs}, Token:{newUser.AuthToken}");
                        hardwares.Add(new NanoleafShapesDevice(deviceIPs, newUser.AuthToken));
                        NanoleafConfigs.Add(new NanoleafConfig() { IPAddress = deviceIPs, AuthToken = newUser.AuthToken });
                    }
                }

                Console.WriteLine($"NanoleafShapesController/InitDevices is success!. hardwares.Count:{hardwares.Count}");

                if (hardwares.Count > 0)
                {
                    config.NanoleafConfigs = NanoleafConfigs;
                    appConfigManager.SaveConfig(config);
                    return hardwares;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NanoleafShapesController/InitDevices is failed. Exception: {ex}.");
            }

            Console.WriteLine($"NanoleafShapesController/InitDevices Can't find any nanoleaf.");
            return null;
        }
    }

    internal class NanoleafShapesAPIHelper
    {
        private readonly HttpClientManager _httpClientManager;
        private string _authToken;

        internal NanoleafShapesAPIHelper(string iPAddress, string authToken = "")
        {
            _httpClientManager = new HttpClientManager($"http://{iPAddress}:16021/api/v1");
            _authToken = authToken;
        }

        internal NewUserResponse GetNewUser()
        {
            NewUserResponse result = null;
            try
            {
                result = _httpClientManager.PostAsync<NewUserResponse>("/new");
                _authToken = result.AuthToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNewUser is faile:{ex}");
            }

            return result;
        }

        internal PanelLayoutResponse GetLayout()
        {
            PanelLayoutResponse result = null;
            try
            {
                result = _httpClientManager.GetAsync<PanelLayoutResponse>($"/{_authToken}/panelLayout/layout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLayout is faile:{ex}");
            }

            return result;
        }

        internal void PutEffect(string animData)
        {
            try
            {
                var effect = new
                {
                    write = new
                    {
                        command = "display",
                        version = "1.0",
                        animType = "custom",
                        animData, // 每個動畫數據字段解釋：[Panel ID] [R] [G] [B] [亮度(0-100)] [持續時間(毫秒)] [延遲(毫秒)]
                        loop = false,
                        palette = new object[] { }
                    }
                };
                _httpClientManager.PutAsyncNotAwait($"/{_authToken}/effects", JsonConvert.SerializeObject(effect));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PutEffect is faile:{ex}");
            }
        }
    }

    public class NanoleafShapesDevice: USBDeviceBase
    {
        private readonly NanoleafShapesAPIHelper _nanoleafShapesAPIHelper;

        public NanoleafShapesDevice(string iPAddress, string authToken) : base()
        {
            _nanoleafShapesAPIHelper = new NanoleafShapesAPIHelper(iPAddress, authToken);
            this._model.DeviceID = iPAddress;
            this._lightingBase = InitDevice();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                //USBDeviceType = USBDevices.NanoleafShapes,
                Name = "Nanoleaf Shapes"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            var layout = _nanoleafShapesAPIHelper.GetLayout();
            (int MinX, int MaxX, int MinY, int MaxY) = CalculatePanelRange(layout);
            (int xMagnification, int xIndex) = Methods.ReduceToTwoDigits(MaxX);
            (int yMagnification, int yIndex) = Methods.ReduceToTwoDigits(MaxY);
            List<LightingBase> hardwares = new List<LightingBase>();
            NanoleafShapesLighting keyboard = new NanoleafShapesLighting(yIndex, xIndex, _model, layout, xMagnification, yMagnification);
            hardwares.Add(keyboard);

            return hardwares;
        }

        private (int MinX, int MaxX, int MinY, int MaxY) CalculatePanelRange(PanelLayoutResponse panelLayout)
        {
            List<int> xCoordinates = new List<int>();
            List<int> yCoordinates = new List<int>();

            foreach (Positiondata panel in panelLayout.PositionData)
            {
                xCoordinates.Add(panel.X);
                yCoordinates.Add(panel.Y);
            }

            int minX = xCoordinates.Min();
            int maxX = xCoordinates.Max();
            int minY = yCoordinates.Min();
            int maxY = yCoordinates.Max();

            return (minX, maxX, minY, maxY);
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (process)
            {
                foreach (var lightingBase in _lightingBase)
                {
                    lightingBase.ProcessStreaming(false, brightness);
                }
            }

            foreach (var lightingBase in _lightingBase)
            {
                _nanoleafShapesAPIHelper.PutEffect(lightingBase.GetHttpClientColorData());
            }
        }
    }

    public class NanoleafShapesLighting: LightingBase
    {
        private readonly int _yAxis;

        private readonly int _xAxis;

        private readonly int _xMagnification;

        private readonly int _yMagnification;

        private readonly PanelLayoutResponse _panelLayoutResponse;

        public NanoleafShapesLighting(int yAxis, int xAxis, HardwareModel hardwareModel, PanelLayoutResponse panelLayoutResponse, int xMagnification, int yMagnification) : base(yAxis, xAxis, hardwareModel)
        {
            _yAxis = yAxis;
            _xAxis = xAxis;
            _xMagnification = xMagnification;
            _yMagnification = yMagnification;
            _panelLayoutResponse = panelLayoutResponse;
            this._model.Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis };
        }

        protected override LightingModel InitModel()
        {
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                //Type = LightingDevices.NanoleafShapes,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Nanoleaf Shapes"
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            var sources = GetPanelColors(_panelLayoutResponse, colorMatrix);
            _httpClientColorData = sources.Count.ToString();
            foreach (KeyValuePair<int, ColorRGB> source in sources)
            {
                _httpClientColorData += $" {source.Key} 1 {source.Value.R} {source.Value.G} {source.Value.B} 0 0";
            }
        }

        public Dictionary<int, ColorRGB> GetPanelColors(PanelLayoutResponse panelLayout, ColorRGB[,] colorArray)
        {
            Dictionary<int, ColorRGB> panelColors = new Dictionary<int, ColorRGB>();

            foreach (Positiondata panel in panelLayout.PositionData.Where(x => x.PanelId != 0))
            {
                int x = AdjustIndex(panel.X / _xMagnification, _xAxis);
                int y = AdjustIndex(panel.Y / _yMagnification, _yAxis);
                ColorRGB panelColor = colorArray[y, x];
                panelColors.Add(panel.PanelId, panelColor);
            }

            return panelColors;
        }

        private int AdjustIndex(int source, int limit)
        {
            if (source >= limit)
                return limit - 1;
            return source;
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
}
