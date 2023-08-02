using System.Diagnostics;
using Newtonsoft.Json;

namespace HueApi.Models
{
  [DebuggerDisplay("{Name} {Archetype}")]
  public class Metadata
  {
    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("archetype")]
    public string? Archetype { get; set; }

    [JsonProperty("image")]
    public ResourceIdentifier? Image { get; set; }
  }
}
