using System;
using System.Threading.Tasks;
using LightDancing.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LightDancingUnitTest.MDNSFind
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task TestFindNanoleafAsync()
        {
            var nanoleafServiceType = "_nanoleafapi._tcp.local.";
            var nanoleafFinder = new MDNSFinder(nanoleafServiceType);

            Console.WriteLine("Searching for Nanoleaf devices...");
            var nanoleafDevices = await nanoleafFinder.FindDevicesAsync();

            if (nanoleafDevices.Count > 0)
            {
                Console.WriteLine("Nanoleaf devices found:");
                foreach (var device in nanoleafDevices)
                {
                    Console.WriteLine($"{JsonConvert.SerializeObject(device)}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No Nanoleaf devices found.");
            }
        }
    }
}
