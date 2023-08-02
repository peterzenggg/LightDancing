using Composition.WindowsRuntimeHelpers;
using System;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;

namespace CaptureSampleCore
{
    public class BasicApplication : IDisposable
    {
        private IDirect3DDevice device;
        private BasicCapture capture;
        private string Name;
        public BasicApplication()
        {
            device = Direct3D11Helper.CreateDevice();
        }
        public void StartCaptureFromItem(IntPtr hmon)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(hmon);
            if (capture == null || Name != item.DisplayName)
            {
                StopCapture();
                capture = new BasicCapture(device, item);
                capture.StartCapture();
                Name = item.DisplayName;
            }
        }
        public System.Drawing.Bitmap GetCaptureBitmap()
        {
            return capture?.GetBitmap();
        }

        public bool SupportCapture()
        {
            return GraphicsCaptureSession.IsSupported();
        }

        public void StopCapture()
        {
            capture?.Dispose();
            capture = null;
        }
        public void Dispose()
        {
            capture?.Dispose();
            device?.Dispose();
        }

    }
}
