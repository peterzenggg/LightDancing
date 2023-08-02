using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class ZigbeeConnectivity: HueResource
    {
        /// <summary>
        /// connected if device has been recently been available. When indicating connectivity issues the device is powered off or has network issues When indicating unidirectional incoming the device only talks to bridge
        /// </summary>
        [JsonProperty("status")]
        public ConnectivityStatus? Status { get; set; }

        [JsonProperty("mac_address")]
        public string? MacAddress { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectivityStatus
    {
        connected, disconnected, connectivity_issue, unidirectional_incoming
    }
}
