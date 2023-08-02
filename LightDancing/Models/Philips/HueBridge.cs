using Newtonsoft.Json;

namespace LightDancing.Models.Philips
{
    public class HueBridgeDiscoveryResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("internalipaddress")]
        public string InternalIPAddress { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }
    public class HueBridgeNewUsesRequest
    {
        [JsonProperty("devicetype")]
        public string DeviceType { get; set; }

        [JsonProperty("generateclientkey")]
        public bool GenerateClientKey { get; set; }
    }
    public class HueBridgeNewUserResponse
    {
        [JsonProperty("success")]
        public UsernameAndClientKey Success { get; set; }
    }

    public class UsernameAndClientKey
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("clientkey")]
        public string ClientKey { get; set; }
    }

    public class HueBridgeConfigResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
