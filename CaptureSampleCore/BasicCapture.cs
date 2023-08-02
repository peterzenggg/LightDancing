////  ---------------------------------------------------------------------------------
////  Copyright (c) Microsoft Corporation.  All rights reserved.
//// 
////  The MIT License (MIT)
//// 
////  Permission is hereby granted, free of charge, to any person obtaining a copy
////  of this software and associated documentation files (the "Software"), to deal
////  in the Software without restriction, including without limitation the rights
////  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////  copies of the Software, and to permit persons to whom the Software is
////  furnished to do so, subject to the following conditions:
//// 
////  The above copyright notice and this permission notice shall be included in
////  all copies or substantial portions of the Software.
//// 
////  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
////  THE SOFTWARE.
////  ---------------------------------------------------------------------------------

using Composition.WindowsRuntimeHelpers;
using System;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;


namespace CaptureSampleCore
{
    public class BasicCapture : IDisposable
    {
        private System.Drawing.Bitmap finalImage1, finalImage2;
        private bool isFinalImage1 = false;
        private GraphicsCaptureItem item;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private SizeInt32 lastSize;
        private IDirect3DDevice device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.Texture2DDescription txt2d;
        private SharpDX.Direct3D11.Texture2D T2D;

        public System.Drawing.Bitmap FinalImage
        {

            get
            {
                return isFinalImage1 ? finalImage1 : finalImage2;
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
        public BasicCapture(IDirect3DDevice d, GraphicsCaptureItem i)
        {
            item = i;
            device = d;
            d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);

            var dxgiFactory = new SharpDX.DXGI.Factory2();

            framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                i.Size);
            session = framePool.CreateCaptureSession(i);
            
            lastSize = i.Size;

            framePool.FrameArrived += OnFrameArrived;
            txt2d = new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = item.Size.Width,
                Height = item.Size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Usage = SharpDX.Direct3D11.ResourceUsage.Staging,
                SampleDescription = { Count = 1, Quality = 0 },
                BindFlags = SharpDX.Direct3D11.BindFlags.None,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
            };
            T2D = new SharpDX.Direct3D11.Texture2D(d3dDevice, txt2d);
        }
       
        public void Dispose()
        {
            session?.Dispose();
            framePool?.Dispose();
            d3dDevice?.Dispose();
            d3dDevice = null;
        }

        public void StartCapture()
        {
            session.StartCapture();
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            try
            {
                using (var frame = sender.TryGetNextFrame())
                {
                    using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                    {
                        d3dDevice.ImmediateContext.CopyResource(bitmap, T2D);
                    }

                } // Retire the frame.
            }
            catch { };
        }
        /// <summary>
        /// Convert Frame To Bitmap
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Bitmap GetBitmap()
        {
            try
            {
                var mapSource = d3dDevice.ImmediateContext.MapSubresource(T2D, 0, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None,
out SharpDX.DataStream stream);
                FinalImage = new System.Drawing.Bitmap(lastSize.Width, lastSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                var boundsRect = new System.Drawing.Rectangle(0, 0, lastSize.Width, lastSize.Height);
                var mapDest = FinalImage.LockBits(boundsRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, FinalImage.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < lastSize.Height; y++)
                {
                    // Copy a single line 
                    SharpDX.Utilities.CopyMemory(destPtr, sourcePtr, lastSize.Width * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }
                FinalImage.UnlockBits(mapDest);
                if(d3dDevice!=null)
                    d3dDevice.ImmediateContext?.UnmapSubresource(T2D, 0);
                return (System.Drawing.Bitmap)FinalImage.Clone();
            }
            catch
            {
                return null;
            }
        }
    }
}
