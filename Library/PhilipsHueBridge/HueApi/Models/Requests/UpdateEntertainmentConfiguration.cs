using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models.Requests
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum EntertainmentConfigurationAction
  {
    start, stop
  }

  public class UpdateEntertainmentConfiguration : BaseResourceRequest
  {
    [JsonProperty("action")]
    public EntertainmentConfigurationAction? Action { get; set; }

    [JsonProperty("configuration_type")]
    public EntertainmentConfigurationType? ConfigurationType { get; set; }

    [JsonProperty("locations")]
    public Locations? Locations { get; set; }

    [JsonProperty("stream_proxy")]
    public StreamProxy? StreamProxy { get; set; }

  }
}
