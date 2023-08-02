using Newtonsoft.Json;

namespace HueApi.Models
{
    public class ZgpConnectivity: HueResource
    {
        /// <summary>
        /// connected if device has been recently been available. When indicating connectivity issues the device is powered off or has network issues When indicating unidirectional incoming the device only talks to bridge
        /// </summary>
        [JsonProperty("status")]
        public ConnectivityStatus? Status { get; set; }

        [JsonProperty("source_id")]
        public string? SourceId { get; set; }
    }
}
