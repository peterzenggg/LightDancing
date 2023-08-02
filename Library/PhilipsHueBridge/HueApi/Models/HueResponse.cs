using Newtonsoft.Json;

namespace HueApi.Models
{
  public class HueResponse<T> : HueErrorResponse
  {
    [JsonProperty("data")]
    public List<T> Data { get; set; } = new();
  }

  public class HuePostResponse : HueResponse<ResourceIdentifier>
  {

  }

  public class HuePutResponse : HueResponse<ResourceIdentifier>
  {

  }

  public class HueDeleteResponse : HueResponse<ResourceIdentifier>
  {

  }

  public class HueErrorResponse
  {
    [JsonProperty("errors")]
    public HueErrors Errors { get; set; } = new();

    public bool HasErrors => Errors.Any();

  }
}
