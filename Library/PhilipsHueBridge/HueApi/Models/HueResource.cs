using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HueApi.Models
{
  [DebuggerDisplay("{Type} | {IdV1} | {Id}")]
  public class HueResource
  {
    [JsonProperty("id")]
    public Guid Id { get; set; } = default!;

    [JsonProperty("id_v1")]
    public string? IdV1 { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = default!;

    [JsonProperty("metadata")]
    public Metadata? Metadata { get; set; } = default!;

    [JsonProperty("creation_time")]
    public DateTimeOffset? CreationTime { get; set; }

    [JsonProperty("owner")]
    public ResourceIdentifier? Owner { get; set; }

    [JsonProperty("services")]
    public List<ResourceIdentifier>? Services { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> ExtensionData { get; set; } = new();
  }
}
