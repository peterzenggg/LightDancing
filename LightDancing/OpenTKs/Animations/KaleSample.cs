using Emgu.CV;
using LightDancing.Colors;
using LightDancing.Common;
using LightDancing.Enums;
using LightDancing.Syncs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playlists;

namespace LightDancing.OpenTKs.Animations
{
    public class KaleSample : GameWindow
    {
        private readonly float[] _vertices =
        {
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            1.0f, 1.0f, 0.0f
        };

        private int _vertexBufferObjsct;
        private int _vertexArrayObjsct;
        private Shader _shader;
        private double _stream = 1.0;
        private byte[] _pixels;
        private int _xAxisSize;
        private int _yAxisSize;
        private Converter _converter;

        private event Action ShaderChangedEvent;
        private bool _shaderChanged;

        private DateTime _timer;

        private readonly AnimationBase animation;
        private const string VERT_PATH = "LightDancing.OpenTKs.Shaders.Canvas.vert";
        private AnimationGroup _animationGroup;
        private AnimationStatus _animationStatus;
        private bool isReady = false;
        private readonly Action _updateHandler;
        private float _speedParameter = 1;
        private int _noiseFactor = 0;
        private Bitmap bitmap = null;
        private object _locker = new object();

        public AnimationGroup ShaderGroupMode
        {
            get { return _animationGroup; }
            set
            {
                _animationGroup = value;
                ShaderChangedEvent?.Invoke();
            }
        }

        /// <summary>
        /// Ref from https://github.com/opentk/LearnOpenTK/blob/master/Chapter1/3-ElementBufferObjects/Window.cs
        /// </summary>
        /// <param name="gameWindowSettings">GameWindowSettings</param>
        /// <param name="nativeWindowSettings">NativeWindowSettings</param>
        /// <param name="shaderType">shaderType</param>
        public KaleSample(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, AnimationGroup animationGroup, OpentkParameters opentkParameters, Action m_updateHandler) : base(gameWindowSettings, nativeWindowSettings)
        {
            _xAxisSize = nativeWindowSettings.Size[0];
            _yAxisSize = nativeWindowSettings.Size[1];
            _pixels = new byte[_xAxisSize * _yAxisSize * ConstValues.RGBA_BYTE];
            _converter = new Converter(_xAxisSize, _yAxisSize);
            _animationGroup = animationGroup;


            _animationStatus = GetAnimationStatus(_animationGroup);

            ShaderChangedEvent += ShaderChanged;

            _updateHandler = m_updateHandler;
            animation = opentkParameters.Mode switch
            {
                OpenTKMode.MusicSync => new SmartMusic(opentkParameters.IsAuto, opentkParameters.CustomGenres),
                OpenTKMode.Wallpaper => new Wallpaper(opentkParameters.Wallpapers, opentkParameters.Shuffle),
                OpenTKMode.BeatAnalysis => new BeatAnalysis(opentkParameters.IsAuto, opentkParameters.CustomGenres),
                OpenTKMode.Animate => new Animate(opentkParameters.Animate),
                _ => throw new ArgumentException(string.Format(ErrorWords.ENUM_NOT_HANDLE, opentkParameters.Mode, "KaleSample")),
            };

            //GetFishyBitmaps();
        }

        /// <summary>
        /// Allows OpentkHelper to change MLs path
        /// </summary>
        /// <param name="path"></param>
        public void SetMLsFilePath(string path)
        {
            if (animation.GetType() == typeof(SmartMusic))
                ((SmartMusic)animation).SetMLsFilePath(path);
        }

        /// <summary>
        /// Stop FFT 
        /// </summary>
        internal void StopFFT()
        {
            if (animation.GetType() == typeof(SmartMusic))
            {
                ((SmartMusic)animation).Dispose();
            }
            if (animation.GetType() == typeof(BeatAnalysis))
            {
                ((BeatAnalysis)animation).Dispose();
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _vertexBufferObjsct = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjsct);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObjsct = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObjsct);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader = new Shader(VERT_PATH, _animationGroup, _animationStatus);
            _shader.Use();
            _timer = DateTime.Now;

            isReady = true;
        }

        /// <summary>
        /// Thread always sleep 1ms to improve CPU usage
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            var timer = (DateTime.Now - _timer).TotalMilliseconds;
            if (timer > 15 && isReady) // the frequency of update timer depend on 1/10 of screen size 
            {
                _timer = DateTime.Now;
                base.OnRenderFrame(args);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                //_stream = _stream + 1 > 40000 ? 0 : _stream + 1;
                _stream = _stream + _speedParameter > double.MaxValue ? 0 : _stream + _speedParameter;
                if (_noiseFactor > 0)
                {
                    float noise = PerlinNoise((float)_stream, _noiseFactor);
                    _stream = _stream + noise > double.MaxValue ? 0 : _stream + noise;
                }

                int resolutionLocation = GL.GetUniformLocation(_shader.Handle, "u_resolution");
                GL.Uniform2(resolutionLocation, (float)_xAxisSize, (float)_yAxisSize); // the shader must receive float

                int streamLocation = GL.GetUniformLocation(_shader.Handle, "u_time");
                GL.Uniform1(streamLocation, (float)(_stream / 100.0));

                animation.SendParameterToGL(_shader, _stream, _animationGroup);

                _shader.Use();
                GL.BindVertexArray(_vertexArrayObjsct);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                /*GrabScreen before SwapBuffers*/
                lock (_locker)
                {
                    bitmap = GrabScreenshot();
                }
                SwapBuffers();

                //GL.ReadPixels(0, 0, _xAxisSize, _yAxisSize, PixelFormat.Rgba, PixelType.UnsignedByte, _pixels);
                _updateHandler?.Invoke();
            }
            Thread.Sleep(1);
        }

        private Bitmap GrabScreenshot()
        {
            Bitmap bmp = new Bitmap(Size.X, Size.Y);
            Rectangle rect = new Rectangle(0, 0, _xAxisSize, _yAxisSize);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                             System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            GL.ReadPixels(0, 0, _xAxisSize, _yAxisSize, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        public static float PerlinNoise(float time, int chaotic)
        {
            float remapTime = time / 30;
            Random random = new Random((int)remapTime);
            float x_1 = random.Next(chaotic);
            random = new Random((int)remapTime + 1);
            float x_2 = random.Next(chaotic);
            float remapTimeFract = remapTime - (int)remapTime;
            float u = remapTimeFract * remapTimeFract * (3 - 2 * remapTimeFract);
            float x = x_1 + (x_2 - x_1) * u;

            return x / chaotic * 10;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (animation.GetType() == typeof(Wallpaper))
            {
                ((Wallpaper)animation).ChangeWallpaper();
            }

            if (animation.GetType() == typeof(Animate))
            {
                ((Animate)animation).UpdateWallpaper();
            }

            if (_shaderChanged)
            {
                _animationStatus = GetAnimationStatus(_animationGroup);
                _shader = new Shader(VERT_PATH, _animationGroup, _animationStatus);
                _shaderChanged = false;
            }
            Thread.Sleep(1);
        }

        private void ShaderChanged()
        {
            _shaderChanged = true;
        }

        public void SetColor(List<ColorRGB> colors)
        {
            animation?.SetColor(colors);
        }

        public void ResetFrequencyBand()
        {
            if (animation.GetType() == typeof(SmartMusic))
            {
                ((SmartMusic)animation).ResetFrequencyBand();
            }
            else
            {
                Debug.WriteLine("ResetFrequencyBand only support with SmartMusic");
            }
        }

        public void SetSpeed(int ms)
        {
            if (animation.GetType() == typeof(SmartMusic))
            {
                ((SmartMusic)animation).SetSpeed(ms);
            }
            else
            {
                Debug.WriteLine("SetSpeed only support with SmartMusic");
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            _xAxisSize = Size.X;
            _yAxisSize = Size.Y;
            _pixels = new byte[_xAxisSize * _yAxisSize * ConstValues.RGBA_BYTE];
            _converter = new Converter(_xAxisSize, _yAxisSize);
        }

        /// <summary>
        /// Convert the current buffer to color matrix
        /// Return null, OpenTkHelper use bitmap for result color
        /// </summary>
        /// <returns></returns>
        public ColorRGB[,] GetColors()
        {
            return null;
        }

        private Bitmap result = null;
        public Bitmap GetBitmap()
        {
            lock (_locker)
            {
                try
                {
                    if (bitmap != null)
                    {
                        result = new Bitmap(bitmap);
                        //result = StackFishOnOutput(beforeFish);
                        //result = beforeFish;
                    }
                }
                catch { }
                return result;
            }
        }

        Bitmap resultStreaming = null;
        public Bitmap GetBitmapForStreaming()
        {
            lock (_locker)
            {
                try
                {
                    if (bitmap != null)
                    {
                        resultStreaming = new Bitmap(bitmap);
                        //resultStreaming = StackFishOnOutput(beforeFish);
                        //resultStreaming = (Bitmap)beforeFish.Clone();
                    }
                }
                catch { }
                return resultStreaming;
            }
        }


        Bitmap resultStreaming2 = null;
        public Bitmap GetBitmapForAlphaVideo()
        {
            lock (_locker)
            {
                try
                {
                    if (bitmap != null)
                    {
                        resultStreaming2 = new Bitmap(bitmap);
                        //resultStreaming = StackFishOnOutput(beforeFish);
                        //resultStreaming = (Bitmap)beforeFish.Clone();
                    }
                }
                catch { }
                return resultStreaming2;
            }
        }

        private List<Bitmap> _stack = new List<Bitmap>();
        private int count = 0;
        private void GetFishyBitmaps()
        {
            for (int i = 1; i < 361; i++)
            {
                string filePath = @"C:\Users\Jasmine Tseng\Desktop\IBP\Q60\LCD\Videos\Vertical Infinite\Vertical Infinite_00";
                if (i < 10)
                {
                    filePath = filePath + "00" + i + ".png";
                }
                else if(i < 100) 
                {
                    filePath = filePath + "0" + i + ".png";
                }
                else
                {
                    filePath = filePath + i + ".png";
                }
                Image image = Image.FromFile(filePath);

                _stack.Add((Bitmap)image);
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap destImage = (Bitmap)image;
            //if (image.Width > width || image.Height > height)
            {
                int sourceWidth = image.Width;
                int sourceHeight = image.Height;


                float nPercentW = width / (float)sourceWidth;
                float nPercentH = height / (float)sourceHeight;

                int destWidth = (int)(sourceWidth * nPercentW);
                int destHeight = (int)(sourceHeight * nPercentH);
                destImage = new Bitmap(destWidth, destHeight);

                using Graphics gg = Graphics.FromImage(destImage);
                gg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gg.DrawImage(image, 0, 0, destWidth, destHeight);
            }

            return destImage;
        }

        public Bitmap StackFishOnOutput(Bitmap bitmap)
        {
            count++;
            if(count >= _stack.Count)
            {
                count = 0;
            }

            Bitmap resultBitmap = new Bitmap(_stack[count].Width, _stack[count].Height);

            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                bitmap = ResizeImage(bitmap, _stack[count].Width, _stack[count].Height);
                // Draw the first bitmap onto the result bitmap
                g.DrawImage(bitmap, 0, 0);

                // Set the alpha blending mode to combine the images
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                // Draw the second bitmap onto the result bitmap with transparency
                g.DrawImage(_stack[count], 0, 0);
            }

            return resultBitmap;
        }

        public void ChangeAnimate(string animate)
        {
            if (animation.GetType() == typeof(Animate))
            {
                ((Animate)animation).ChangeWallpaper(animate);
            }
        }

        public void SetHue(List<ColorRGB> colors)
        {
            animation?.SetColor(colors);
        }

        /// <summary>
        /// Normal speed add "1" for each cycle, set _speedParameter to different parameter to change speed
        /// </summary>
        /// <param name="index"></param>
        public void SetSpeed(float index)
        {
            _speedParameter = index;

        }

        public void SetSat(List<ColorRGB> colors)
        {
            animation?.SetColor(colors);
        }

        /// <summary>
        /// The percenatge of noise, normal is set to 0, means no noise
        /// </summary>
        /// <param name="index">Range is from 0 to 1, 0 means no noise</param>
        public void SetNoise(float index)
        {
            _noiseFactor = (int)(index * 30);
        }

        //Cause error for now, but might need it in the future
        //protected override void OnUnload()
        //{
        //    // Unbind all the resources by binding the targets to 0/null.
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //    GL.BindVertexArray(0);
        //    GL.UseProgram(0);

        //    // Delete all the resources.
        //    GL.DeleteBuffer(_vertexBufferObjsct);
        //    GL.DeleteVertexArray(_vertexArrayObjsct);

        //    GL.DeleteProgram(_shader.Handle);

        //    base.OnUnload();
        //}

        public AnimationStatus GetAnimationStatus(AnimationGroup animationGroup)
        {
            if (animationGroup.Background != null && animationGroup.Panorama != null && animationGroup.Protagonist != null && animationGroup.Kick != null)
            {
                return AnimationStatus.FourBands;
            }
            else if (animationGroup.Background != null && animationGroup.Panorama == null && animationGroup.Protagonist == null)
            {
                return AnimationStatus.BackgroundOnly;
            }
            else if (animationGroup.Background == null && animationGroup.Panorama != null && animationGroup.Protagonist == null)
            {
                return AnimationStatus.PanoramaOnly;
            }
            else if (animationGroup.Background == null && animationGroup.Panorama == null && animationGroup.Protagonist != null)
            {
                return AnimationStatus.ProtagonistOnly;
            }
            else if (animationGroup.Background != null && animationGroup.Panorama != null && animationGroup.Protagonist == null)
            {
                return AnimationStatus.BackgroundAndPanorama;
            }
            else if (animationGroup.Background != null && animationGroup.Panorama == null && animationGroup.Protagonist != null)
            {
                return AnimationStatus.BackgroundAndProtagonist;
            }
            else if (animationGroup.Background == null && animationGroup.Panorama != null && animationGroup.Protagonist != null)
            {
                return AnimationStatus.PanoramaAndProtagonist;
            }
            else if (animationGroup.Background != null && animationGroup.Panorama != null && animationGroup.Protagonist != null)
            {
                return AnimationStatus.All;
            }
            else if (animationGroup.Background == null && animationGroup.Panorama == null && animationGroup.Protagonist == null && animationGroup.Kick == null && animationGroup.WallpaperAnimate != null)
            {
                return AnimationStatus.WallpaperAnimate;
            }
            else if (animationGroup.Background == null && animationGroup.Panorama == null && animationGroup.Protagonist == null && animationGroup.Kick == null && animationGroup.WallpaperAnimate == null && animationGroup.MusicAnimatePath != null)
            {
                return AnimationStatus.MusicAnimate;
            }
            else
            {
                return AnimationStatus.NA;
            }
        }
    }
}
