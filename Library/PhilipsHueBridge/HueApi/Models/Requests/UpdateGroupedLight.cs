using HueApi.Models.Requests.Interface;
using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class UpdateGroupedLight: BaseResourceRequest, IUpdateColor, IUpdateColorTemperature, IUpdateOn, IUpdateDimmingDelta, IUpdateDimming
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

        [JsonProperty("alert")]
        public UpdateAlert? Alert { get; set; }

        [JsonProperty("dynamics")]
        public Dynamics? Dynamics { get; set; }

    }
}
