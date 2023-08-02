using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
  public class RegisterRequest
  {
    public RegisterRequest(string deviceType)
    {
      DeviceType = deviceType;
    }

    [JsonProperty("devicetype")]
    public string DeviceType { get; }

    [JsonProperty("generateclientkey")]
    public bool GenerateClientKey { get; set; } = true;
  }
}
