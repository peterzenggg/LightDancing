using AuraServiceLib;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;
using System.Globalization;

namespace LightDancing.Hardware.Devices.UniversalDevice.ASUS.MotherBoard
{
    public class Z790AController : ILightingControl
    {
        public List<USBDeviceBase> InitDevices()
        {
            try
            {
                IAuraSdk sdk = new AuraSdk();
                sdk.SwitchMode();
                IAuraSyncDeviceCollection devices = sdk.Enumerate(0);

                var targetDevice = new List<IAuraSyncDevice>();
                var targetDeviceNames = new List<string>() { "Mainboard_Master", "Vga 1", "ENE_RGB_For_ASUS0", "ENE_RGB_For_ASUS1", "ENE_RGB_For_ASUS2", "ENE_RGB_For_ASUS3", "AddressableStrip 1", "AddressableStrip 2", "AddressableStrip 3" };
                foreach (IAuraSyncDevice dev in devices)
                {
                    if (!targetDeviceNames.Contains(dev.Name))
                        continue;
                    targetDevice.Add(dev);
                }

                if (targetDevice.Count > 0)
                {
                    List<USBDeviceBase> hardwares = new List<USBDeviceBase>
                {
                    new Z790ADevice(targetDevice)
                };
                    return hardwares;
                }
            }
            catch { }

            return null;
        }
    }

    public class Z790ADevice : USBDeviceBase
    {
        private readonly List<IAuraSyncDevice> _devices;

        public Z790ADevice(List<IAuraSyncDevice> devices) : base()
        {
            _devices = devices;
            _lightingBase = InitDevice();
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                //USBDeviceType = USBDevices.ASUSZ790A,
                Name = "ASUS Z790-A"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            foreach (IAuraSyncDevice dev in _devices)
            {
                Z790ALighting keyboard = new Z790ALighting(1, 1, _model, dev);
                hardwares.Add(keyboard);
            }

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            ThrottledAction throttledAction = new ThrottledAction(50);

            throttledAction.Invoke(() =>
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
                    var colorData = lightingBase.GetHttpClientColorData();
                    var lightingBaseDevice = ((Z790ALighting)lightingBase)._dev;
                    foreach (IAuraRgbLight light in lightingBaseDevice.Lights)
                    {
                        light.Color = uint.Parse(colorData, NumberStyles.HexNumber);
                    }

                    lightingBaseDevice.Apply();
                }
            });
        }
    }

    public class Z790ALighting : LightingBase
    {
        private readonly int _yAxis;

        private readonly int _xAxis;

        public readonly IAuraSyncDevice _dev;

        public Z790ALighting(int yAxis, int xAxis, HardwareModel hardwareModel, IAuraSyncDevice dev) : base(yAxis, xAxis, hardwareModel)
        {
            _yAxis = yAxis;
            _xAxis = xAxis;
            _dev = dev;
            _model.Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis };
            _model.DeviceID = dev.Name;
            _model.Name = dev.Name;
        }

        protected override LightingModel InitModel()
        {
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = _xAxis, Height = _yAxis },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = "",
                //Type = LightingDevices.ASUSZ790A,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "ASUS Z790-A"
            };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            if (colorMatrix.Length < 1) { return; }
            var color = colorMatrix[0, 0];
            //0x00BBGGRR
            _httpClientColorData = $"00{Methods.ByteToHexString(color.B)}{Methods.ByteToHexString(color.G)}{Methods.ByteToHexString(color.R)}";
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
