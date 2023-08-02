using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class ButtonResource: HueResource
    {
        [JsonProperty("button")]
        public Button? Button { get; set; }
    }

    public class Button
    {
        [JsonProperty("last_event")]
        public ButtonLastEvent? LastEvent { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ButtonLastEvent
    {
        initial_press, repeat, short_release, long_release, double_short_release, long_press
    }
}