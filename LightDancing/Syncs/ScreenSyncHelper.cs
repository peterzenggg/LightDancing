using DesktopDuplication;
using Force.DeepCloner;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Streamings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace LightDancing.Syncs
{
    public class ScreenSyncHelper : IScreenSyncHelper
    {
        private static readonly Lazy<ScreenSyncHelper> lazy = new Lazy<ScreenSyncHelper>(() => new ScreenSyncHelper());
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly object _locker = new object();
        private bool _isProcessing = false;
        private ColorRGB[,] _syncColors;
        private ColorEffectBase _mode;
        private Action m_updateHandler;
        private Dictionary<string, Tuple<int, int>> _monitorDictionary = new Dictionary<string, Tuple<int, int>>();
        private readonly List<DesktopDuplicator> _desktopDuplicators = new List<DesktopDuplicator>();
        private readonly List<string> _monitorNameList = new List<string>();
        private string _selectedMonitor;
        private Bitmap lastImage;
        private ImageFilterBase _filter;
        private Action FlushList { get; set; }
        private int select = 0;
        private bool _blueReduction = false;
        private readonly Monitors monitors = new Monitors();

        public static ScreenSyncHelper Instance => lazy.Value;

        public Action ScreenUpdated { get; set; }

        public ScreenSyncHelper()
        {
            _mode = new Standard();
            InitMonitorList();
            new Thread(new ThreadStart(CheckMonitorCounts)) { IsBackground = true }.Start();
            _filter = new NoneFilter();
        }

        /// <summary>
        /// Check monitor hotswap
        /// </summary>
        private void CheckMonitorCounts()
        {
            while (true)
            {
                if (monitors.CheckMonitor())
                {
                    InitMonitorList();
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Init monitor list
        /// </summary>
        private void InitMonitorList()
        {
            foreach (DesktopDuplicator d in _desktopDuplicators)
            {
                d.DisposeDx();
            }
            _desktopDuplicators.Clear(); // this will break desktopDuplicator object
            _monitorDictionary = monitors.GetMonitors();
            foreach (var monitor in _monitorDictionary)
            {
                _monitorNameList.Add(monitor.Key);
                if (monitor.Key == "All Screen")
                    continue;
                _desktopDuplicators.Add(new DesktopDuplicator(monitor.Value.Item1, monitor.Value.Item2));
            }
            FlushList?.Invoke();
        }

        /// <summary>
        /// Start Screen Sync
        /// </summary>
        public void Start()
        {
            CancellationToken cancelToken = _cancelSource.Token;
            _ = Task.Run(() => {
                _isProcessing = false;
                lock (_locker)
                {
                    _isProcessing = true;

                    while (_isProcessing)
                    {
                        if (StreamingHelper.Instance.FrameRateContolTimer.IsAvailable())
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                _isProcessing = false;
                                return;
                            }
                            _syncColors = CaptureColors();
                            m_updateHandler?.Invoke();
                        }
                        Thread.Sleep(1);
                    }
                }
            }, cancelToken);
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
        /// Capture color matrix
        /// MEMO: ScreenSync uses desktop duplication, CPU loading is heavy, need to adjust in the future!
        /// </summary>
        /// <returns>Color matrix</returns>
        private ColorRGB[,] CaptureColors()
        {
            switch (_selectedMonitor)
            {
                case "All Screen":
                    try
                    {
                        foreach (var desktopDuplicator in _desktopDuplicators)
                        {
                            Bitmap frame = desktopDuplicator.GetLatestFrameV2();
                            if (frame == null)
                                continue;
                            DesktopDuplicator.G.DrawImage(frame, desktopDuplicator.X, desktopDuplicator.Y);
                        }
                        lastImage = DesktopDuplicator.GetImg;
                        ColorRGB[,] result0 = GetMatrixColors(lastImage);
                        result0 = _filter.Process(result0);
                        return result0;
                    }
                    catch
                    {
                        return null;
                    }
                default:
                    try
                    {
                        if (_desktopDuplicators.Count > select)
                        {
                            lastImage = _desktopDuplicators[select].GetLatestFrameV2();
                            if (lastImage != null)
                            {
                                ColorRGB[,] result = GetMatrixColors(lastImage);
                                result = _filter.Process(result);
                                return result;
                            }
                        }
                        return null;
                    }
                    catch { }
                    break;
            }
            return null;
        }

        public bool GetIsCanUseDXGI()
        {
            try
            {
                _desktopDuplicators[0].TestDXGI();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.HResult == -2005270524)
                    return false;
            }
            return true;
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
        Color[] dominantColors = new Color[8];
        /// <summary>
        /// Get the matrix color from CaptureInfo
        /// </summary>
        /// <param name="image">The screenshot</param>
        /// <returns></returns>
        private ColorRGB[,] GetMatrixColors(Bitmap image)
        {
            ConcurrentDictionary<Color, int> colorCounts = new ConcurrentDictionary<Color, int>();
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
                    colorCounts.AddOrUpdate(color, 1, (_, count) => count + 1);
                    result[y, x] = _mode.GetColors(color.R, color.G, color.B);
                    if (_blueReduction)
                        result[y, x].SetColorFilter(1f, 1f, 0.75f);
                }
            }

            int numberOfColorsToExtract = 8; // Define the number of colors you want to extract

            var sortedColors = colorCounts.OrderByDescending(c => c.Value).Take(numberOfColorsToExtract);

            var newColors = sortedColors.Select(c => c.Key).ToList();

            for (int i = 0; i < dominantColors.Length; i++)
            {
                var index = newColors.FindIndex(x => x == dominantColors[i]);
                if (index < 0)
                {
                    dominantColors[i] = Color.Empty;
                }
                else
                {
                    newColors.RemoveAt(index);
                }
            }

            for(int i = 0; i < newColors.Count; i++)
            {
                if (dominantColors.Contains(Color.Empty))
                {
                    for(int j = 0; j < dominantColors.Length; j++)
                    {
                        if (dominantColors[j] == Color.Empty)
                        {
                            dominantColors[j] = newColors[i];
                            break;
                        }
                    }

                }
                else
                {
                    dominantColors[dominantColors.Length - newColors.Count + i] = newColors[i];
                }
                
            }

            //if (sortedColors.Select(c => c.Key).ToList() != null)
            //{
            //    dominantColors.AddRange(sortedColors.Select(c => c.Key).ToList().DeepClone());
            //    dominantColors.RemoveAt(0);
            //    dominantColors.RemoveAt(0);
            //}
            return result;
        }

        public Color[] GetDominantColors()
        {
            return dominantColors.DeepClone();
        }


        public void UpdateMode(ColorEffectBase mode)
        {
            _mode = mode;
        }

        public void RegisterPreviewUpdateCallback(Action func)
        {
            m_updateHandler = func;
        }

        public void RegisterFlushList(Action flushAction)
        {
            FlushList = flushAction;
        }

        /// <summary>
        /// Get list of monitor names
        /// </summary>
        /// <returns>list of string of monitor names</returns>
        public List<string> GetMonitorList()
        {
            return _monitorNameList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void ChangeMonitor(int index)
        {
            if (index != 0)
            {
                select = index - 1;
            }
            _selectedMonitor = _monitorNameList[index];
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

        public void SetSpeedConfig(int speed)
        {
            //CANNOT BE CONTROLLED
        }

        public void SetBlueFilter(bool _reduction)
        {
            _blueReduction = _reduction;
        }
    }
}