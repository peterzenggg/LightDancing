using System.Collections.Generic;
using LightDancing.Enums;
using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using System.Diagnostics;
using System.Linq;
using System;

namespace LightDancing.Hardware.Devices.UniversalDevice.Razer.MousePads
{
    internal class RazerStriderChromaConfig
    {
        /// <summary>
        /// Razer Byte Array Posistion For Access Byte 
        /// </summary>
        internal const int RAZER_ACCESS_BYTE = 89;
        /// <summary>
        /// Razer Send Max Feature Buffer LENGTH
        /// </summary>
        internal const int MAX_FEATURE_LENGTH = 91;
    }

    internal class RazerStriderChromaController : ILightingControl
    {
        private const int DEVICE_VID = 0x1532;
        private const int DEVICE_PID = 0x0c05;

        public List<USBDeviceBase> InitDevices()
        {
            List<HidStream> streams = new HidDetector().GetHidStreams(DEVICE_VID, DEVICE_PID, RazerStriderChromaConfig.MAX_FEATURE_LENGTH, true);
            if (streams != null && streams.Any())
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    hardwares.Add(new RazerStriderChromaDevice(stream));
                }

                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class RazerStriderChromaDevice : USBDeviceBase
    {
        public RazerStriderChromaDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                USBDeviceType = USBDevices.RazerStriderChroma,
                Name = "Razer Strider Chroma"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return new List<LightingBase>() {
                new RazerStriderChroma(_model)
            };
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            try
            {
                if (_lightingBase != null && _lightingBase.Any())
                {
                    var lightingBase = _lightingBase.FirstOrDefault();

                    if (process)
                    {
                        lightingBase.ProcessStreaming(false, brightness);
                    }

                    ((HidStream)_deviceStream).SetFeature(lightingBase.GetDisplayColors().ToArray());
                }
            }
            catch
            {
                Console.WriteLine($"False to streaming on RazerStriderChroma");
            }
        }

        public override void TurnFwAnimationOn()
        {
            byte[] sendCommand = new byte[RazerStriderChromaConfig.MAX_FEATURE_LENGTH];
            sendCommand[2] = 0x1F;
            sendCommand[6] = 0x0C;
            sendCommand[7] = 0x0F;
            sendCommand[8] = 0x82;
            sendCommand[9] = 0x01;
            sendCommand[10] = 0x05;
            SendTurnFwCommand(sendCommand);

            sendCommand[8] = 0x02;
            sendCommand[9] = 0x01;
            sendCommand[10] = 0x00;
            sendCommand[11] = 0x04;
            sendCommand[12] = 0x02;
            sendCommand[13] = 0x28;
            sendCommand[14] = 0x02;
            SendTurnFwCommand(sendCommand);

            sendCommand[6] = 0x02;
            sendCommand[7] = 0x00;
            sendCommand[8] = 0x84;
            sendCommand[9] = 0x00;
            sendCommand[10] = 0x00;
            sendCommand[11] = 0x00;
            sendCommand[12] = 0x00;
            sendCommand[13] = 0x00;
            sendCommand[14] = 0x00;
            SendTurnFwCommand(sendCommand);

            sendCommand[8] = 0x04;
            SendTurnFwCommand(sendCommand);
        }

        private void SendTurnFwCommand(byte[] sendCommand)
        {
            try
            {
                sendCommand[RazerStriderChromaConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(sendCommand);
                ((HidStream)_deviceStream).SetFeature(sendCommand);
                System.Threading.Thread.Sleep(1);//need add sleep 1 for HidStream access
            }
            catch
            {
                Trace.WriteLine($"False to streaming on RazerStriderChroma");
            }
        }
    }

    public class RazerStriderChroma : LightingBase
    {
        /// <summary>
        /// MousePads Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 3;

        /// <summary>
        /// MousePads X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 9;

        ///// <summary>
        ///// key = MousePads's key, Value = 2D position on MousePads (ex: ESC = (0, 0)), PosistionY = y position, PosistionX = x position
        ///// </summary>
        private readonly List<LayoutModel> KEYS_LAYOUTS = new List<LayoutModel>()
        {
            new LayoutModel(){ LED = Keyboard.LED1, PosistionY = 2, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED2, PosistionY = 2, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED3, PosistionY = 2, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED4, PosistionY = 2, PosistionX = 3 },
            new LayoutModel(){ LED = Keyboard.LED5, PosistionY = 2, PosistionX = 4 },
            new LayoutModel(){ LED = Keyboard.LED6, PosistionY = 2, PosistionX = 5 },
            new LayoutModel(){ LED = Keyboard.LED7, PosistionY = 2, PosistionX = 6 },
            new LayoutModel(){ LED = Keyboard.LED8, PosistionY = 2, PosistionX = 7 },
            new LayoutModel(){ LED = Keyboard.LED9, PosistionY = 2, PosistionX = 8 },
            new LayoutModel(){ LED = Keyboard.LED10, PosistionY = 1, PosistionX = 8 },
            new LayoutModel(){ LED = Keyboard.LED11, PosistionY = 0, PosistionX = 8 },
            new LayoutModel(){ LED = Keyboard.LED12, PosistionY = 0, PosistionX = 7 },
            new LayoutModel(){ LED = Keyboard.LED13, PosistionY = 0, PosistionX = 6 },
            new LayoutModel(){ LED = Keyboard.LED14, PosistionY = 0, PosistionX = 5 },
            new LayoutModel(){ LED = Keyboard.LED15, PosistionY = 0, PosistionX = 4 },
            new LayoutModel(){ LED = Keyboard.LED16, PosistionY = 0, PosistionX = 3 },
            new LayoutModel(){ LED = Keyboard.LED17, PosistionY = 0, PosistionX = 2 },
            new LayoutModel(){ LED = Keyboard.LED18, PosistionY = 0, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED19, PosistionY = 0, PosistionX = 0 },
        };

        public RazerStriderChroma(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
        {
        }

        /// <summary>
        /// Init the model
        /// MEMO: Device ID should be unique, the better way is get it from the firmware.
        /// </summary>
        /// <returns></returns>
        protected override LightingModel InitModel()
        {
            SetKeyLayout();
            return new LightingModel()
            {
                Layouts = new MatrixLayouts() { Width = KEYBOARD_XAXIS_COUNTS, Height = KEYBOARD_YAXIS_COUNTS },
                FirmwareVersion = _usbModel.FirmwareVersion,
                DeviceID = _usbModel.DeviceID,
                Type = LightingDevices.RazerStriderChroma,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Razer Strider Chroma"
            };
        }

        /// <summary>
        /// All Led Command combain to 91 bytes
        /// </summary>
        /// <param name="colorMatrix"></param>
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            byte[] commandCollect = new byte[RazerStriderChromaConfig.MAX_FEATURE_LENGTH];
            commandCollect[2] = 0x1F;
            commandCollect[6] = 0x3E;
            commandCollect[7] = 0x0F;
            commandCollect[8] = 0x03;
            commandCollect[13] = 0x12;

            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                int ledIndex = (i * 3) + 14;
                var model = KEYS_LAYOUTS[i];
                ColorRGB color = colorMatrix[model.PosistionY, model.PosistionX];
                commandCollect[ledIndex] = color.R;
                commandCollect[ledIndex + 1] = color.G;
                commandCollect[ledIndex + 2] = color.B;
            }

            commandCollect[RazerStriderChromaConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(commandCollect);

            _displayColorBytes = commandCollect.ToList();
        }

        protected override void TurnOffLed()
        {
            byte[] commandCollect = new byte[RazerStriderChromaConfig.MAX_FEATURE_LENGTH];
            commandCollect[2] = 0x1F;
            commandCollect[6] = 0x3E;
            commandCollect[7] = 0x0F;
            commandCollect[8] = 0x03;
            commandCollect[13] = 0x12;

            commandCollect[RazerStriderChromaConfig.RAZER_ACCESS_BYTE] = Methods.CalculateRazerAccessByte(commandCollect);
            _displayColorBytes = commandCollect.ToList();
        }

        protected override void SetKeyLayout()
        {
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
