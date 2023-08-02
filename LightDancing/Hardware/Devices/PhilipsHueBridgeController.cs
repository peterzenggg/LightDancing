using HueApi.ColorConverters;
using HueApi.Entertainment;
using HueApi.Entertainment.Models;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.Models.Philips;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HueApi.Entertainment.Extensions;
using HueApi.Models;

namespace LightDancing.Hardware.Devices
{
    public class PhilipsHueBridgeController : ILightingControl
    {
        public List<USBDeviceBase> InitDevices()
        {
            return InitDevicesRealDo();
        }

        public List<USBDeviceBase> InitDevicesRealDo(string iPAddress = "", bool isAskIPAddress = false)
        {
            try
            {
                Console.WriteLine($"PhilipsHueBridgeController/InitDevices is starting...");

                var philipsConfigs = new List<PhilipsConfig>();
                var appConfigManager = new AppConfigManager();
                AppConfig config = appConfigManager.LoadConfig();
                if (config.PhilipsConfigs != null)
                {
                    foreach (var philipsConfig in config.PhilipsConfigs)
                    {
                        Console.WriteLine($"PhilipsHueBridgeController/InitDevices find a config in file. Start to link... IP:{philipsConfig.IPAddress}, UserName:{philipsConfig.UserName}, PSK:{philipsConfig.PSK}");
                        var hueBridgeAPIHelper = new HueBridgeAPIHelper(philipsConfig.IPAddress, philipsConfig.UserName);
                        var hueBridgeAPIConfig = hueBridgeAPIHelper.GetConfig();
                        if (hueBridgeAPIConfig == null)
                        {
                            Console.WriteLine($"PhilipsHueBridgeController/InitDevices Link is faile! IP:{philipsConfig.IPAddress}, UserName:{philipsConfig.UserName}, PSK:{philipsConfig.PSK}");
                            continue;
                        }
                        philipsConfigs.Add(philipsConfig);
                        Console.WriteLine($"PhilipsHueBridgeController/InitDevices Link is success! IP:{philipsConfig.IPAddress}, UserName:{philipsConfig.UserName}, PSK:{philipsConfig.PSK}");
                    }
                }

                List<string> waitingCheckedIPs = new List<string>();

                try
                {
                    if (isAskIPAddress)
                    {
                        Console.WriteLine("PhilipsHueBridgeController/InitDevices Start to search IPAddress...");
                        var localDevices = new HttpClientManager("https://discovery.meethue.com/").GetAsync<List<HueBridgeDiscoveryResponse>>("");

                        if (localDevices.Any())
                        {
                            waitingCheckedIPs = localDevices.Select(x => x.InternalIPAddress).ToList();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"PhilipsHueBridgeController/InitDevices Search IPAddress is faile. Exception{e}");
                }

                if (!string.IsNullOrEmpty(iPAddress) && !waitingCheckedIPs.Contains(iPAddress))
                {
                    waitingCheckedIPs.Add(iPAddress);
                }

                Console.WriteLine($"PhilipsHueBridgeController/InitDevices Waiting checked IPs count:{waitingCheckedIPs.Count()}.");
                waitingCheckedIPs.Except(philipsConfigs.Select(x => x.IPAddress));
                Console.WriteLine($"PhilipsHueBridgeController/InitDevices Waiting checked IPs except config count:{waitingCheckedIPs.Count}.");

                foreach (var waitingCheckedIP in waitingCheckedIPs)
                {
                    Console.WriteLine($"PhilipsHueBridgeController/InitDevices Try link to PhilipsHueBridge... IP:{waitingCheckedIP}");
                    var hueBridgeAPIHelper = new HueBridgeAPIHelper(waitingCheckedIP);
                    var newUser = hueBridgeAPIHelper.GetNewUser();
                    if (newUser == null)
                    {
                        Console.WriteLine($"PhilipsHueBridgeController/InitDevices Link to PhilipsHueBridge is faile. IP:{waitingCheckedIP}");
                        continue;
                    }
                    Console.WriteLine($"PhilipsHueBridgeController/InitDevices Link to PhilipsHueBridge is success. IP:{waitingCheckedIP}, UserName:{newUser.UserName}, PSK:{newUser.ClientKey}");
                    philipsConfigs.Add(new PhilipsConfig() { IPAddress = waitingCheckedIP, UserName = newUser.UserName, PSK = newUser.ClientKey });
                }

                Console.WriteLine($"PhilipsHueBridgeController/InitDevices is success!. hardwares.Count:{philipsConfigs.Count}");
                if (philipsConfigs.Any())
                {
                    config.PhilipsConfigs = philipsConfigs;
                    appConfigManager.SaveConfig(config);
                    var uSBDevices = new List<USBDeviceBase>();
                    foreach (var philipsConfig in philipsConfigs)
                    {
                        uSBDevices.Add(new PhilipsHueBridgeDevice(philipsConfig));
                    }
                    return uSBDevices;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"PhilipsHueBridgeController/InitDevices is failed. Exception: {e}.");
            }

            return null;
        }
    }
    internal class HueBridgeAPIHelper
    {
        private readonly HttpClientManager _httpClientManager;
        private string _userName;

        internal HueBridgeAPIHelper(string iPAddress, string userName = "")
        {
            _httpClientManager = new HttpClientManager($"http://{iPAddress}/api/");
            _userName = userName;
        }

        internal UsernameAndClientKey GetNewUser()
        {
            UsernameAndClientKey result = null;
            try
            {
                var request = JsonConvert.SerializeObject(new HueBridgeNewUsesRequest() { DeviceType = "myNexuxDevice", GenerateClientKey = true });
                var apiResult = _httpClientManager.PostAsync<List<HueBridgeNewUserResponse>>("", request);
                if (apiResult.Any())
                {
                    var usernameAndClientKey = apiResult.FirstOrDefault().Success;
                    result = apiResult.FirstOrDefault().Success;
                    _userName = usernameAndClientKey.UserName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNewUser is faile:{ex}");
            }

            return result;
        }

        internal HueBridgeConfigResponse GetConfig()
        {
            HueBridgeConfigResponse result = null;
            try
            {
                result = _httpClientManager.GetAsync<HueBridgeConfigResponse>($"{_userName}/config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNewUser is faile:{ex}");
            }

            return result;
        }
    }

    public class PhilipsHueBridgeDevice : USBDeviceBase
    {
        readonly PhilipsConfig _philipsConfig;
        readonly StreamingHueClient _client;
        readonly StreamingGroup _stream;
        readonly EntertainmentConfiguration _group;
        EntertainmentLayer _lights;
        bool _isSteamIsLinking = false;
        bool _isSteamLink = false;
        static Timer _timer;
        static DateTime _lastCallTime;

        public PhilipsHueBridgeDevice(PhilipsConfig philipsConfig) : base()
        {
            _philipsConfig = philipsConfig;
            _client = new StreamingHueClient(_philipsConfig.IPAddress, _philipsConfig.UserName, _philipsConfig.PSK);
            var all = Task.Run(() => _client.LocalHueApi.GetEntertainmentConfigurationsAsync()).Result;
            _group = all.Data.LastOrDefault();
            _stream = new StreamingGroup(_group.Channels)
            {
                IsForSimulator = false
            };

            this._model.DeviceID = philipsConfig.IPAddress;
            this._lightingBase = InitDevice();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                //USBDeviceType = USBDevices.PhilipsHueBridge,
                Name = "Philips Hue Bridge"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            _lights = _stream.GetNewLayer(isBaseLayer: true);
            foreach (var light in _lights)
            {
                PhilipsHueBridgeLighting keyboard = new PhilipsHueBridgeLighting(1, 1, _model, light.Id.ToString());
                hardwares.Add(keyboard);
            }

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            try
            {
                if (_isSteamIsLinking)
                {
                    return;
                }

                if (!_isSteamLink)
                {
                    Console.WriteLine($"PhilipsHueBridgeDevice/SendToHardware steam is not yet link.");
                    StremLink();
                    _lastCallTime = DateTime.Now;
                    _timer = new Timer(CheckIfMethodCalled, null, 10000, 10000);
                    return;
                }

                _lastCallTime = DateTime.Now;
                CancellationTokenSource cst = new CancellationTokenSource();
                foreach (var lightingBase in _lightingBase)
                {
                    if (process)
                    {
                        lightingBase.ProcessStreaming(false, brightness);
                    }
                    var light = _lights.Where(light => light.Id.ToString() == lightingBase.GetModel().Name.Split(' ').LastOrDefault());

                    if (light.Any())
                    {
                        light.FirstOrDefault().SetState(cst.Token, new RGBColor(lightingBase.GetHttpClientColorData()), 1);
                        cst = WaitCancelAndNext(cst);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PhilipsHueBridgeDevice/SendToHardware is failed. Exception: {ex}.");
            }
        }

        private static CancellationTokenSource WaitCancelAndNext(CancellationTokenSource cst)
        {
            cst.Cancel();
            cst = new CancellationTokenSource();
            return cst;
        }

        private void StremLink()
        {
            Console.WriteLine($"PhilipsHueBridgeDevice/StremLink link steam is start...");
            _isSteamIsLinking = true;
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            var isNotStreamLinked = !IsStreamLinked();
            while (isNotStreamLinked)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    _isSteamIsLinking = false;
                    _isSteamLink = false;
                    Console.WriteLine($"PhilipsHueBridgeDevice/StremLink Link over time.（10 seconds.）.");
                    throw new TimeoutException("Link over time.（10 seconds.）");
                }

                try
                {
                    StreamStart();
                    isNotStreamLinked = !IsStreamLinked();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"PhilipsHueBridgeDevice/StremLink StreamStart link is faile. Exception:{e}.");
                }
            }
            Console.WriteLine($"PhilipsHueBridgeDevice/StremLink link steam is success...");
            _isSteamIsLinking = false;
            _isSteamLink = true;
        }

        private void StreamStart()
        {
            Console.WriteLine($"PhilipsHueBridgeDevice/StreamStart Link is faile. _group.Id:{_group.Id}.");
            Task.Run(() => _client.ConnectAsync(_group.Id, simulator: false)).Wait();

            Task.Run(() => _client.AutoUpdateAsync(_stream, new CancellationToken(), 50, onlySendDirtyStates: false));
        }

        private bool IsStreamLinked()
        {
            var entArea = Task.Run(() => _client.LocalHueApi.GetEntertainmentConfigurationAsync(_group.Id)).Result;

            return entArea.Data.First().Status == EntertainmentConfigurationStatus.active;
        }

        private void CheckIfMethodCalled(object state)
        {
            if ((DateTime.Now - _lastCallTime).TotalSeconds > 10)
            {
                _isSteamLink = false;
            }
        }
    }

    public class PhilipsHueBridgeLighting : LightingBase
    {
        private readonly int _yAxis;

        private readonly int _xAxis;

        public PhilipsHueBridgeLighting(int yAxis, int xAxis, HardwareModel hardwareModel, string lightID) : base(yAxis, xAxis, hardwareModel)
        {
            _yAxis = yAxis;
            _xAxis = xAxis;
            this._model.Name = $"Philips Hue Light {lightID}";
            this._model.DeviceID = $"{_usbModel.DeviceID} {lightID}";
            this._model.Layouts.Width = xAxis;
            this._model.Layouts.Height = _yAxis;
        }

        protected override LightingModel InitModel()
        {
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                //Type = LightingDevices.PhilipsHueBridge,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Philips Hue Light"
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            if (colorMatrix.Length < 1)
            {
                return;
            }

            _httpClientColorData = colorMatrix[0, 0].GetRGBString();
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
