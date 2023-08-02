using CaptureSampleCore;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Streamings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;


namespace LightDancing.Syncs
{
    public class MonitorInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
        public string DeviceName { get; set; }
        public IntPtr Hmon { get; set; }
    }

    static class MonitorEnumerationHelper
    {
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MonitorInfoEx
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                {
                    MonitorInfoEx monitorInfo = new MonitorInfoEx();
                    monitorInfo.Size = Marshal.SizeOf(monitorInfo);
                    bool success = GetMonitorInfo(hMonitor, ref monitorInfo);
                    if (success)
                    {
                        var info = new MonitorInfo
                        {
                            ScreenSize = new Vector2(monitorInfo.Monitor.right - monitorInfo.Monitor.left, monitorInfo.Monitor.bottom - monitorInfo.Monitor.top),
                            MonitorArea = new Rect(monitorInfo.Monitor.left, monitorInfo.Monitor.top, monitorInfo.Monitor.right - monitorInfo.Monitor.left, monitorInfo.Monitor.bottom - monitorInfo.Monitor.top),
                            WorkArea = new Rect(monitorInfo.WorkArea.left, monitorInfo.WorkArea.top, monitorInfo.WorkArea.right - monitorInfo.WorkArea.left, monitorInfo.WorkArea.bottom - monitorInfo.WorkArea.top),
                            IsPrimary = monitorInfo.Flags > 0,
                            Hmon = hMonitor,
                            DeviceName = monitorInfo.DeviceName
                        };
                        result.Add(info);
                    }
                    return true;
                }, IntPtr.Zero);
            return result;
        }
    }

    public class ScreenSyncHelperMS : IScreenSyncHelper
    {
        private static readonly Lazy<ScreenSyncHelperMS> lazy = new Lazy<ScreenSyncHelperMS>(() => new ScreenSyncHelperMS());
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly object _locker = new object();
        private ColorRGB[,] _syncColors;
        private ColorEffectBase _mode;
        private Action m_updateHandler;
        private ImageFilterBase _filter;
        private bool _blueReduction = false;
        private Action FlushList { get; set; }
        private List<MonitorInfo> _monitorInfoList = new List<MonitorInfo>();
        private readonly List<string> _monitorNameList = new List<string>();
        private MonitorInfo _selectMonitor;
        private readonly BasicApplication _app;
        private long _fps;
        private readonly Stopwatch _countTime;

        public static ScreenSyncHelperMS Instance => lazy.Value;

        public Action ScreenUpdated { get; set; }

        public ScreenSyncHelperMS()
        {
            InitMonitorList();
            if(_monitorInfoList.Count > 0)
            {
                _selectMonitor = _monitorInfoList[0];
            }
            new Thread(new ThreadStart(GetMonitorInfoList)) { IsBackground = true }.Start();
            _countTime = new Stopwatch();
            _mode = new Standard();
            _filter = new NoneFilter();
            _app = new BasicApplication();
        }

        /// <summary>
        /// Init monitor list
        /// </summary>
        private void InitMonitorList()
        {
            var monitorList = MonitorEnumerationHelper.GetMonitors();
            if (monitorList.Count() != _monitorInfoList.Count)
            {
                _monitorInfoList = monitorList.ToList();
                if (_monitorInfoList.Count > 0)
                {
                    foreach (var monitor in _monitorInfoList)
                    {
                        _monitorNameList.Add(monitor.DeviceName);
                    }
                }
                FlushList?.Invoke();
            }
        }

        public void RegisterFlushList(Action flushListFunction)
        {
            FlushList = flushListFunction;
        }

        private void GetMonitorInfoList()
        {
            while (true)
            {
                InitMonitorList();
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Get list of monitor names
        /// </summary>
        /// <returns>list of string of monitor names</returns>
        public List<string> GetMonitorList()
        {
            return _monitorNameList;
        }

        public void UpdateMode(ColorEffectBase mode)
        {
            _mode = mode;
        }

        public void SetBlur(ImageFilterBase filter)
        {
            _filter = filter;
        }

        public string GetBase64StringColor()
        {
            var colors = GetColors();
            ScreenUpdated?.Invoke();
            return Methods.ColorRgbToBase64(colors);
        }

        public void SetBlueFilter(bool _reduction)
        {
            _blueReduction = _reduction;
        }

        /// <summary>
        /// Get color matrix
        /// </summary>
        /// <returns>Color matrix</returns>
        public ColorRGB[,] GetColors()
        {
            return _syncColors;
        }

        /// <summary>
        /// Return null for gifsync
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmapForPreviewImage()
        {
            return null;
        }

        public Bitmap GetBitmapForAlphaVideo()
        {
            return null;
        }

        /// <summary>
        /// Return null for gifsync
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmapForStreaming()
        {
            return null;
        }

        private bool process = false;

        /// <summary>
        /// Start Screen Sync
        /// </summary>
        public void Start()
        {
            CancellationToken cancelToken = _cancelSource.Token;
            _ = Task.Run(() =>
            {
                lock (_locker)
                {
                    _app.StartCaptureFromItem(_selectMonitor.Hmon);
                    process = true;
                    int count = 0;
                    long fpsTotal = 0;
                    _countTime.Start();
                    while (!cancelToken.IsCancellationRequested)
                    {
                        if (StreamingHelper.Instance.FrameRateContolTimer.IsAvailable())
                        {
                            _syncColors = CaptureColors();
                            m_updateHandler?.Invoke();
                            _countTime.Stop();
                            fpsTotal += 1000 / _countTime.ElapsedMilliseconds;
                            _countTime.Reset();
                            _countTime.Start();

                            count++;
                            if (count == 10)
                            {
                                _fps = fpsTotal / count;
                                fpsTotal = 0;
                                count = 0;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }

                    _app.StopCapture();
                    process = false;
                    _syncColors = GetBlackScreen();
                    m_updateHandler?.Invoke();
                }
            }, cancelToken);
        }

        /// <summary>
        /// Turn sync off and set the last image to all black
        /// </summary>
        /// <returns></returns>
        private ColorRGB[,] GetBlackScreen()
        {
            ColorRGB[,] result = new ColorRGB[ScreenInfo.Instance.ScaleHeight, ScreenInfo.Instance.ScaleWidth];
            for (int height = 0; height < ScreenInfo.Instance.ScaleHeight; height++)
            {
                for (int width = 0; width < ScreenInfo.Instance.ScaleWidth; width++)
                {
                    result[height, width] = ColorRGB.Black();
                }
            }
            return result;
        }

        /// <summary>
        /// Stop Screen Sync
        /// </summary>
        public void Stop()
        {
            _cancelSource.Cancel();
            lock (_locker)
            {
                _cancelSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// If computure is avaliable for screen sync capture
        /// </summary>
        /// <returns></returns>
        public bool SupportSync()
        {
            return _app.SupportCapture();
        }

        private ColorRGB[,] GetMatrixColors(Bitmap image)
        {
            if (image == null)
                return null;
            int yAxisCount = ScreenInfo.Instance.ScaleHeight;
            int xAxisCount = ScreenInfo.Instance.ScaleWidth;
            ColorRGB[,] result = new ColorRGB[yAxisCount, xAxisCount];
            double widthRange = (double)image.Width / xAxisCount;
            double heightRange = (double)image.Height / yAxisCount;
            double halfWidth = widthRange / 2;
            double halfHeight = heightRange / 2;
            for (int y = 0; y < yAxisCount; y++)
            {
                for (int x = 0; x < xAxisCount; x++)
                {
                    int xAxis = (int)((x * widthRange) + halfWidth);
                    int yAxis = (int)((y * heightRange) + halfHeight);
                    Color color = image.GetPixel(xAxis, yAxis);
                    result[y, x] = _mode.GetColors(color.R, color.G, color.B);
                    if (_blueReduction)
                        result[y, x].SetColorFilter(1f, 1f, 0.75f);
                }
            }
            return result;
        }

        /// <summary>
        /// Capture color matrix
        /// MEMO: ScreenSync uses desktop duplication, CPU loading is heavy, need to adjust in the future!
        /// </summary>
        /// <returns>Color matrix</returns>
        private ColorRGB[,] CaptureColors()
        {
            ColorRGB[,] result0 = GetMatrixColors(_app.GetCaptureBitmap());
            result0 = _filter.Process(result0);
            return result0;
        }

        /// <summary>
        /// change syncing monitor
        /// </summary>
        /// <param name="index">monitor index from monitorList</param>
        public void ChangeMonitor(int index)
        {
            /*if index is not valid, do not change screen*/
            if (_monitorInfoList.Count > index && index != -1)
            {
                _selectMonitor = _monitorInfoList[index];
            }

            /*if screensync is processing, need to stop and restart to change selected monitor*/
            if (process)
            {
                Stop();
                Start();
            }
        }

        public void RegisterPreviewUpdateCallback(Action func)
        {
            m_updateHandler = func;
        }

        public void SetSpeedConfig(int speed)
        {
            //CANNOT BE CONTROLLED
        }

        /// <summary>
        /// Get Screen Sync frame rate
        /// </summary>
        /// <returns>FPS</returns>
        public string GetFPS()
        {
            return _fps.ToString();
        }
    }
}
