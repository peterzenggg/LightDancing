using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LightDancing.Syncs
{
    public class ScreenInfo
    {
        private static ScreenInfo instance = null;

        /// <summary>
        /// PrimaryScreen's Width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// PrimaryScreen's Height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The color martix of scale width
        /// </summary>
        public int ScaleWidth { get; set; }

        /// <summary>
        /// The color martix of scale Hight
        /// </summary>
        public int ScaleHeight { get; set; }

        /// <summary>
        /// Action when resolution ratio is changed
        /// </summary>
        public Action ResolutionChanged { get; set; }

        public static ScreenInfo Instance
        {
            get
            {
                return instance ?? new ScreenInfo();
            }
        }

        private ScreenInfo()
        {
            if (instance == null)
            {
                instance = this;
                const int ENUM_CURRENT_SETTINGS = -1;

                DEVMODE devMode = default;
                devMode.dmSize = (short)Marshal.SizeOf(devMode);
                EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

                if (devMode.dmPelsWidth > 0 && devMode.dmPelsHeight > 0)
                {
                    Width = devMode.dmPelsWidth;
                    Height = devMode.dmPelsHeight;
                }
                else
                {
                    using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        Width = (int)graphics.VisibleClipBounds.Width;
                        Height = (int)graphics.VisibleClipBounds.Height;
                    };
                }
                SetScaleSize(10);
            }
        }

        public void SetScaleSize(int ratio)
        {
            ScaleWidth = Width / ratio;
            ScaleHeight = Height / ratio;
            ResolutionChanged?.Invoke();
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
    }
}