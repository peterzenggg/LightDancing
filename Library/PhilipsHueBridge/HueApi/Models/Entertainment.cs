using Newtonsoft.Json;

namespace HueApi.Models
{
    public class Entertainment: HueResource
    {
        [JsonProperty("owner")]
        public ResourceIdentifier? Owner { get; set; }

        [JsonProperty("proxy")]
        public bool Proxy { get; set; }

        [JsonProperty("renderer")]
        public bool Renderer { get; set; }

        [JsonProperty("segments")]
        public Segment? Segments { get; set; }

        [JsonProperty("max_streams")]
        public int? MaxStreams { get; set; }

    }

    public class Segment
    {
        [JsonProperty("configurable")]
        public bool Configurable { get; set; }

        [JsonProperty("max_segments")]
        public int MaxSegments { get; set; }

        [JsonProperty("segments")]
        public List<SegmentItem> Segments { get; set; } = new();
    }

    public class SegmentItem
    {
        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }
    }
}
