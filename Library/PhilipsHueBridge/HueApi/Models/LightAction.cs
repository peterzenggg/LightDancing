using HueApi.Models.Requests.Interface;
using Newtonsoft.Json;

namespace HueApi.Models
{
  public class LightAction : IUpdateColor, IUpdateColorTemperature, IUpdateOn
  {
    [JsonProperty("on")]
    public On? On { get; set; }

    [JsonProperty("dimming")]
    public Dimming? Dimming { get; set; }

    [JsonProperty("color")]
    public Color? Color { get; set; }

    [JsonProperty("color_temperature")]
    public ColorTemperature? ColorTemperature { get; set; }

    [JsonProperty("gradient")]
    public Gradient? Gradient { get; set; }

    [JsonProperty("effects")]
    public Effects? Effects { get; set; }

    [JsonProperty("dynamics")]
    public Dynamics? Dynamics { get; set; }
  }
}
