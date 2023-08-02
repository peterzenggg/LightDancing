using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models.Requests
{
  public class UpdateScene : BaseResourceRequest
  {
    [JsonProperty("actions")]
    public List<SceneAction>? Actions { get; set; }

    [JsonProperty("recall")]
    public Recall? Recall { get; set; }

    [JsonProperty("palette")]
    public Palette? Palette { get; set; }

    [JsonProperty("speed")]
    public double? Speed { get; set; }

  }

  [JsonConverter(typeof(StringEnumConverter))]
  public enum SceneRecallAction
  {
    active, dynamic_palette
  }

  [JsonConverter(typeof(StringEnumConverter))]
  public enum SceneRecallStatus
  {
    active, dynamic_palette
  }

  public class Recall
  {
    [JsonProperty("action")]
    public SceneRecallAction? Action { get; set; }

    [JsonProperty("status")]
    public SceneRecallStatus? Status { get; set; }

    [JsonProperty("duration")]
    public int? Duration { get; set; }

    [JsonProperty("dimming")]
    public Dimming? Dimming { get; set; }
  }
}
