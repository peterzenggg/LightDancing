using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
  public class RelativeRotaryResource : HueResource
  {
    [JsonProperty("relative_rotary")]
    public RelativeRotary? RelativeRotary { get; set; }
  }

  public class RelativeRotary
  {
    [JsonProperty("last_event")]
    public RelativeRotaryLastEvent? LastEvent { get; set; }
  }

  public class RelativeRotaryLastEvent
  {
    [JsonProperty("action")]
    public RelativeRotaryLastEventAction? Action { get; set; }

    [JsonProperty("rotation")]
    public RelativeRotaryLastEventRotation? Rotation { get; set; }
  }

  public class RelativeRotaryLastEventRotation
  {
    /// <summary>
    /// A rotation opposite to the previous rotation, will always start with new start command.
    /// </summary>
    [JsonProperty("direction")]
    public RelativeRotaryDirection? Direction { get; set; }

    /// <summary>
    /// amount of rotation since previous event in case of repeat, amount of rotation since start in case of a start_event. Resolution = 1000 steps / 360 degree rotation.
    /// </summary>
    [JsonProperty("steps")]
    public int? Steps { get; set; }

    [JsonProperty("duration")]
    public int? Duration { get; set; }
  }

  [JsonConverter(typeof(StringEnumConverter))]
  public enum RelativeRotaryDirection
  {
    clock_wise, counter_clock_wise
  }

  [JsonConverter(typeof(StringEnumConverter))]
  public enum RelativeRotaryLastEventAction
  {
    start, repeat
  }
}
