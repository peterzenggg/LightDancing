using HidSharp;
using LightDancing.Colors;
using LightDancing.Enums;
using System;
using System.Collections.Generic;

namespace LightDancing.Hardware.Devices
{
    public abstract class USBDeviceBase : IDisposable
    {
        private readonly object locker = new object();

        protected List<LightingBase> _lightingBase;

        protected readonly DeviceStream _deviceStream;

        protected readonly DeviceStream _scrollStream;

        protected HardwareModel _model;

        protected MiniHubLayout _hubLayout;

        protected string _serialID;

        private bool _disposed = false;

        /// <summary>
        /// Send streaming commands to devices
        /// </summary>
        /// <param name="process">True for streaming, False for Static color</param>
        protected abstract void SendToHardware(bool process, float brightness);

        public USBDeviceBase()
        {
            _model = InitModel();
        }

        /// <summary>
        /// InitModel needs to be before InitDevice
        /// </summary>
        /// <param name="deviceStream"></param>
        public USBDeviceBase(DeviceStream deviceStream)
        {
            _deviceStream = deviceStream;
            _model = InitModel();
            _lightingBase = InitDevice();
        }

        public USBDeviceBase(DeviceStream deviceStream, string serialID)
        {
            _serialID = serialID;
            _deviceStream = deviceStream;
            _model = InitModel();
            _lightingBase = InitDevice();
        }

        public USBDeviceBase(DeviceStream deviceStream, string serialID, MiniHubLayout minihubLayout)
        {
            _serialID = serialID;
            _deviceStream = deviceStream;
            _hubLayout = minihubLayout;
            _model = InitModel();
            _lightingBase = InitDevice();
        }

        /// <summary>
        /// For HeatMoving Keeb
        /// </summary>
        /// <param name="deviceStream"></param>
        public USBDeviceBase(Tuple<HidStream, HidStream> deviceStream)
        {
            _deviceStream = deviceStream.Item1;
            _scrollStream = deviceStream.Item2;
            _model = InitModel();
            _lightingBase = InitDevice();
        }

        public void ProcessStreaming(float _brightness)
        {
            lock (locker)
            {
                SendToHardware(true, _brightness);
            }
        }

        public void ProcessStaticColors(ColorRGB[,] colorMatrix, float _brightness)
        {
            if (_lightingBase != null && _lightingBase.Count > 0)
            {
                foreach (var device in _lightingBase)
                {
                    device.ProcessStaticColors(colorMatrix, _brightness);
                }
                SendToHardware(false, _brightness);
            }
        }

        protected abstract List<LightingBase> InitDevice();

        protected abstract HardwareModel InitModel();

        ~USBDeviceBase()
        {
            _deviceStream?.Dispose();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _deviceStream != null)
            {
                _deviceStream.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Get the lightingBase in this USB Device
        /// </summary>
        /// <returns>List<LightingBase> List of lightingbase</returns>
        public List<LightingBase> GetLightingDevice()
        {
            return _lightingBase;
        }

        /// <summary>
        /// Get device update ability
        /// </summary>
        /// <returns>true and fw version, false and failure reason</returns>
        public virtual Tuple<bool, string> GetUpdateAbility()
        {
            return Tuple.Create(false, "N/A");
        }

        /// <summary>
        /// Process Update
        /// </summary>
        /// <returns></returns>
        public virtual Tuple<bool, string> Update()
        {
            return Tuple.Create(false, "No Update Avaliable");
        }

        /// <summary>
        /// Calling when App is closing to go back to hardware default mode
        /// Turn fw animation on/turn mb bypass on
        /// </summary>
        public virtual void TurnFwAnimationOn()
        {
        }

        /// <summary>
        /// Get current model of usb device
        /// </summary>
        /// <returns></returns>
        public HardwareModel GetModel()
        {
            if (_model != null)
            {
                return _model;
            }
            else
            {
                InitModel();
                return _model;
            }
        }
    }
}
