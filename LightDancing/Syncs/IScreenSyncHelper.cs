using LightDancing.Colors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LightDancing.Syncs
{
    public interface IScreenSyncHelper : ISyncHelper
    {
        /// <summary>
        /// Register for monitor howswap callback
        /// </summary>
        /// <param name="flushAction">init this function when monitor hotswap is invoked</param>
        public void RegisterFlushList(Action flushAction);

        /// <summary>
        /// Get list of monitor names
        /// </summary>
        /// <returns>list of monitor names</returns>
        public List<string> GetMonitorList();

        /// <summary>
        /// Change sycning monitor
        /// </summary>
        /// <param name="index">index from monitorList</param>
        public void ChangeMonitor(int index);

        /// <summary>
        /// Start screen sync
        /// </summary>
        public void Start();

        /// <summary>
        /// Update ScreenSync mode
        /// </summary>
        /// <param name="mode"></param>
        public void UpdateMode(ColorEffectBase mode);
    }
}
