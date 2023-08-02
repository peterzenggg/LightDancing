using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class UpdateDevice: BaseResourceRequest
    {
        [JsonProperty("identify")]
        public Identify? Identify { get; set; }
    }

    public class Identify
    {
        [JsonProperty("action")]
        public string Action { get; set; } = "identify";
    }
}
