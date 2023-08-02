using Newtonsoft.Json;

namespace HueApi.Models
{
    public class HueErrors: List<HueError>
    {
    }

    public class HueError
    {
        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
