using Newtonsoft.Json;

namespace HueApi.Models
{
  public class GroupedLight : HueResource
  {
    [JsonProperty("alert")]
    public Alert? Alert { get; set; }

    [JsonProperty("on")]
    public On? On { get; set; }
  }
}
