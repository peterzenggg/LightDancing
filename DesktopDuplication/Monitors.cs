using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DesktopDuplication
{
    public class Monitors
    {
        private readonly Dictionary<string, Tuple<int, int>> MONITOR_MAP = new Dictionary<string, Tuple<int, int>>();
        private int oldCount = Screen.AllScreens.Length;
        private Factory1 factory = new Factory1();

        public bool CheckMonitor()
        {
            int newcount = Screen.AllScreens.Length;
            if (newcount != oldCount)
            {
                oldCount = newcount;
                factory = new Factory1();
                MONITOR_MAP.Clear();
                return true;
            }
            return false;
        }

        public Dictionary<string, Tuple<int, int>> GetMonitors()
        {
            int GPU_count = factory.GetAdapterCount1();
            int monitor_count = 1;
            for (int i = 0; i < GPU_count; i++)
            {
                Adapter1 adapter = factory.GetAdapter1(i);

                var count = adapter.GetOutputCount();
                if (MONITOR_MAP.Count == 0 && count > 0)
                {
                    MONITOR_MAP.Add("All Screen", Tuple.Create(-1, -1));
                }

                for (int j = 0; j < count; j++)
                {
                    string monitorName = adapter.Outputs[j].Description.DeviceName;
                    MONITOR_MAP.Add(monitorName.Split('\\').Last(), Tuple.Create(i, j));
                    monitor_count++;
                }
                adapter.Dispose();
            }

            factory.Dispose();

            return MONITOR_MAP;
        }
    }
}
