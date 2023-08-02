using Newtonsoft.Json;

namespace HueApi.Models
{
    public class Room: HueResource
    {
        [JsonProperty("children")]
        public List<ResourceIdentifier> Children { get; set; } = new();

        [JsonProperty("grouped_services")]
        public List<ResourceIdentifier> GroupedServices { get; set; } = new();

        [JsonProperty("services")]
        public List<ResourceIdentifier> Services { get; set; } = new();

    }
}
