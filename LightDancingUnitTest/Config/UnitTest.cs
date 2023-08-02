using System.Collections.Generic;
using LightDancing.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightDancingUnitTest.Config
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestReadAppConfig()
        {
            AppConfigManager configManager = new AppConfigManager();

            // Load the existing configuration or create a new one
            AppConfig config = configManager.LoadConfig();

            // Modify the configuration
            config.NanoleafConfigs = new List<NanoleafConfig>() { new NanoleafConfig() { IPAddress = "192.168.0.21", AuthToken = "ADASDQWEWQEEWQ" } };

            // Save the configuration
            configManager.SaveConfig(config);
        }
    }
}
