using System;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = SharpDX.Mathematics.Interop.RawRectangle;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DesktopDuplication
{
    /// <summary>
    /// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
    /// </summary>
    public class DesktopDuplicator
    {
        private Device mDevice;
        private Texture2DDescription mTextureDesc;
        private OutputDescription mOutputDesc;
        private OutputDuplication mDeskDupl;

        private Texture2D desktopImageTexture = null;
        private OutputDuplicateFrameInformation frameInfo = new OutputDuplicateFrameInformation();
        private int mWhichOutputDevice = -1;
        private object Locker = new object();

        private Bitmap finalImage1, finalImage2;
        private bool isFinalImage1 = false;
        public int X
        {
            get
            {

                if (minX < 0)
                {
                    return mOutputDesc.DesktopBounds.Left - minX;
                }
                return mOutputDesc.DesktopBounds.Left;
            }
        }
        public int Y
        {
            get
            {
                if (minY < 0)
                {
                    return mOutputDesc.DesktopBounds.Top - minY;
                }
                return mOutputDesc.DesktopBounds.Top;
            }
        }
        private static Bitmap bigimg;
        public static Graphics G;
        public static Bitmap GetImg
        {
            get
            {
                return bigimg;
            }
        }


        public Bitmap FinalImage
        {

            get
            {
                lock (Locker)
                {
                    return isFinalImage1 ? finalImage1 : finalImage2;
                }
            }
            set
            {
                if (isFinalImage1)
                {
                    finalImage2 = value;
                    if (finalImage1 != null) finalImage1.Dispose();
                }
                else
                {
                    finalImage1 = value;
                    if (finalImage2 != null) finalImage2.Dispose();
                }
                isFinalImage1 = !isFinalImage1;
            }
        }

        /// <summary>
        /// Duplicates the output of the specified monitor.
        /// </summary>
        /// <param name="whichMonitor">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichMonitor)
            : this(0, whichMonitor) { }

        /// <summary>
        /// Duplicates the output of the specified monitor on the specified graphics adapter.
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
        /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        int whichGraphicsCardAdapter;
        int whichOutputDevice;


        public DesktopDuplicator(int whichGraphicsCardAdapter, int whichOutputDevice)
        {
            this.whichGraphicsCardAdapter = whichGraphicsCardAdapter;
            this.whichOutputDevice = whichOutputDevice;
            this.mWhichOutputDevice = whichOutputDevice;
            Adapter1 adapter = null;
            try
            {
                adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
            }
            this.mDevice = new Device(adapter);
            Output output = null;
            try
            {
                output = adapter.GetOutput(whichOutputDevice);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified output device.");
            }
            var output1 = output.QueryInterface<Output1>();
            this.mOutputDesc = output.Description;
            var desktopBoundsWidth = this.mOutputDesc.DesktopBounds.Right - this.mOutputDesc.DesktopBounds.Left;
            var desktopBoundsHeight = this.mOutputDesc.DesktopBounds.Bottom - this.mOutputDesc.DesktopBounds.Top;
            ComputScreenSize(mOutputDesc.DesktopBounds);
            this.mTextureDesc = new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = desktopBoundsWidth,
                Height = desktopBoundsHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            try
            {
                this.mDeskDupl = output1.DuplicateOutput(mDevice);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                {
                    throw new DesktopDuplicationException("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                }
            }
        }
        /// <summary>
        /// ComputAllScreen size in Image.
        /// </summary>

        static int maxX;
        static int maxY;
        static int minX;
        static int minY;
        static int Width;
        static int heigth;
        void ComputScreenSize(Rectangle DesktopBounds)
        {
            var desktopBoundsWidth = DesktopBounds.Right - DesktopBounds.Left;
            var desktopBoundsHeight = DesktopBounds.Bottom - DesktopBounds.Top;
            if (minX > DesktopBounds.Left)
            {
                minX = DesktopBounds.Left;
            }
            if ((DesktopBounds.Left + desktopBoundsWidth) > maxX)
            {
                maxX = DesktopBounds.Left + desktopBoundsWidth;
            }
            if (minY > DesktopBounds.Top)
            {
                minY = DesktopBounds.Top;
            }
            if ((DesktopBounds.Top + desktopBoundsHeight) > maxY)
            {
                maxY = DesktopBounds.Top + desktopBoundsHeight;
            }
            Width = maxX - minX;
            heigth = maxY - minY;
            bigimg = new Bitmap(Width, heigth);
            G = Graphics.FromImage(bigimg);
        }
        public static bool Isbreak = false;
        public Bitmap GetLatestFrameV2()
        {
            if (Isbreak)
            {
                throw new Exception("break");
            }
            bool retrievalTimedOut = RetrieveFrame();
            if (retrievalTimedOut)
                return (Bitmap)FinalImage.Clone();
            try
            {
                 ProcessFrame();
            }
            catch
            {
                ReleaseFrame();
            }
            try
            {
                ReleaseFrame();
            }
            catch
            {
                //    throw new DesktopDuplicationException("Couldn't release frame.");  
            }
            return (Bitmap)FinalImage.Clone();
        }
        public void TestDXGI()
        {
            Adapter1 adapter;
            try
            {
                adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
            }
            Output output;
            try
            {
                output = adapter.GetOutput(whichOutputDevice);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified output device.");
            }
            Result reason = mDevice.DeviceRemovedReason;
            if (reason.Failure)
            {
                Console.WriteLine($"mDevice.DeviceRemovedReason is faile, {reason}");
                throw new Exception($"mDevice.DeviceRemovedReason is faile, {reason}");
            }
            var output1 = output.QueryInterface<Output1>();
            output1.DuplicateOutput(mDevice);
        }
        private bool RetrieveFrame()
        {
            if (desktopImageTexture == null)
                try
                {
                    desktopImageTexture = new Texture2D(mDevice, mTextureDesc);
                }
                catch
                {
                    Isbreak = true;
                    return true;
                }
            SharpDX.DXGI.Resource desktopResource = null;
            frameInfo = new OutputDuplicateFrameInformation();
            try
            {
                if (mDeskDupl == null)
                {
                    Adapter1 adapter = null;
                    try
                    {
                        adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
                    }
                    catch (SharpDXException)
                    {
                        throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
                    }
                    Output output = null;
                    try
                    {
                        output = adapter.GetOutput(whichOutputDevice);
                    }
                    catch (SharpDXException)
                    {
                        throw new DesktopDuplicationException("Could not find the specified output device.");
                    }
                    try
                    {
                        var output1 = output.QueryInterface<Output1>();
                        this.mDeskDupl = output1.DuplicateOutput(mDevice);
                    }
                    catch (SharpDXException ex)
                    {
                        if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                        {
                            throw new DesktopDuplicationException("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                        }
                    }
                }
                mDeskDupl.AcquireNextFrame(1, out frameInfo, out desktopResource);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    return true;
                }
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                {
                    Console.WriteLine($"ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code, set mDeskDupl to null");
                    mDeskDupl = null;
                    desktopResource?.Dispose();
                    return true;
                }
                if (ex.ResultCode.Failure)
                {
                    Isbreak = true;
                    throw new DesktopDuplicationException("Failed to acquire next frame.");
                }
            }
            using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
                mDevice.ImmediateContext.CopyResource(tempTexture, desktopImageTexture);
            desktopResource?.Dispose();
            return false;
        }

        private void RetrieveFrameMetadata(DesktopFrame frame)
        {

            if (frameInfo.TotalMetadataBufferSize > 0)
            {
                // Get moved regions
                OutputDuplicateMoveRectangle[] movedRectangles = new OutputDuplicateMoveRectangle[frameInfo.TotalMetadataBufferSize];
                mDeskDupl.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out int movedRegionsLength);
                frame.MovedRegions = new MovedRegion[movedRegionsLength / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle))];
                for (int i = 0; i < frame.MovedRegions.Length; i++)
                {
                    var movedRectanglesWidth = movedRectangles[i].DestinationRect.Right - movedRectangles[i].DestinationRect.Left;
                    var movedRectanglesHeight = movedRectangles[i].DestinationRect.Bottom - movedRectangles[i].DestinationRect.Top;
                    frame.MovedRegions[i] = new MovedRegion()
                    {
                        Source = new System.Drawing.Point(movedRectangles[i].SourcePoint.X, movedRectangles[i].SourcePoint.Y),
                        Destination = new System.Drawing.Rectangle(movedRectangles[i].DestinationRect.Left, movedRectangles[i].DestinationRect.Top, movedRectanglesWidth, movedRectanglesHeight)
                    };
                }

                // Get dirty regions
                Rectangle[] dirtyRectangles = new Rectangle[frameInfo.TotalMetadataBufferSize];
                mDeskDupl.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out int dirtyRegionsLength);
                frame.UpdatedRegions = new System.Drawing.Rectangle[dirtyRegionsLength / Marshal.SizeOf(typeof(Rectangle))];
                for (int i = 0; i < frame.UpdatedRegions.Length; i++)
                {
                    var dirtyRectanglesWidth = dirtyRectangles[i].Right - dirtyRectangles[i].Left;
                    var dirtyRectanglesHeight = dirtyRectangles[i].Bottom - dirtyRectangles[i].Top;
                    frame.UpdatedRegions[i] = new System.Drawing.Rectangle(dirtyRectangles[i].Left, dirtyRectangles[i].Top, dirtyRectanglesWidth, dirtyRectanglesHeight);
                }
            }
            else
            {
                frame.MovedRegions = new MovedRegion[0];
                frame.UpdatedRegions = new System.Drawing.Rectangle[0];
            }
        }

        private void RetrieveCursorMetadata(DesktopFrame frame)
        {
            var pointerInfo = new PointerInfo();

            // A non-zero mouse update timestamp indicates that there is a mouse position update and optionally a shape change
            if (frameInfo.LastMouseUpdateTime == 0)
                return;

            bool updatePosition = true;

            // Make sure we don't update pointer position wrongly
            // If pointer is invisible, make sure we did not get an update from another output that the last time that said pointer
            // was visible, if so, don't set it to invisible or update.

            if (!frameInfo.PointerPosition.Visible && (pointerInfo.WhoUpdatedPositionLast != this.mWhichOutputDevice))
                updatePosition = false;

            // If two outputs both say they have a visible, only update if new update has newer timestamp
            if (frameInfo.PointerPosition.Visible && pointerInfo.Visible && (pointerInfo.WhoUpdatedPositionLast != this.mWhichOutputDevice) && (pointerInfo.LastTimeStamp > frameInfo.LastMouseUpdateTime))
                updatePosition = false;

            // Update position
            if (updatePosition)
            {
                pointerInfo.Position = new SharpDX.Mathematics.Interop.RawPoint(frameInfo.PointerPosition.Position.X, frameInfo.PointerPosition.Position.Y);
                pointerInfo.WhoUpdatedPositionLast = mWhichOutputDevice;
                pointerInfo.LastTimeStamp = frameInfo.LastMouseUpdateTime;
                pointerInfo.Visible = frameInfo.PointerPosition.Visible;
            }

            // No new shape
            if (frameInfo.PointerShapeBufferSize == 0)
                return;

            if (frameInfo.PointerShapeBufferSize > pointerInfo.BufferSize)
            {
                pointerInfo.PtrShapeBuffer = new byte[frameInfo.PointerShapeBufferSize];
                pointerInfo.BufferSize = frameInfo.PointerShapeBufferSize;
            }

            try
            {
                unsafe
                {
                    fixed (byte* ptrShapeBufferPtr = pointerInfo.PtrShapeBuffer)
                    {
                        mDeskDupl?.GetFramePointerShape(frameInfo.PointerShapeBufferSize, (IntPtr)ptrShapeBufferPtr, out pointerInfo.BufferSize, out pointerInfo.ShapeInfo);
                    }
                }
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to get frame pointer shape.");
                }
            }

            //frame.CursorVisible = pointerInfo.Visible;
            frame.CursorLocation = new Point(pointerInfo.Position.X, pointerInfo.Position.Y);
        }

        private void ProcessFrame()
        {
            // Get the desktop capture texture
            var mapSource = mDevice.ImmediateContext.MapSubresource(desktopImageTexture, 0, MapMode.Read, MapFlags.None);
            lock (Locker)
            {
                var desktopBoundsWidth = mOutputDesc.DesktopBounds.Right - mOutputDesc.DesktopBounds.Left;
                var desktopBoundsHeight = mOutputDesc.DesktopBounds.Bottom - mOutputDesc.DesktopBounds.Top;
                FinalImage = new System.Drawing.Bitmap(desktopBoundsWidth, desktopBoundsHeight, PixelFormat.Format32bppRgb);
                var boundsRect = new System.Drawing.Rectangle(0, 0, desktopBoundsWidth, desktopBoundsHeight);
                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = FinalImage.LockBits(boundsRect, ImageLockMode.WriteOnly, FinalImage.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < desktopBoundsHeight; y++)
                {
                    // Copy a single line 
                    Utilities.CopyMemory(destPtr, sourcePtr, desktopBoundsWidth * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                // Release source and dest locks
                FinalImage.UnlockBits(mapDest);
            }
            mDevice.ImmediateContext.UnmapSubresource(desktopImageTexture, 0);
        }

        private void ReleaseFrame()
        {
            try
            {
                mDeskDupl?.ReleaseFrame();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    //throw new DesktopDuplicationException("Failed to release frame.");
                }
            }
        }
        public void DisposeDx()
        {
            maxX = 0;
            maxY = 0;
            minX = 0;
            minY = 0;
            Isbreak = false;
            mDevice?.Dispose();
            mDevice = null;
            mDeskDupl?.Dispose();
            mDeskDupl = null;
            desktopImageTexture?.Dispose();
            desktopImageTexture = null;
        }
    }
}

