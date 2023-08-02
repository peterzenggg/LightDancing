using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class UpdateEntertainment: BaseResourceRequest
    {
        [JsonProperty("proxy")]
        public bool Proxy { get; set; }

        [JsonProperty("renderer")]
        public bool Renderer { get; set; }

        [JsonProperty("segments")]
        public Segment? Segments { get; set; }

        [JsonProperty("max_streams")]
        public int? MaxStreams { get; set; }
    }
}
