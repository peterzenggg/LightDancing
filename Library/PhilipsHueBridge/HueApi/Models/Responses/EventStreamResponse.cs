using Newtonsoft.Json;

namespace HueApi.Models.Responses
{
    public class EventStreamData: HueResource
    {
        [JsonProperty("owner")]
        public ResourceIdentifier? Owner { get; set; }

    }

    public class EventStreamResponse: HueResource
    {
        [JsonProperty("creationtime")]
        public new DateTimeOffset CreationTime { get; set; }

        [JsonProperty("data")]
        public List<EventStreamData> Data { get; set; } = new();

    }

}
