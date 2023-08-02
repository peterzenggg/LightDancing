using HidSharp;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LightDancing.Hardware.Devices
{
    internal class SerailKeebController : ILightingControl
    {
        private const string DEVICE_PID = "VID_0483&PID_5740";
        private const int BAUDRATES = 115200;

        public List<USBDeviceBase> InitDevices()
        {
            SerialDetector serialDetector = new SerialDetector();
            List<SerialStream> streams = serialDetector.GetSerialStreams(DEVICE_PID);

            if (streams != null && streams.Count > 0)
            {
                List<USBDeviceBase> hardwares = new List<USBDeviceBase>();
                foreach (SerialStream stream in streams)
                {
                    stream.BaudRate = BAUDRATES;
                    SerialKeebDevice keyboard = new SerialKeebDevice(stream);
                    hardwares.Add(keyboard);
                }

                return hardwares;
            }
            else
            {
                return null;
            }
        }
    }

    public class SerialKeebDevice : USBDeviceBase
    {
        public SerialKeebDevice(SerialStream deviceStream) : base(deviceStream)
        {
        }

        protected override List<LightingBase> InitDevice()
        {
            List<LightingBase> hardwares = new List<LightingBase>();
            SerailKeeb keeb = new SerailKeeb(_model);
            hardwares.Add(keeb);

            return hardwares;
        }

        protected override void SendToHardware(bool process, float brightness)
        {
            if (process)
            {
                foreach (var device in _lightingBase)
                {
                    device.ProcessStreaming(false, brightness);
                }
            }
        }

        protected override HardwareModel InitModel()
        {
            string deviceID = _deviceStream.Device.DevicePath;
            return new HardwareModel()
            {
                FirmwareVersion = GetFirmwareVersion(),
                DeviceID = deviceID,
                Name = "Serial Keeb",
            };
        }

        /// <summary>
        /// Get current device of firmware version.
        /// </summary>
        /// <returns>Firmware version</returns>
        private string GetFirmwareVersion()
        {
            byte[] commands = new byte[] { 0xff, 0xee, 0x00, 0x00, 0x01 };
            try
            {
                _deviceStream.Write(commands);
            }
            catch
            {
                Trace.WriteLine($"False toturn off on SerialKeyboard");
            }

            Thread.Sleep(20); // Need to wait for fw completed.

            if (commands[0] == 0xff && commands[1] == 0xee)
            {
                _deviceStream.Read(commands);
                return string.Format("{0}.{1}.{2}", commands[2], commands[3], commands[4]);
            }
            else
            {
                // MEMO: Add log to handle this error
                return "N/A";
            }
        }
    }

    internal class SerailKeeb : LightingBase
    {
        /// <summary>
        /// The keyboard of Y-Axis count
        /// </summary>
        private const int KEYBOARD_YAXIS_COUNTS = 14;

        /// <summary>
        /// The keyboard of X-Axis count
        /// </summary>
        /// </summary>
        private const int KEYBOARD_XAXIS_COUNTS = 27;

        /// <summary>
        /// KEY = Row count, Value = LED count
        /// </summary>
        private readonly Dictionary<int, List<int>> KEYBOARD_MAPPINGS = new Dictionary<int, List<int>>()
        {
            { 0, new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 } },
            { 1, new List<int>() { 0, 17, 19, 21, 23, 25, 26 } },
            { 2, new List<int>() { 0, 26 } },
            { 3, new List<int>() { 0, 2, 4, 6, 8, 9, 10, 11, 12, 13, 14, 15, 17, 19, 21, 24, 26 } },
            { 4, new List<int>() { 0, 26 } },
            { 5, new List<int>() { 0, 3, 5, 7, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 22, 24, 26 } },
            { 6, new List<int>() { 0, 26 } },
            { 7, new List<int>() { 4, 6, 8, 10, 11, 12, 13, 14, 15, 16, 17, 19, 21, 24 } },
            { 8, new List<int>()},
            { 9, new List<int>() { 5, 7, 9, 11, 12, 13, 14, 15, 16, 17, 18, 20, 22, 24 } },
            { 10, new List<int>()},
            { 11, new List<int>() { 2, 4, 6, 9, 10, 11, 12, 13, 16, 18, 19, 20, 22, 24 }},
            { 12, new List<int>()},
            { 13, new List<int>() { 0, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 24, 26 }},
        };

        public SerailKeeb(HardwareModel hardwareModel) : base(KEYBOARD_YAXIS_COUNTS, KEYBOARD_XAXIS_COUNTS, hardwareModel)
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
                Name = "Serial Keeb",
                //Type = LightingDevices.SerailKeyboard,
            };
        }

        protected override void SetKeyLayout()
        {
            KeyboardLayout = new List<List<Keyboard>>()
            { 
                //Row 1
                new List<Keyboard>(){ Keyboard.TopLED1, Keyboard.TopLED2, Keyboard.TopLED3, Keyboard.TopLED4, Keyboard.TopLED5, Keyboard.TopLED6, Keyboard.TopLED7,
                                      Keyboard.TopLED8, Keyboard.TopLED9, Keyboard.TopLED10, Keyboard.TopLED11, Keyboard.TopLED12, Keyboard.TopLED13, Keyboard.TopLED14,
                                      Keyboard.TopLED15, Keyboard.TopLED16, Keyboard.TopLED17,Keyboard.TopLED18, Keyboard.TopLED19, Keyboard.TopLED20, Keyboard.TopLED21,
                                      Keyboard.TopLED22, Keyboard.TopLED23, Keyboard.TopLED24, Keyboard.TopLED25},
                //Row 2
                new List<Keyboard>(){ Keyboard.LeftLED1, Keyboard.MediaLED1, Keyboard.MediaLED2, Keyboard.MediaLED3, Keyboard.MediaLED4, Keyboard.MediaLED5, Keyboard.RightLED1 },
                //Row 4
                new List<Keyboard>(){ Keyboard.LeftLED2, Keyboard.ESC,Keyboard.One,Keyboard.Two, Keyboard.Three, Keyboard.Four,
                                      Keyboard.Five, Keyboard.Six, Keyboard.Seven, Keyboard.Eight, Keyboard.Nine, Keyboard.Zero, Keyboard.Hyphen, Keyboard.Plus,
                                      Keyboard.BackSpace, Keyboard.DELETE, Keyboard.RightLED2 },
                //Row 6
                new List<Keyboard>(){ Keyboard.LeftLED3, Keyboard.Tab, Keyboard.Q, Keyboard.W, Keyboard.E, Keyboard.R, Keyboard.T,
                                      Keyboard.Y, Keyboard.U, Keyboard.I, Keyboard.O, Keyboard.P, Keyboard.LeftBracket, Keyboard.RightBracket, Keyboard.BackSlash,
                                      Keyboard.HOME, Keyboard.RightLED3 },
                //Row 8
                new List<Keyboard>(){ Keyboard.LeftLED4, Keyboard.CapLock, Keyboard.A, Keyboard.S, Keyboard.D, Keyboard.F, Keyboard.G, Keyboard.H,
                                      Keyboard.J, Keyboard.K, Keyboard.L, Keyboard.Semicolon, Keyboard.SingleQuote, Keyboard.Enter, Keyboard.PAGEUp, Keyboard.RightLED4 },
                //Row 10
                new List<Keyboard>(){ Keyboard.LeftLED5, Keyboard.LeftShift,Keyboard.Z, Keyboard.X, Keyboard.C, Keyboard.V,Keyboard.B,
                                      Keyboard.N, Keyboard.M, Keyboard.Comma, Keyboard.Period, Keyboard.Slash,Keyboard.RightShift, Keyboard.UP, Keyboard.PAGEDown, Keyboard.RightLED5 },
                //Row 12
                new List<Keyboard>(){ Keyboard.LeftLED6, Keyboard.CTRL_L,Keyboard.WIN_L, Keyboard.ALT_L, Keyboard.SPACE_L, Keyboard.SPACE, Keyboard.SPACE_R,
                                      Keyboard.ALT_R, Keyboard.FN, Keyboard.BLOCKER, Keyboard.LEFT, Keyboard.DOWN, Keyboard.RIGHT, Keyboard.RightLED6 },
                //Row 14
                new List<Keyboard>(){ Keyboard.BottomLED1, Keyboard.BottomLED2,Keyboard.BottomLED3, Keyboard.BottomLED4, Keyboard.BottomLED5,
                                      Keyboard.BottomLED6, Keyboard.BottomLED7, Keyboard.BottomLED8, Keyboard.BottomLED9, Keyboard.BottomLED10, Keyboard.BottomLED11,
                                      Keyboard.BottomLED12, Keyboard.BottomLED13, Keyboard.BottomLED14, Keyboard.BottomLED15, Keyboard.BottomLED16, Keyboard.BottomLED17,
                                      Keyboard.BottomLED18, Keyboard.BottomLED19, Keyboard.BottomLED20, Keyboard.BottomLED21, Keyboard.BottomLED22 }
            };
        }

        //Do NOT contain single key RGB preview
        protected override void ProcessColor(ColorRGB[,] colorMatrix)
        {
            List<byte> collectBytes = new List<byte>() { 0xff, 0xee, 0x01, 0x7a, 0x00 };
            keyColor = new Dictionary<Keyboard, ColorRGB>();

            foreach (var rowMappings in KEYBOARD_MAPPINGS)
            {
                foreach (int cloumn in rowMappings.Value)
                {
                    var color = colorMatrix[rowMappings.Key, cloumn];
                    byte[] grb = new byte[] { color.G, color.R, color.B };
                    collectBytes.AddRange(grb);
                }
            }

            try
            {
                //_deviceStream.Write(collectBytes.ToArray(), 0, collectBytes.Count);
            }
            catch
            {
                Trace.WriteLine($"False to streaming on SerialKeyboard");
            }
        }

        /// <summary>
        /// Do nothing cuz there's no keylayout for serial keyboard
        /// </summary>
        /// <param name="keyColors"></param>
        protected override void ProcessZoneSelection(Dictionary<Keyboard, ColorRGB> keyColors)
        {

        }

        protected override void TurnOffLed()
        {
            int totalLEDs = KEYBOARD_MAPPINGS.Sum(x => x.Value.Count);
            byte[] collectBytes = new byte[5 + (totalLEDs * 3)]; // 0xff, 0xee, 0x01, 0x7a, 0x00 + LEDs bytes
            collectBytes[0] = 0xff;
            collectBytes[1] = 0xee;
            collectBytes[2] = 0x01;
            collectBytes[3] = 0x7a;
            collectBytes[4] = 0x00;

            try
            {
                //_deviceStream.Write(collectBytes, 0, collectBytes.Length);
            }
            catch
            {
                Trace.WriteLine($"False to streaming on SerialKeyboard");
            }
        }
    }
}
