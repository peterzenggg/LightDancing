using LightDancing.Colors;
using LightDancing.Enums;
using LightDancing.Hardware.Devices;
using LightDancing.OpenTKs;
using LightDancing.Syncs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightDancing.Hardware.Devices.UniversalDevice.Mountain;
using LightDancing.Hardware.Devices.UniversalDevice.AsRock.MotherBoard;
using System.Diagnostics;
using Windows.Devices.Usb;
using LightDancing.Hardware.Devices.Fans;
using System.Runtime.CompilerServices;
using System.Collections;


namespace LightDancing.Hardware
{
    public class HardwaresDetector : IDisposable
    {
        private static readonly Lazy<HardwaresDetector> _lazy = new Lazy<HardwaresDetector>(() => new HardwaresDetector());

        private List<LightingBase> _lightingDevices = null;
        private List<USBDeviceBase> _usbDevices = null;
        private readonly List<string> _previousDevices = new List<string>();
        private readonly List<USBDeviceBase> _updateDevice = new List<USBDeviceBase>();
        private List<DFUDevice> _dfuDevices = new List<DFUDevice>();
        private List<FanGroup> _fanGroups = new List<FanGroup>();
        private List<FanBase> _fanDevices = new List<FanBase>();
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private readonly object _locker = new object();
        private object _usblist_locker = new object();
        private bool _isProcessing = false;
        private bool _isMirror = false;
        private readonly Dictionary<string, Tuple<List<List<Keyboard>>, Dictionary<Keyboard, ColorRGB>>> keyColorSet = new Dictionary<string, Tuple<List<List<Keyboard>>, Dictionary<Keyboard, ColorRGB>>>();
        private Action rgbPreview_updateHandler;
        public event EventHandler<string> Changed;
        public event EventHandler<string> DFUChanged;
        private float _brightness = 1f;
        private readonly UsbHotSwap usbHotSwap;
        /*mini hub parameters*/
        private int _port1Fans = 1;
        private int _port2Fans = 3;
        private int _port1Leds = 16; 
        private int _port2Leds = 0;
        private int _port1FanSpeed = 20;
        private int _port2FanSpeed = 20;
        public bool STM32_Setting { get; set; }
        public List<USBDevices> ControllerList { get; set; }
        private readonly List<USBDevices> _didntHotSwapUsbDevice = new List<USBDevices>() {
            //USBDevices.ASUSZ790A,
            //USBDevices.NanoleafShapes,
            //USBDevices.PhilipsHueBridge,
            USBDevices.Q60
        };

        private HardwaresDetector()
        {
            if (System.IO.Directory.Exists(STM32Directory.STM32_DIRECTORY_PATH))
            {
                STM32_Setting = System.IO.File.Exists(STM32Directory.CLI_PATH);

                if (STM32Directory.DEFAULT_BIN_PATH != null && STM32Directory.DEFAULT_BIN_PATH.Count > 0)
                {
                    foreach (var filepath in STM32Directory.DEFAULT_BIN_PATH)
                    {
                        STM32_Setting = STM32_Setting && System.IO.File.Exists(filepath.Value);
                    }
                }
                else
                {
                    STM32_Setting = false;
                }
            }
            else
            {
                STM32_Setting = false;
            }

            /*Init the ControllerList*/
            lock (_usblist_locker)
            {
                ControllerList = new List<USBDevices>();
                foreach (USBDevices device in Enum.GetValues(typeof(USBDevices)))
                {
                    ControllerList.Add(device);
                }
            }

            InitLightingDevices();
            _dfuDevices = DfuDetector.GetDfuData(_updateDevice);

            if (_lightingDevices != null)
            {
                foreach (var item in _lightingDevices)
                {
                    _previousDevices.Add(item.GetModel().DeviceID);
                }
            }
            usbHotSwap = new UsbHotSwap(DeviceList_Changed, DFUList_Changed, ControllerList);
        }

        protected virtual void DeviceChanged()
        {
            Changed?.Invoke(this, null);
        }

        private void DeviceList_Changed()
        {
            StopStreaming();

            if (_usbDevices != null)
            {
                foreach (USBDeviceBase stream in _usbDevices)
                {
                    if (!_didntHotSwapUsbDevice.Contains(stream.GetModel().USBDeviceType))
                    {
                        stream.Dispose();
                    }
                    //if(stream.GetType() == typeof(Q60Device))
                    //{
                    //    ((Q60Device)stream).StopQ60Hotswap();
                    //}
                }
                _usbDevices = _usbDevices.Where(x => _didntHotSwapUsbDevice.Contains(x.GetModel().USBDeviceType)).ToList();
            }

            if (_lightingDevices != null)
            {
                _lightingDevices = _lightingDevices.Where(x => _didntHotSwapUsbDevice.Contains(x.GetModel().USBDeviceType)).ToList();
            }

            InitLightingDevices(true);

            _previousDevices.Clear();

            if (_lightingDevices != null)
            {
                foreach (var device in _lightingDevices)
                {
                    _previousDevices.Add(device.GetModel().DeviceID);
                }

                DeviceChanged();
            }
        }

        private void DFUList_Changed()
        {
            _dfuDevices = DfuDetector.GetDfuData(_updateDevice);

            DFUChanged?.Invoke(this, null);
        }

        public static HardwaresDetector Instance => _lazy.Value;

        /// <summary>
        /// Init all iBUYPOWER _hardwares
        /// </summary>
        private void InitLightingDevices(bool isHotSwap = false)
        {
            lock (_usblist_locker)
            {
                LightingControlFactory factory = new LightingControlFactory();

                foreach (USBDevices device in ControllerList)
                {
                    if (isHotSwap && _didntHotSwapUsbDevice.Contains(device))
                    {
                        continue;
                    }

                    ILightingControl control = factory.GetControl(device);

                    if (control != null)
                    {
                        List<USBDeviceBase> devices;

                        if (control.GetType() == typeof(IBPMiniHubController))
                        {
                            devices = ((IBPMiniHubController)control).InitDevices(_port1Fans, _port1Leds, _port2Fans, _port2Leds, _port1FanSpeed, _port2FanSpeed);
                        }
                        else
                        {
                            devices = control.InitDevices();
                        }

                        if (devices != null)
                        {
                            if (_lightingDevices == null)
                            {
                                _lightingDevices = new List<LightingBase>();
                            }

                            if (_usbDevices == null)
                            {
                                _usbDevices = new List<USBDeviceBase>();
                            }

                            if(_fanGroups == null)
                            {
                                _fanGroups = new List<FanGroup>();
                            }
                            //_fanGroups.Clear();
                            _usbDevices.AddRange(devices);

                            foreach (var item in devices)
                            {
                                /*Mini hub layout setting*/
                                if (item.GetType() == typeof(IBPMiniHubDevice))
                                {
                                    ((IBPMiniHubDevice)item).LayoutChanged += DeviceList_Changed;
                                }
                                List<LightingBase> lightingBases = item.GetLightingDevice();
                                _lightingDevices.AddRange(lightingBases);
                            }
                        }
                    }
                }

                _fanGroups.Clear();
                foreach (var item in _usbDevices)
                {
                    if (item.GetType() == typeof(Q60Device))
                    {
                        _fanGroups.AddRange(((Q60Device)item).GetFanGroups());
                    }

                    if (item.GetType() == typeof(NP50Device))
                    {
                        _fanGroups.AddRange(((NP50Device)item).GetFanGroups());
                    }

                    if (item.GetType() == typeof(AsRockMotherBoard))
                    {
                        _fanGroups.AddRange(((AsRockMotherBoard)item).GetFanGroups());
                    }
                }

                _fanDevices.Clear();
                foreach (var fanGroup in _fanGroups)
                {
                    foreach(var fan in fanGroup.DeviceBases)
                    {
                        _fanDevices.Add((FanBase)fan);
                    }
                }
            }
        }

        /// <summary>
        /// Get fan group for system widget
        /// </summary>
        /// <returns></returns>
        public List<FanGroup> GetFanGroups()
        {
            return _fanGroups;
        }

        /// <summary>
        /// Get fan devices for cooling widget
        /// </summary>
        /// <returns></returns>
        public List<FanBase> GetFanDevices()
        {
            return _fanDevices;
        }

        //public bool ResetNanoleaf(string iPAddress = "")
        //{
        //    try
        //    {
        //        Console.WriteLine($"HardwaresDetector/ResetNanoleaf is starting...");
        //        _usbDevices = _usbDevices.Where(x => x.GetModel().USBDeviceType != USBDevices.NanoleafShapes).ToList();
        //        _lightingDevices = _lightingDevices.Where(x => x.GetModel().Type != LightingDevices.NanoleafShapes).ToList();

        //        var usbDevices = new NanoleafShapesController().InitDevicesRealDo(iPAddress, true);

        //        if (usbDevices == null || !usbDevices.Any())
        //        {
        //            Console.WriteLine($"HardwaresDetector/ResetNanoleaf usbDevices is null and empty.");
        //            return false;
        //        }

        //        _usbDevices.AddRange(usbDevices);

        //        foreach (var device in usbDevices)
        //        {
        //            _lightingDevices.AddRange(device.GetLightingDevice());
        //        }

        //        DeviceChanged();
        //        Console.WriteLine($"HardwaresDetector/ResetNanoleaf is success.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"HardwaresDetector/ResetNanoleaf is failed. Exception: {ex}.");
        //        return false;
        //    }
        //}

        //public bool ResetPhilipsHueBridge(string iPAddress = "")
        //{
        //    try
        //    {
        //        Console.WriteLine($"HardwaresDetector/ResetPhilipsHueBridge is starting...");
        //        _usbDevices = _usbDevices.Where(x => x.GetModel().USBDeviceType != USBDevices.PhilipsHueBridge).ToList();
        //        _lightingDevices = _lightingDevices.Where(x => x.GetModel().Type != LightingDevices.PhilipsHueBridge).ToList();

        //        var usbDevices = new PhilipsHueBridgeController().InitDevicesRealDo(iPAddress, true);

        //        if (usbDevices == null || !usbDevices.Any())
        //        {
        //            Console.WriteLine($"HardwaresDetector/ResetPhilipsHueBridge usbDevices is null and empty.");
        //            return false;
        //        }

        //        _usbDevices.AddRange(usbDevices);

        //        foreach (var device in usbDevices)
        //        {
        //            _lightingDevices.AddRange(device.GetLightingDevice());
        //        }

        //        DeviceChanged();
        //        Console.WriteLine($"HardwaresDetector/ResetPhilipsHueBridge is success.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"HardwaresDetector/ResetPhilipsHueBridge is failed. Exception: {ex}.");
        //        return false;
        //    }
        //}

        /// <summary>
        /// List current connecting of devices
        /// </summary>
        /// <returns>Devices</returns>
        public List<LightingBase> GetLightingDevices()
        {
            return _lightingDevices;
        }

        /// <summary>
        /// Get USB Device List
        /// </summary>
        /// <returns>USB Devices</returns>
        public List<USBDeviceBase> GetUSBDevices()
        {
            return _usbDevices;
        }

        /// <summary>
        /// Set streaming of animation with AnimationCoding
        /// </summary>
        /// <param name="type">AnimationCoding</param>
        public void SetStreaming(LightingBase hardware, AnimationCoding type)
        {
            LightingBase device = _lightingDevices.FirstOrDefault(x => x == hardware);
            device.SetStreaming(type);
        }

        /// <summary>
        /// Set streaming for streaming syncs
        /// </summary>
        /// <param name="hardware">device</param>
        /// <param name="syncHelper">Sync</param>
        /// <param name="captureInfos">Capture Area</param>
        public void SetStreaming(LightingBase hardware, ISyncHelper syncHelper, CaptureInfos captureInfos)
        {
            LightingBase device = _lightingDevices.FirstOrDefault(x => x == hardware);
            device.SetStreaming(syncHelper, captureInfos);
        }

        public void SetLedTurnOn(LightingBase hardware, bool isTurnOn)
        {
            LightingBase device = _lightingDevices.FirstOrDefault(x => x == hardware);
            device.SetLedTurnOn(isTurnOn);
        }

        public void SetFwAnimationON()
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                foreach (var device in _usbDevices)
                {
                    device.TurnFwAnimationOn();
                }
            }
        }

        /// <summary>
        /// Chnage Mountain Everest Keeboard Side 
        /// </summary>
        /// <param name="keeb"></param>
        public void ChangeMountainSide(MountainEverestKeeb keeb)
        {
            int index = _lightingDevices.FindIndex(x => x.GetModel().Name == keeb.GetModel().Name);
            if (index != -1)
            {
                _lightingDevices[index] = keeb;
            }
            DeviceChanged();
        }

        public void SmartDeviceHotswap()
        {
            _lightingDevices = new List<LightingBase>();
            _fanGroups.Clear();
            foreach (USBDeviceBase usbDevice in _usbDevices)
            {
                _lightingDevices.AddRange(usbDevice.GetLightingDevice());

                if (usbDevice.GetType() == typeof(Q60Device))
                {
                    _fanGroups.AddRange(((Q60Device)usbDevice).GetFanGroups());
                }
                if (usbDevice.GetType() == typeof(NP50Device))
                {
                    _fanGroups.AddRange(((NP50Device)usbDevice).GetFanGroups());
                }
            }

            _fanDevices.Clear();
            foreach (var fanGroup in _fanGroups)
            {
                foreach (var fan in fanGroup.DeviceBases)
                {
                    _fanDevices.Add((FanBase)fan);
                }
            }
            DeviceChanged();
        }

        /// <summary>
        /// Process streaming with all devices
        /// </summary>
        public Task ProcessStreaming()
        {
            CancellationToken cancelToken = cancelSource.Token;

            return Task.Run(() =>
            {
                if (_lightingDevices != null && _lightingDevices.Count > 0)
                {
                    _isProcessing = false;
                    lock (_locker)
                    {
                        _isProcessing = true;

                        while (_isProcessing)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                _isProcessing = false;
                                return;
                            }

                            keyColorSet.Clear();

                            foreach (var usbDevice in _usbDevices)
                            {
                                usbDevice.ProcessStreaming(_brightness);
                            }

                            rgbPreview_updateHandler?.Invoke();
                            Thread.Sleep(15);
                        }
                    }
                }
            }, cancelToken);
        }

        /// <summary>
        /// Stop streaming
        /// Add scroll wheels for DROPALTKeyboard, SolidyearKeyboard
        /// </summary>
        public void StopStreaming()
        {
            cancelSource.Cancel();

            lock (_locker)
            {
                cancelSource = new CancellationTokenSource();
            }
        }

        public void ProcessCommands(ColorRGB[,] colorMatrix)
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                foreach (var usbDevice in _usbDevices)
                {
                    usbDevice.ProcessStaticColors(colorMatrix, _brightness);
                }
            }
        }

        /// <summary>
        /// Manual set color for each key
        /// </summary>
        /// <param name="keyColors">Dictionary for each key and its color</param>
        public void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
            if (_lightingDevices != null && _lightingDevices.Count > 0)
            {
                foreach (var device in _lightingDevices)
                {
                    device.ProcessZoneSelect(keyColors);
                }
            }
        }

        public void Dispose()
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                foreach (var device in _usbDevices)
                {
                    device.Dispose();
                }
                _usbDevices.Clear();
                _lightingDevices.Clear();
            }
        }

        public void SetMirror(bool isMirror)
        {
            _isMirror = isMirror;
        }

        /// <summary>
        /// UI call DLL, register update callback
        /// </summary>
        public void RegisterScrollWheelCallback(Action func)
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                foreach (var device in _usbDevices)
                {
                    if (device.GetType() == typeof(DROPALTKeyboardDevice))
                    {
                        ((DROPALTKeyboardDevice)(device)).RegisterUpdateCallback(func);
                    }

                    if (device.GetType() == typeof(HotKeyboardDevice))
                    {
                        ((HotKeyboardDevice)(device)).RegisterUpdateCallback(func);
                    }
                }
            }
        }

        /// <summary>
        /// call enum ScrollWheelsMode
        /// </summary>
        public ScrollWheelsMode GetScrollWheel()
        {
            ScrollWheelsMode scrollWheel = ScrollWheelsMode.NA;

            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                foreach (var device in _usbDevices)
                {
                    if (device.GetType() == typeof(DROPALTKeyboard))
                    {
                        if (((DROPALTKeyboardDevice)(device))._isScrollWheelInvoke)
                        {
                            scrollWheel = ((DROPALTKeyboardDevice)(device)).GetDeviceScrollWheels();
                        }
                    }

                    if (device.GetType() == typeof(HotKeyboardDevice))
                    {
                        if (((HotKeyboardDevice)(device))._isScrollWheelInvoke)
                        {
                            scrollWheel = ((HotKeyboardDevice)(device)).GetDeviceScrollWheels();
                        }
                    }
                }
            }
            return scrollWheel;
        }

        /// <summary>
        /// Callback function updates front end when keyboard color is changed
        /// </summary>
        /// <param name="func"></param>
        public void RegisterKeyColorCallback(Action func)
        {
            rgbPreview_updateHandler = func;
        }

        /// <summary>
        /// Return a dictionay which contains the deviceID
        /// The Item1 is Keyboard layout
        /// The Item2 is the keyboard enum with its color
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Tuple<List<List<Keyboard>>, Dictionary<Keyboard, ColorRGB>>> GetKeyColor()
        {
            return keyColorSet;
        }

        public void SetBrightnessConfig(float brightness)
        {
            _brightness = brightness;
        }

        /// <summary>
        /// Add update requested device to update list
        /// The task run is to prevent the device is unplugged as soon as update requested
        /// </summary>
        /// <param name="lightingBase"></param>
        public void AddUpdateDevice(USBDeviceBase usbDeviceBase)
        {
            if (usbDeviceBase != null)
            {
                _updateDevice.Add(usbDeviceBase);

                /*After 3 seconds if the device is not added to the list, we can just remove it from the update list*/
                //Task.Run(() =>
                //{
                //    Thread.Sleep(3000);
                //    int dfuIndex = _dfuDevices.FindIndex(x => x.SerialID == usbDeviceBase.GetModel().DeviceID);

                //    if (dfuIndex < 0)
                //    {
                //        int updateDeviceIndex = _updateDevice.FindIndex(x => x.GetModel().DeviceID == usbDeviceBase.GetModel().DeviceID);
                //        if(updateDeviceIndex >= 0)
                //            _updateDevice.RemoveAt(updateDeviceIndex);
                //        DeviceChanged();
                //    }
                //});
            }
        }

        /// <summary>
        /// _isUpdated = error means device is plugged out, so we need to do the usb hotswap again after remove the device
        /// </summary>
        /// <param name="serialID"></param>
        /// <param name="_isUpdated"></param>
        public void RemoveUpdateDevice(string serialID, bool _isUpdated)
        {
            for (int i = 0; i < _updateDevice.Count; i++)
            {
                if (_updateDevice[i].GetModel().DeviceID == serialID)
                {
                    _updateDevice.RemoveAt(i);
                    break;
                }
            }

            if (!_isUpdated)
            {
                DeviceChanged();
                DFUList_Changed();
            }
        }

        public List<DFUDevice> GetDFUDevices()
        {
            return _dfuDevices;
        }

        public List<USBDeviceBase> GetUpdatingList()
        {
            return _updateDevice;
        }

        /// <summary>
        /// Set up minihub layout
        /// </summary>
        /// <param name="port1Fans">Set port1 Fans</param>
        /// <param name="port2Fans">Set port2 Fans Value</param>
        /// <param name="port1Leds">Set port3 Leds Value</param>
        /// <param name="port2Leds">Set port4 Leds Leds</param>
        /// <returns></returns>
        public string ChangeMiniHubLayout(int port1Fans, int port2Fans, int port1Leds, int port2Leds)
        {
            if (port1Leds + port2Leds > 100)
            {
                return "port1Leds + port2Leds max is 100";
            }
            else if (port1Leds > 50)
            {
                return "port1Leds max is 50";
            }
            else
            {
                _port1Fans = port1Fans;
                _port1Leds = port1Leds;
                _port2Fans = port2Fans;
                _port2Leds = port2Leds;
                if (_usbDevices != null && _usbDevices.Count > 0)
                {
                    int index = _usbDevices.FindIndex(x => x.GetType() == typeof(IBPMiniHubDevice));
                    if (index >= 0)
                    {
                        ((IBPMiniHubDevice)_usbDevices[index]).ChangeHubLayout(port1Fans, port2Fans, port1Leds, port2Leds);
                        return "OK";
                    }
                    else
                    {
                        return "Not Find Device";
                    }
                }
                else
                {
                    return "Not Find Device";
                }
            }
        }

        public void SetAsRockFanControll(List<ESCORE_FAN_ID> IdList)
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                int index = _usbDevices.FindIndex(x => x.GetType() == typeof(AsRockMotherBoard));
                if (index >= 0)
                {
                    ((AsRockMotherBoard)_usbDevices[index]).SetFanList(IdList);
                }
                else
                {
                    Debug.WriteLine("Not Find AsRockMotherBoard Device");
                }
            }
        }

        public ASRockMotherBoardModel GetMotherBoardTempModel()
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                int index = _usbDevices.FindIndex(x => x.GetType() == typeof(AsRockMotherBoard));
                if (index >= 0)
                {
                    ASRockMotherBoardModel Result = ((AsRockMotherBoard)_usbDevices[index]).GetTempMode();
                    return Result;
                }
                else
                {
                    Debug.WriteLine("Not Find AsRockMotherBoard Device");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<ASRockMode> GetASRockLedChList()
        {
            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                int index = _usbDevices.FindIndex(x => x.GetType() == typeof(AsRockMotherBoard));
                if (index >= 0)
                {
                    List<ASRockMode> Result = ((AsRockMotherBoard)_usbDevices[index]).GetLedControlList();
                    return Result;
                }
                else
                {
                    Debug.WriteLine("Not Find AsRockMotherBoard Device");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void ApplyMotherBoardChChange()
        {
            DeviceList_Changed();
        }
        /// <summary>
        /// Set mini hub fan speed
        /// </summary>
        /// <param name="port1FanSpeed">port1 fan speed (1~100)</param>
        /// <param name="port2FanSpeed">port2 fan speed (1~100)</param>
        public void SetMiniHubFanSpeed(int? port1FanSpeed, int? port2FanSpeed)
        {
            if (port1FanSpeed != null)
            {
                _port1FanSpeed = (int)port1FanSpeed < 10 ? 10 : (int)port1FanSpeed;
                _port1FanSpeed = _port1FanSpeed > 100 ? 100 : (int)_port1FanSpeed;
            }

            if (port2FanSpeed != null)
            {
                _port2FanSpeed = (int)port2FanSpeed < 10 ? 10 : (int)port2FanSpeed;
                _port2FanSpeed = _port2FanSpeed > 100 ? 100 : (int)_port2FanSpeed;
            }

            if (_usbDevices != null && _usbDevices.Count > 0)
            {
                int minihubIndex = _usbDevices.FindIndex(x => x.GetType() == typeof(IBPMiniHubDevice));
                if (minihubIndex >= 0)
                {
                    ((IBPMiniHubDevice)_usbDevices[minihubIndex]).SetFanSpeed(_port1FanSpeed, port2FanSpeed);
                }
            }
        }

        /// <summary>
        /// Change the universal controller list
        /// </summary>
        /// <param name="uSBDevices">list of USBDevices that would be detected</param>
        public void ChangeUniversalControllerList(List<USBDevices> uSBDevices)
        {
            lock (_usblist_locker)
            {
                ControllerList = new List<USBDevices>();
                foreach (USBDevices usbDevice in uSBDevices)
                {
                    if (!ControllerList.Contains(usbDevice))
                    {
                        ControllerList.Add(usbDevice);
                    }
                }
            }
            usbHotSwap.ChangeUSBDeviceControllerList(ControllerList);
        }
    }

    public class HardwareModel
    {
        /// <summary>
        /// Firmware version
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// Current device type
        /// </summary>
        public USBDevices USBDeviceType { get; set; }


        /// <summary>
        /// Device Name
        /// </summary>
        public string Name;
    }

    public static class STM32Directory
    {
        public static string STM32_DIRECTORY_PATH { set; get; }

        public static string CLI_PATH { set; get; }

        public static Dictionary<USBDevices, string> DEFAULT_BIN_PATH { set; get; }

        public static Dictionary<USBDevices, string> UPDATE_BIN_PATH { set; get; }

        public static bool STM32_Setting
        {
            get
            {
                return STM32Setting;
            }
        }

        private static bool STM32Setting = false;

        public static string SetSTM32Directory(string stm32DirectoryPath)
        {
            if (!System.IO.Directory.Exists(stm32DirectoryPath))
            {
                return "STM32 Directory not find";
            }
            else
            {
                STM32_DIRECTORY_PATH = stm32DirectoryPath;
            }

            if (!System.IO.File.Exists(stm32DirectoryPath + @"\\bin\\STM32_Programmer_CLI.exe"))
            {
                return "STM32_Programmer_CLI.exe not find";
            }
            else
            {
                CLI_PATH = stm32DirectoryPath + @"\\bin\\STM32_Programmer_CLI.exe";
            }

            if (!System.IO.File.Exists(stm32DirectoryPath + @"\\bin\\CNVS_1.0.1.1.hex"))
            {
                return "CNVS Base.hex not Find";
            }
            else if (!System.IO.File.Exists(stm32DirectoryPath + @"\\bin\\MiniHub_1.0.1.1.hex"))
            {
                return "Mini Hub Base.hex not Find";
            }
            else if (!System.IO.File.Exists(stm32DirectoryPath + @"\\bin\\CNVS_1.0.2.2.hex"))
            {
                return "CNVS v1 Base.hex not Find";
            }

            DEFAULT_BIN_PATH = new Dictionary<USBDevices, string>()
            {
                {USBDevices.CNVSLeft, stm32DirectoryPath + @"\\bin\\CNVS_1.0.1.1.hex" },
                {USBDevices.CNVSv1, stm32DirectoryPath + @"\\bin\\CNVS_1.0.2.2.hex" },
                {USBDevices.IBPMiniHub, stm32DirectoryPath + @"\\bin\\MiniHub_1.0.1.1.hex" },
            };

            STM32Setting = System.IO.File.Exists(STM32Directory.CLI_PATH);

            foreach (var filepath in STM32Directory.DEFAULT_BIN_PATH)
            {
                STM32Setting = STM32Setting && System.IO.File.Exists(filepath.Value);
            }

            return "success";
        }

        public static string SetUpdateBinPath(Tuple<USBDevices, string> filePath)
        {
            if (!System.IO.File.Exists(filePath.Item2))
            {
                return "Bin file not find";
            }

            if (UPDATE_BIN_PATH != null && UPDATE_BIN_PATH.ContainsKey(filePath.Item1))
            {
                UPDATE_BIN_PATH[filePath.Item1] = filePath.Item2;
            }
            else
            {
                if (UPDATE_BIN_PATH == null)
                {
                    UPDATE_BIN_PATH = new Dictionary<USBDevices, string>();
                }
                UPDATE_BIN_PATH.Add(filePath.Item1, filePath.Item2);
            }
            return "success";
        }
    }
}
