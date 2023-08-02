using Newtonsoft.Json;

namespace HueApi.Models
{
  public class ProductData
  {
    [JsonProperty("certified")]
    public bool Certified { get; set; }

    [JsonProperty("manufacturer_name")]
    public string ManufacturerName { get; set; } = default!;

    [JsonProperty("model_id")]
    public string ModelId { get; set; } = default!;

    [JsonProperty("product_archetype")]
    public string ProductArchetype { get; set; } = default!;

    [JsonProperty("product_name")]
    public string ProductName { get; set; } = default!;

    [JsonProperty("software_version")]
    public string SoftwareVersion { get; set; } = default!;
  }

  public class Device : HueResource
  {
    [JsonProperty("product_data")]
    public ProductData ProductData { get; set; } = new();

    [JsonProperty("services")]
    public List<ResourceIdentifier> Services { get; set; } = new();

  }
}
