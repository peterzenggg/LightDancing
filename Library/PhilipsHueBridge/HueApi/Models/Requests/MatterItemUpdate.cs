using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
  public class MatterItemUpdate
  {
    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("action")]
    public string Action { get; } = "matter_reset";
  }
}
