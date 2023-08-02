using HueApi.Models.Requests.Interface;
using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class UpdateLight: BaseResourceRequest, IUpdateColor, IUpdateColorTemperature, IUpdateOn, IUpdateDimmingDelta, IUpdateDimming
    {
        [JsonProperty("on")]
        public On? On { get; set; }

        [JsonProperty("dimming")]
        public Dimming? Dimming { get; set; }

        [JsonProperty("dimming_delta")]
        public DimmingDelta? DimmingDelta { get; set; }

        [JsonProperty("color_temperature")]
        public ColorTemperature? ColorTemperature { get; set; }

        [JsonProperty("color_temperature_delta")]
        public ColorTemperatureDelta? ColorTemperatureDelta { get; set; }

        [JsonProperty("color")]
        public Color? Color { get; set; }

        [JsonProperty("dynamics")]
        public Dynamics? Dynamics { get; set; }

        [JsonProperty("alert")]
        public UpdateAlert? Alert { get; set; }

        [JsonProperty("gradient")]
        public Gradient? Gradient { get; set; }

        [JsonProperty("effects")]
        public Effects? Effects { get; set; }

        [JsonProperty("timed_effects")]
        public TimedEffects? TimedEffects { get; set; }

        [JsonProperty("powerup")]
        public PowerUp? PowerUp { get; set; }

    }

    public class UpdateAlert
    {
        [JsonProperty("action")]
        public string Action { get; set; } = "breathe";

    }
}
