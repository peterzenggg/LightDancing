using HueApi.Models.Requests.Interface;
using Newtonsoft.Json;

namespace HueApi.Models
{
    public class Scene: HueResource
    {
        [JsonProperty("actions")]
        public List<SceneAction> Actions { get; set; } = new();

        [JsonProperty("group")]
        public ResourceIdentifier? Group { get; set; }

        [JsonProperty("palette")]
        public Palette? Palette { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }
    }

    public class SceneAction
    {
        [JsonProperty("action")]
        public LightAction Action { get; set; } = default!;

        [JsonProperty("target")]
        public ResourceIdentifier Target { get; set; } = default!;
    }

    public class Palette
    {
        [JsonProperty("color")]
        public List<ColorPalette> Color { get; set; } = new();

        [JsonProperty("color_temperature")]
        public List<ColorTemperature> ColorTemperature { get; set; } = new();

        //[MaxLength(1)]
        [JsonProperty("dimming")]
        public List<Dimming> Dimming { get; set; } = new();
    }

    public class ColorPalette
    {
        [JsonProperty("color")]
        public Color Color { get; set; } = new();

        [JsonProperty("dimming")]
        public Dimming Dimming { get; set; } = new();
    }

}
