using Newtonsoft.Json;

namespace HueApi.Models.Sensors
{
    public class LightLevel: HueResource
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = default!;

        [JsonProperty("light")]
        public Light Light { get; set; } = default!;

        [JsonProperty("owner")]
        public ResourceIdentifier? Owner { get; set; }
    }

    public class Light
    {
        [JsonProperty("light_level")]
        public int LightLevel { get; set; } = default!;

        [JsonProperty("light_level_valid")]
        public bool LightLevelValid { get; set; }

        public double LuxLevel
        {
            get
            {
                double lightLevel = LightLevel > 0 ? LightLevel - 1 : 0;
                lightLevel = lightLevel / 10000;
                return Math.Pow(10, lightLevel);
            }
        }
    }
}
