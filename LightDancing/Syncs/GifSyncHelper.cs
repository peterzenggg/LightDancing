using Emgu.CV;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LightDancing.Syncs
{
    public class GifSyncHelper : ISyncHelper
    {
        private static readonly Lazy<GifSyncHelper> lazy = new Lazy<GifSyncHelper>(() => new GifSyncHelper());
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly object _locker = new object();
        private ColorRGB[,] _syncColors;
        private Action m_updateHandler;
        private DurationTimer _durationTimer;
        private bool _isProcessing = false;
        private float speedAdjustment = 1;
        private GifPlayMode _gifPlayMode = GifPlayMode.Looping;
        private float _speedConfig = 1f;
        private bool _blueReduction = false;
        private int _displayFrame = 0;
        private readonly List<ImageFileObject> _playList = new List<ImageFileObject>();

        public static GifSyncHelper Instance => lazy.Value;

        public Action ScreenUpdated { get; set; }

        public GifSyncHelper()
        {
            ScreenInfo.Instance.ResolutionChanged += ReinitList;
        }

        public void RegisterPreviewUpdateCallback(Action func)
        {
            m_updateHandler = func;
        }

        public void SetBlur(ImageFilterBase filter)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            _cancelSource.Cancel();
            lock (_locker)
            {
                _cancelSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// http://giflib.sourceforge.net/whatsinagif/animation_and_transparency.html
        /// </summary>
        public void Start()
        {
            if (_isProcessing)
            {
                Stop();
            }
            ProcessPlaylist();
        }
        
        /// <summary>
        /// update UI and Color
        /// </summary>
        private void ProcessPlaylist()
        {
            PlayListController imageList = new PlayListController(_playList);
            int frameCount = 0;
            _displayFrame = 0;
            frameCount = imageList.GetTotalFrameCount();
            _durationTimer = new DurationTimer((int)(imageList.GetFrameDelay(_displayFrame) * speedAdjustment * _speedConfig));
            bool _isPlayingForward = true;
            CancellationToken cancelToken = _cancelSource.Token;
            _ = Task.Run(() =>
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
                            _syncColors = GetBlackScreen();
                            m_updateHandler?.Invoke();
                            return;
                        }
                        if (_durationTimer.IsAvailable())
                        {
                            /*Change to next selectedFrame*/
                            if (_gifPlayMode == GifPlayMode.Looping)
                            {
                                _displayFrame = (_displayFrame + 1) % frameCount;
                            }
                            else
                            {
                                if (_isPlayingForward)
                                {
                                    _displayFrame++;
                                    if (_displayFrame >= frameCount)
                                    {
                                        _isPlayingForward = false;
                                        _displayFrame = frameCount - 1;
                                    }
                                }
                                else
                                {
                                    _displayFrame--;
                                    if (_displayFrame < 0)
                                    {
                                        _isPlayingForward = true;
                                        _displayFrame = 0;
                                    }
                                }
                            }
                            _durationTimer.UpdateDuration((int)(imageList.GetFrameDelay(_displayFrame) * speedAdjustment * _speedConfig));
                            using Bitmap displayFrame = new Bitmap(imageList.GetFrame(_displayFrame));
                            if (displayFrame != null)
                            {
                                _syncColors = GetFrameColor(displayFrame);//stream save color
                                m_updateHandler?.Invoke();
                            }
                        }
                        Thread.Sleep(1);
                    }
                }
            }, cancelToken);

        }

        /// <summary>
        /// Turn sync off and set the last image to all black
        /// </summary>
        /// <returns>ColorRGB matrix with all black</returns>
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

        public void SetBlueFilter(bool _reduction)
        {
            _blueReduction = _reduction;
        }

        /// <summary>
        /// Reinit play list when change display resolution
        /// </summary>
        public void ReinitList()
        {
            foreach (ImageFileObject obj in _playList)
            {
                switch (obj.Type)
                {
                    case ImageType.Image:
                        obj.InitSingleImage();
                        break;
                    case ImageType.Gif:
                        obj.InitGifFile();
                        break;
                    case ImageType.Video:
                        obj.InitVideo();
                        break;
                }
            }
            _displayFrame = 0;
        }

        /// <summary>
        /// Remove playList[index] from playlist
        /// </summary>
        /// <param name="index">the index of the image to be removed</param>
        public void RemoveImage(int index)
        {
            ImageFileObject imageObject = _playList[index];
            imageObject.Dispose();
            _playList.RemoveAt(index);
            GC.Collect();
        }

        /// <summary>
        /// Change the sequence of image (playList[index]) from index to insert
        /// </summary>
        /// <param name="index">the original index of image</param>
        /// <param name="insert">the new index of image</param>
        public void PlayListInsert(int index, int insert)
        {
            ImageFileObject buffer = _playList[index];
            _playList.RemoveAt(index);
            if (_playList.Count < insert)
            {
                _playList.Add(buffer);
            }
            else
            {
                _playList.Insert(insert, buffer);
            }
        }

        /// <summary>
        /// Read in files and check file size
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="fileName">file name</param>
        /// <param name="imageFile">image</param>
        /// <param name="errorMsg"></param>
        public void ReadFile(string filePath, out string fileName, out Image imageFile, out string errorMsg)
        {
            fileName = Path.GetFileName(filePath);
            errorMsg = "";
            string fileType = Path.GetExtension(filePath).ToLower();
            imageFile = null;

            if (fileType == ".gif" || fileType == ".png" || fileType == ".jpeg" || fileType == ".jpg" || fileType == ".bmp")
            {
                try
                {
                    imageFile = Image.FromFile(filePath);
                    _playList.Add(new ImageFileObject(imageFile, filePath));
                }
                catch
                {
                    imageFile = null;
                    errorMsg = "OpenFail";
                }
            }
            else if (fileType == ".m4v" || fileType == ".mp4" || fileType == ".avi" || fileType == ".flv" || fileType == ".mkv" || fileType == ".mov" || fileType == ".webm" || fileType == ".wmv")
            {
                VideoCapture videoCapture = new VideoCapture(filePath);
                if (videoCapture.IsOpened)
                {
                    Mat mat = new Mat();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            videoCapture.Read(mat);
                        }
                        catch
                        {
                            return;
                        }
                    });

                    /*Need to rewrite*/
                    if (task.Wait(400))
                    {
                        CvInvoke.Resize(mat, mat, new Size(ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight));
                        imageFile = mat.ToBitmap();
                        MemoryStream ms = new MemoryStream();
                        imageFile.Save(ms, ImageFormat.Bmp);
                        double imageBufferLength = ms.Length / 1024F / 1024F;
                        var frameCount = videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                        double totolMemory = imageBufferLength * frameCount * 2;
                        try
                        {
                            //if totolMemory < 1 will cause error
                            if (totolMemory > 1)
                            {
                                System.Runtime.MemoryFailPoint mf = new System.Runtime.MemoryFailPoint((int)totolMemory);
                            }
                            _playList.Add(new ImageFileObject(filePath));
                        }
                        catch
                        {
                            imageFile = null;
                            errorMsg = "Out of Memory";  
                        }
                    }
                    else
                    {
                        errorMsg = "OpenFail";
                        return;
                    }
                }
                else
                {
                    errorMsg = "Video Open Failed";
                    return;
                }
            }
        }

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

        public void SetSpeed(int Index)
        {
            switch (Index)
            {
                case 0:
                    speedAdjustment = 4; //x0.25 => delay = 1 / 0.25 = 4
                    break;
                case 1:
                    speedAdjustment = 2; //x0.5 => delay = 1 / 0.5 = 2
                    break;
                case 2:
                    speedAdjustment = 1.33F; //x0.75
                    break;
                case 3:
                    speedAdjustment = 1; //x1
                    break;
                case 4:
                    speedAdjustment = 0.8F; //x1.25 => delay = 1 / 1.25 = 0.8
                    break;
                case 5:
                    speedAdjustment = 0.6F; //x1.5
                    break;
                case 6:
                    speedAdjustment = 0.57F; //x1.75
                    break;
                case 7:
                    speedAdjustment = 0.5F; //x2
                    break;
            }
        }

        /// <summary>
        /// Image To RGB array
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private ColorRGB[,] GetFrameColor(Bitmap image)
        {
            ColorRGB[,] result = new ColorRGB[image.Height, image.Width];
            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color color = image.GetPixel(w, h);
                    ColorEffectBase _mode = new Standard();
                    result[h, w] = _mode.GetColors(color.R, color.G, color.B);
                    if (_blueReduction)
                        result[h, w].SetColorFilter(1f, 1f, 0.75f);
                }
            }
            return result;
        }

        /// <summary>
        /// Set play mode to looping or bounce
        /// </summary>
        /// <param name="newMode"></param>
        public void SetGifPlayMode(GifPlayMode newMode)
        {
            _gifPlayMode = newMode;
        }

        public void SetSpeedConfig(int speed)
        {
            switch (speed)
            {
                case 0:
                    _speedConfig = 4; //x0.25 => delay = 1 / 0.25 = 4
                    break;
                case 1:
                    _speedConfig = 2; //x0.5 => delay = 1 / 0.5 = 2
                    break;
                case 2:
                    _speedConfig = 1.33F; //x0.75
                    break;
                case 3:
                    _speedConfig = 1; //x1
                    break;
                case 4:
                    _speedConfig = 0.8F; //x1.25 => delay = 1 / 1.25 = 0.8
                    break;
                case 5:
                    _speedConfig = 0.6F; //x1.5
                    break;
                case 6:
                    _speedConfig = 0.57F; //x1.75
                    break;
                case 7:
                    _speedConfig = 0.5F; //x2
                    break;
            }
        }

        public string GetBase64StringColor()
        {
            var colors = GetColors();
            ScreenUpdated?.Invoke();
            return Methods.ColorRgbToBase64(colors);
        }
    }

    /// <summary>
    /// Playlist controller 
    /// </summary>
    public class PlayListController
    {
        private readonly int _totalFrame;
        private readonly List<int> _frameCountList;
        private readonly List<int> _frameDelayList;
        private readonly List<ImageFileObject> _fileObjectList;

        public PlayListController(List<ImageFileObject> fileList)
        {
            _fileObjectList = new List<ImageFileObject>();
            _frameCountList = new List<int>();
            _frameDelayList = new List<int>();
            _totalFrame = 0;

            foreach (ImageFileObject file in fileList)
            {
                if (file.Type == ImageType.Image)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        _fileObjectList.Add(file);
                        file.GetDetail(out int thisFrameCount, out int thisFrameDelay);
                        _totalFrame += thisFrameCount;
                        _frameCountList.Add(_totalFrame);
                        _frameDelayList.Add(thisFrameDelay);
                    }
                }
                else
                {
                    _fileObjectList.Add(file);
                    file.GetDetail(out int thisFrameCount, out int thisFrameDelay);
                    _totalFrame += thisFrameCount;
                    _frameCountList.Add(_totalFrame);
                    _frameDelayList.Add(thisFrameDelay);
                }
            }
        }

        public int GetTotalFrameCount()
        {
            return _totalFrame;
        }

        public int GetFrameDelay(int frame)
        {
            int selectedImage = 0;
            foreach (int frameCount in _frameCountList)
            {
                if (frame < frameCount)
                {
                    break;
                }
                selectedImage++;
            }
            return _frameDelayList[selectedImage];
        }

        public Image GetFrame(int selectedFrame)
        {
            int selectedImage = 0;
            int currentFrame = selectedFrame;
            foreach (int frameCount in _frameCountList)
            {
                if (selectedFrame < frameCount)
                {
                    break;
                }
                currentFrame = selectedFrame - frameCount;
                selectedImage++;
            }
            return _fileObjectList[selectedImage].GetFrameImage(currentFrame);
        }
    }

    /// <summary>
    /// Media file object
    /// </summary>
    public class ImageFileObject : IDisposable
    {
        public ImageType Type { get; set; }
        public string FilePath { get; set; }
        private List<Bitmap> _imageBuffer;
        private int _frameCount;
        private int _frameDelay;
        private bool _cancelThread = false;
        private Thread _thread;
        private readonly FrameDimension _dimension;

        private int FrameDelay
        {
            get
            {
                return _frameDelay;
            }
            set
            {
                if (value > 150 || value <= 0)
                {
                    _frameDelay = 150;
                }
                else
                {
                    _frameDelay = value;
                }
            }
        }
        
        public ImageFileObject(string filePath)
        {
            Type = ImageType.Video;
            FilePath = filePath;
            _cancelThread = false;
            InitVideo();
        }
        
        /// <summary>
        /// Create ImageFileObject for gif and image file
        /// </summary>
        /// <param name="image">image file</param>
        /// <param name="filepath">file path</param>
        public ImageFileObject(Image image, string filepath)
        {
            //ImageFile = image;
            FilePath = filepath;
            _imageBuffer = new List<Bitmap>();

            if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                Type = ImageType.Gif;
                _dimension = new FrameDimension(image.FrameDimensionsList[0]);
                _frameCount = image.GetFrameCount(_dimension);
                MemoryStream memoryStream = new MemoryStream();
                image.Save(memoryStream, image.RawFormat);
                byte[] imgbyte = memoryStream.ToArray();
                byte[] timeArray = new byte[2];
                for (int i = 0; i < imgbyte.Length - 1; i++)
                {
                    if (imgbyte[i] == 33 && imgbyte[i + 1] == 249)
                    {
                        Buffer.BlockCopy(imgbyte, i + 4, timeArray, 0, 2);
                        var delayTime = (int)(timeArray[1] * Math.Pow(16, 2) + timeArray[0]) * 10;
                        FrameDelay = delayTime;
                        break;
                    }
                }
                InitGifFile();
            }
            else
            {
                Type = ImageType.Image;
                InitSingleImage();
            }
        }

        /// <summary>
        /// Init single image file such as png, jpg...
        /// </summary>
        public void InitSingleImage()
        {
            if (_imageBuffer != null)
            {
                _imageBuffer.Clear();
            }
            else
            {
                _imageBuffer = new List<Bitmap>();
            }

            Image image = Image.FromFile(FilePath);
            var buffImg = ResizeImage(image, ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight);
            _frameCount = 1;
            FrameDelay = 1000;
            _imageBuffer.Add((Bitmap)buffImg.Clone());
        }

        /// <summary>
        /// Init gif file
        /// </summary>
        public void InitGifFile()
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    _cancelThread = true;
                    _thread?.Join();
                }
            }

            if (_imageBuffer != null)
            {
                _imageBuffer.Clear();
            }
            else
            {
                _imageBuffer = new List<Bitmap>();
            }
            
            _cancelThread = false;

            Image image = Image.FromFile(FilePath);

            _thread = new Thread(new ThreadStart(() =>
            {
                for (int i = 0; i < _frameCount; i++)
                {
                    image.SelectActiveFrame(_dimension, i);
                    Image bitmap = ResizeImage(image, ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight);
                    _imageBuffer.Add((Bitmap)bitmap);
                    if (_cancelThread) 
                        break;
                }
            })){ IsBackground = true };
            _thread.Start();
        }

        public void InitVideo()
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    _cancelThread = true;
                    _thread?.Join();
                }
            }

            if (_imageBuffer != null)
            {
                _imageBuffer.Clear();
            }
            else
            {
                _imageBuffer = new List<Bitmap>();

            }

            _cancelThread = false;
            VideoCapture videoCapture = new VideoCapture(FilePath);
            _frameCount = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            FrameDelay = (int)(1000 / videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps));

            float nPercentW = ScreenInfo.Instance.ScaleWidth / (float)videoCapture.Width;
            float nPercentH = ScreenInfo.Instance.ScaleHeight / (float)videoCapture.Height;
            float nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;
            int destWidth = (int)(videoCapture.Width * nPercent);
            int destHeight = (int)(videoCapture.Height * nPercent);

            _thread = new Thread(new ThreadStart(() =>
            {
                Mat thisFrame = new Mat();
                while (videoCapture.Grab() && !_cancelThread)
                {
                    videoCapture.Retrieve(thisFrame);
                    CvInvoke.Resize(thisFrame, thisFrame, new Size(destWidth, destHeight));
                    Bitmap bitmap = thisFrame.ToBitmap();
                    bitmap = ToCenter(bitmap, ScreenInfo.Instance.ScaleWidth, ScreenInfo.Instance.ScaleHeight);
                    _imageBuffer.Add(bitmap);
                }
            })){ IsBackground = true };
            _thread.Start();
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap destImage = (Bitmap)image;
            if (image.Width > width || image.Height > height)
            {
                //int sourceWidth = image.Width;
                //int sourceHeight = image.Height;


                //float nPercentW = width / (float)sourceWidth;
                //float nPercentH = height / (float)sourceHeight;
                //float nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

                //int destWidth = (int)(sourceWidth * nPercent);
                //int destHeight = (int)(sourceHeight * nPercent);
                //destImage = new Bitmap(destWidth, destHeight);

                //using Graphics gg = Graphics.FromImage(destImage);
                //gg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //gg.DrawImage(image, 0, 0, destWidth, destHeight);

                int sourceWidth = image.Width;
                int sourceHeight = image.Height;

                float nPercentW = width / (float)sourceWidth;
                float nPercentH = height / (float)sourceHeight;
                float nPercent = nPercentH > nPercentW ? nPercentH : nPercentW;

                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);
                destImage = new Bitmap(destWidth, destHeight);

                using Graphics gg = Graphics.FromImage(destImage);
                gg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gg.DrawImage(image, 0, 0, destWidth, destHeight);
            }

            Bitmap blank = ToCenter(destImage, width, height);
            return blank;
        }

        private Bitmap ToCenter(Image Img, int width, int height)
        {
            ///*Center image with black space*/
            Bitmap blank = new Bitmap(width, height);
            //Img = SetAlpha((Bitmap)Img, 0);
            using (Graphics g = Graphics.FromImage(blank))
            {
                g.Clear(Color.Black);
                g.DrawImageUnscaled(Img, (width - Img.Width) / 2, (height - Img.Height) / 2, Img.Width, Img.Height);
            }
            return blank;
        }

        public Bitmap SetAlpha(Bitmap bmp, byte alpha)
        {
            if (bmp == null) throw new ArgumentNullException("bmp");

            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var line = data.Scan0;
            var eof = line + data.Height * data.Stride;
            while (line != eof)
            {
                var pixelAlpha = line + 3;
                var eol = pixelAlpha + data.Width * 4;
                while (pixelAlpha != eol)
                {
                    System.Runtime.InteropServices.Marshal.WriteByte(
                        pixelAlpha, alpha);
                    pixelAlpha += 4;
                }
                line += data.Stride;
            }
            bmp.UnlockBits(data);

            return bmp;
        }


        public void GetDetail(out int frameCount, out int frameDelay)
        {
            frameCount = _frameCount;
            frameDelay = FrameDelay;
        }

        /// <summary>
        /// get single selectedFrame
        /// </summary>
        public Image GetFrameImage(int index)
        {
            if (index < _imageBuffer.Count)
            {
                return _imageBuffer[index];
            }
            else
            {
                while (_imageBuffer.Count == 0)
                {
                    Thread.Sleep(100);
                }
                return _imageBuffer[_imageBuffer.Count - 1];
            }
        }

        /// <summary>
        /// release image memory
        /// </summary>
        public void Dispose()
        {
            _cancelThread = true;
            _thread?.Join();
            if (_imageBuffer != null)
            {
                for (int i = 0; i < _imageBuffer.Count; i++)
                {
                    _imageBuffer[i].Dispose();
                }
                _imageBuffer.Clear();
            }
        }
    }

    public enum ImageType
    {
        Image,
        Gif,
        Video,
    }
}
