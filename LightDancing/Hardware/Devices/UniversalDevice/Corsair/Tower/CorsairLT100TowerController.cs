using HidSharp;
using System.Collections.Generic;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Linq;
using System;

namespace LightDancing.Hardware.Devices.UniversalDevice.Corsair.Tower
{
    internal class CorsairLT100TowerConfig
    {
        /// <summary>
        /// Corsair Tower Max Output Buffer 
        /// </summary>
        public const int MAX_OUTPUT_LENGTH = 65;
    }

    internal class CorsairLT100TowerController : ILightingControl
    {
        private const int DEVICE_VID = 0x1B1C;
        private const int DEVICE_PID = 0x0C23;
        /// <summary>
        /// Corsair Tower Max Intput Buffer 
        /// </summary>
        private const int MAX_INPUT_LENGTH = 17;
        /// <summary>
        /// Corsair Tower Max Feature Buffer 
        /// </summary>
        private const int MAX_FEATURE_LENGTH = 0;

        public List<USBDeviceBase> InitDevices()
        {
            List<HidStream> streams = new HidDetector().GetHidStreams(DEVICE_VID, DEVICE_PID, MAX_FEATURE_LENGTH, CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH, MAX_INPUT_LENGTH);
            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (HidStream stream in streams)
                {
                    CorsairLT100TowerDevice tower = new CorsairLT100TowerDevice(stream);
                    hardwares.Add(tower);
                }
                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class CorsairLT100TowerDevice : USBDeviceBase
    {
        public CorsairLT100TowerDevice(HidStream deviceStream) : base(deviceStream)
        {
        }

        protected override HardwareModel InitModel()
        {
            return new HardwareModel()
            {
                FirmwareVersion = "NA",
                DeviceID = _deviceStream.Device.DevicePath,
                USBDeviceType = USBDevices.CorsairLT100Tower,
                Name = "Corsair LT100 Tower"
            };
        }

        protected override List<LightingBase> InitDevice()
        {
            return new List<LightingBase>()
            {
                new CorsairLT100Tower(_model)
            };
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (_lightingBase != null && _lightingBase.Any())
            {
                if (process)
                {
                    _lightingBase.FirstOrDefault().ProcessStreaming(false, brightness);
                }

                List<byte> allCommand = _lightingBase.FirstOrDefault().GetDisplayColors();

                InitTower();

                for (int i = 0; i < 3; i++)///Three Channel Command To Stream
                {
                    SendStream(allCommand.GetRange(i * CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH, CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH).ToArray());
                }

                CommitTowerColorCommand();
            }
        }

        /// <summary>
        /// Init LT100 Tower Can Be Wrtie
        /// </summary>
        private void InitTower()
        {
            byte[] packet = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            packet[1] = 0x38;
            packet[3] = 0x02;
            SendStream(packet);

            packet = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            packet[1] = 0x34;
            SendStream(packet);
        }

        /// <summary>
        /// Commit All Command Into LT100
        /// </summary>
        private void CommitTowerColorCommand()
        {
            byte[] packet = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            packet[1] = 0x33;
            packet[2] = 0xFF;
            SendStream(packet);
        }

        private void SendStream(byte[] byteArray)
        {
            try
            {
                ((HidStream)_deviceStream).Write(byteArray);
            }
            catch
            {
                Console.WriteLine($"Fail to streaming on CorsairLT100");
            }
        }
    }

    public class CorsairLT100Tower : LightingBase
    {
        /// <summary>
        /// Canvs Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 27;
        /// <summary>
        /// Canvs X-Axis count
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 2;
        private const byte FIRST_COMMAND = 0x32;
        private const byte SECOND_COMMAND = 0x50;
        private const int ONE_RGB_ROUND_COUNTS = 6;
        ///// <summary>
        ///// key = Canvs's key, Value = 2D position on Canvs (ex: ESC = (0, 0)), PosistionY = y position, PosistionX = x position
        ///// </summary>
        private readonly List<LayoutModel> KEYS_LAYOUTS = new List<LayoutModel>()
        {
            new LayoutModel(){ LED = Keyboard.LED1, PosistionY = 26, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED2, PosistionY = 25, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED3, PosistionY = 24, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED4, PosistionY = 23, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED5, PosistionY = 22, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED6, PosistionY = 21, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED7, PosistionY = 20, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED8, PosistionY = 19, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED9, PosistionY = 18, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED10, PosistionY = 17, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED11, PosistionY = 16, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED12, PosistionY = 15, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED13, PosistionY = 14, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED14, PosistionY = 13, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED15, PosistionY = 12, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED16, PosistionY = 11, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED17, PosistionY = 10, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED18, PosistionY = 9, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED19, PosistionY = 8, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED20, PosistionY = 7, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED21, PosistionY = 6, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED22, PosistionY = 5, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED23, PosistionY = 4, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED24, PosistionY = 3, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED25, PosistionY = 2, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED26, PosistionY = 1, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED27, PosistionY = 0, PosistionX = 1 },
            new LayoutModel(){ LED = Keyboard.LED28, PosistionY = 26, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED29, PosistionY = 25, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED30, PosistionY = 24, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED31, PosistionY = 23, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED32, PosistionY = 22, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED33, PosistionY = 21, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED34, PosistionY = 20, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED35, PosistionY = 19, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED36, PosistionY = 18, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED37, PosistionY = 17, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED38, PosistionY = 16, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED39, PosistionY = 15, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED40, PosistionY = 14, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED41, PosistionY = 13, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED42, PosistionY = 12, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED43, PosistionY = 11, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED44, PosistionY = 10, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED45, PosistionY = 9, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED46, PosistionY = 8, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED47, PosistionY = 7, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED48, PosistionY = 6, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED49, PosistionY = 5, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED50, PosistionY = 4, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED51, PosistionY = 3, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED52, PosistionY = 2, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED53, PosistionY = 1, PosistionX = 0 },
            new LayoutModel(){ LED = Keyboard.LED54, PosistionY = 0, PosistionX = 0 },
        };

        public CorsairLT100Tower(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Type = LightingDevices.CorsairLT100Tower,
                USBDeviceType = _usbModel.USBDeviceType,
                Name = "Corsair LT100 Tower"
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { };
        }

        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            byte[] redCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            redCommand[1] = FIRST_COMMAND;
            redCommand[4] = SECOND_COMMAND;

            byte[] greenCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            greenCommand[1] = FIRST_COMMAND;
            greenCommand[4] = SECOND_COMMAND;
            greenCommand[5] = 0x01;

            byte[] blueCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            blueCommand[1] = FIRST_COMMAND;
            blueCommand[4] = SECOND_COMMAND;
            blueCommand[5] = 0x02;

            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                LayoutModel model = KEYS_LAYOUTS[i];
                ColorRGB PosistionColor = colorMatrix[model.PosistionY, model.PosistionX];
                redCommand[i + ONE_RGB_ROUND_COUNTS] = PosistionColor.R;
                greenCommand[i + ONE_RGB_ROUND_COUNTS] = PosistionColor.G;
                blueCommand[i + ONE_RGB_ROUND_COUNTS] = PosistionColor.B;
            }

            _displayColorBytes = new List<byte>();
            _displayColorBytes.AddRange(redCommand);
            _displayColorBytes.AddRange(greenCommand);
            _displayColorBytes.AddRange(blueCommand);
        }

        protected override void TurnOffLed()
        {
            byte[] RedCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            RedCommand[1] = FIRST_COMMAND;
            RedCommand[4] = SECOND_COMMAND;

            byte[] GreenCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            GreenCommand[1] = FIRST_COMMAND;
            GreenCommand[4] = SECOND_COMMAND;
            GreenCommand[5] = 0x01;

            byte[] BlueCommand = new byte[CorsairLT100TowerConfig.MAX_OUTPUT_LENGTH];
            BlueCommand[1] = FIRST_COMMAND;
            BlueCommand[4] = SECOND_COMMAND;
            BlueCommand[5] = 0x02;

            for (int i = 0; i < KEYS_LAYOUTS.Count; i++)
            {
                RedCommand[i + ONE_RGB_ROUND_COUNTS] = 0;
                GreenCommand[i + ONE_RGB_ROUND_COUNTS] = 0;
                BlueCommand[i + ONE_RGB_ROUND_COUNTS] = 0;
            }

            _displayColorBytes = new List<byte>();
            _displayColorBytes.AddRange(RedCommand);
            _displayColorBytes.AddRange(GreenCommand);
            _displayColorBytes.AddRange(BlueCommand);
        }

        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {
        }
    }
}
