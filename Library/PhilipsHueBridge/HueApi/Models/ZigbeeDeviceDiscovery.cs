using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class ZigbeeDeviceDiscovery: HueResource
    {
        [JsonProperty("status")]
        public ZigbeeDeviceDiscoveryStatus? Status { get; set; }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ZigbeeDeviceDiscoveryStatus
    {
        active, ready
    }
}
