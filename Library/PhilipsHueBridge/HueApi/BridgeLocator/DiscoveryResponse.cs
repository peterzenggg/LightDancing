using Newtonsoft.Json;

namespace HueApi.BridgeLocator
{
    public class DiscoveryResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("internalipaddress")]
        public string InternalIpAddress { get; set; } = default!;

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
