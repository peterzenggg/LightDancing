using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class BaseResourceRequest
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("metadata")]
        public Metadata? Metadata { get; set; }
    }
}
