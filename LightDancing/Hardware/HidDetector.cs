using HidSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Hardware
{
    public class HidDetector
    {
        public List<HidStream> GetHidStreams(int vid, int pid, int maxReportLength, int maxOutputLength, int maxInputLength)
        {
            List<HidStream> hidStreams = null;
            foreach (var device in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxFeatureReportLength() == maxReportLength && x.GetMaxOutputReportLength() == maxOutputLength && x.GetMaxInputReportLength() == maxInputLength))
            {
                if (hidStreams == null)
                {
                    hidStreams = new List<HidStream>();
                }

                if (device.TryOpen(out HidStream stream))
                {
                    hidStreams.Add(stream);
                }
            }

            return hidStreams;
        }

        public List<Tuple<HidStream, HidStream>> GetHidStreamSets(int vid, int pid, int maxReportLength, int maxOutputLength, int maxInputLength, int scrollwheelInputLength)
        {
            List<Tuple<HidStream, HidStream>> hidStreams = null;
            var deviceList = DeviceList.Local.GetHidDevices(vid, pid);

            List<HidDevice> scrollStream = new List<HidDevice>();
            foreach (var scrollwheelDevice in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxInputReportLength() == scrollwheelInputLength))
            {
                scrollStream.Add(scrollwheelDevice);
            }

            foreach (var device in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxFeatureReportLength() == maxReportLength && x.GetMaxOutputReportLength() == maxOutputLength && x.GetMaxInputReportLength() == maxInputLength))
            {
                if (hidStreams == null)
                {
                    hidStreams = new List<Tuple<HidStream, HidStream>>();
                }

                if (scrollStream[hidStreams.Count] != null)
                {
                    if (device.TryOpen(out HidStream stream) && scrollStream[hidStreams.Count].TryOpen(out HidStream scrollwheelStream))
                        hidStreams.Add(Tuple.Create(stream, scrollwheelStream));
                }
            }

            return hidStreams;
        }

        /// <summary>
        /// Get hid streams for Redragon devices (with only max feature length)
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        /// <param name="maxReportLength"></param>
        /// <returns></returns>
        public List<HidStream> GetHidStreams(int vid, int pid, int maxReportLength, bool isNoneReadWritePermissions = false)
        {
            List<HidStream> hidStreams = null;
            foreach (var device in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxFeatureReportLength() == maxReportLength))
            {
                if (hidStreams == null)
                {
                    hidStreams = new List<HidStream>();
                }
                if (isNoneReadWritePermissions)
                {
                    OpenConfiguration operate = new OpenConfiguration();
                    operate.SetOption(OpenOption.Priority, OpenPriority.High);
                    if (device.TryOpen(operate, out HidStream stream))
                    {
                        hidStreams.Add(stream);
                    }
                }
                else
                {
                    if (device.TryOpen(out HidStream stream))
                    {
                        hidStreams.Add(stream);
                    }
                }
            }

            return hidStreams;
        }

        public List<Tuple<HidStream, HidStream>> GetKeebStreams(int vid, int pid, int maxReportLength, int maxOutputLength)
        {
            List<Tuple<HidStream, HidStream>> hidStreams = null;
            var deviceList = DeviceList.Local.GetHidDevices(vid, pid);

            List<HidDevice> streamingStream = new List<HidDevice>();
            var list = DeviceList.Local.GetHidDevices(vid, pid);
            foreach (var scrollwheelDevice in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxOutputReportLength() == maxOutputLength))
            {
                streamingStream.Add(scrollwheelDevice);
            }

            foreach (var device in DeviceList.Local.GetHidDevices(vid, pid).Where(x => x.GetMaxFeatureReportLength() == maxReportLength))
            {
                if (hidStreams == null)
                {
                    hidStreams = new List<Tuple<HidStream, HidStream>>();
                }

                if (streamingStream[hidStreams.Count] != null)
                {
                    if (device.TryOpen(out HidStream stream) && streamingStream[hidStreams.Count].TryOpen(out HidStream scrollwheelStream))
                        hidStreams.Add(Tuple.Create(stream, scrollwheelStream));
                }
            }

            return hidStreams;
        }
    }
}
