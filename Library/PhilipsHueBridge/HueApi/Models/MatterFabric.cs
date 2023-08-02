using System.Diagnostics;
using Newtonsoft.Json;

namespace HueApi.Models
{
  [DebuggerDisplay("{Type} | {IdV1} | {Id}")]
  public class MatterFabric : MatterItem
  {
    [JsonProperty("status")]
    public string Status { get; set; } = default!;

    [JsonProperty("fabric_data")]
    public FabricData? FabricData { get; set; } = default!;

    [JsonProperty("creation_time")]
    public DateTimeOffset CreationTime { get; set; } = default!;

  }
  public class FabricData
  {
    [JsonProperty("label")]
    public string Label { get; set; } = default!;

    [JsonProperty("vendor_id")]
    public int VendorId { get; set; } = default!;
  }
}
