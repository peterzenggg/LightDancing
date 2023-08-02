using Newtonsoft.Json;

namespace HueApi.Models.Responses
{
    public class RegisterResponse
    {
        [JsonProperty("success")]
        public RegisterResult Success { get; } = default!;
    }

    public class RegisterResult
    {
        [JsonProperty("username")]
        public string Username { get; } = default!;

        [JsonProperty("clientkey")]
        public string? ClientKey { get; set; }
    }
}
