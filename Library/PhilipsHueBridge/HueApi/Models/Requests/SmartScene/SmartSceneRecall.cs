using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models.Requests.SmartScene
{
  public class SmartSceneRecall
  {
    [JsonProperty("action")]
    public SmartSceneRecallAction Action { get; set; } = default!;
  }

  [JsonConverter(typeof(StringEnumConverter))]
  public enum SmartSceneRecallAction
  {
    activate, deactivate
  }
}
