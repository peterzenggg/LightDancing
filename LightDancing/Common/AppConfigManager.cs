using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LightDancing.Common
{
    public class AppConfigManager
    {
        private readonly string _configFilePath;

        public AppConfigManager()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string hyteFolderPath = Path.Combine(documentsPath, "HYTE");

            if (!Directory.Exists(hyteFolderPath))
            {
                Directory.CreateDirectory(hyteFolderPath);
            }

            _configFilePath = Path.Combine(hyteFolderPath, "settings.json");
        }

        public AppConfig LoadConfig()
        {
            if (!File.Exists(_configFilePath))
            {
                return new AppConfig();
            }

            string json = File.ReadAllText(_configFilePath);
            return JsonConvert.DeserializeObject<AppConfig>(json);
        }

        public void SaveConfig(AppConfig config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
        }
    }
    public class AppConfig
    {
        public List<NanoleafConfig> NanoleafConfigs { get; set; }
        public List<PhilipsConfig> PhilipsConfigs { get; set; }
    }

    public class NanoleafConfig
    {
        public string IPAddress { get; set; }
        public string AuthToken { get; set; }
    }
    public class PhilipsConfig
    {
        public string IPAddress { get; set; }
        public string UserName { get; set; }
        public string PSK { get; set; }
    }
}