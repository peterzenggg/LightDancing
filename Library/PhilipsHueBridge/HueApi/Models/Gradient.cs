using HueApi.Models.Requests.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class Gradient
    {
        [JsonProperty("points")]
        public List<GradientPoint> Points { get; set; } = new();

        [JsonProperty("mode")]
        public GradientMode Mode { get; set; } = new();

        [JsonProperty("mode_values")]
        public List<string>? ModeValues { get; set; }

        [JsonProperty("points_capable")]
        public int? PointsCapable { get; set; }

        [JsonProperty("pixel_count")]
        public int? PixelCount { get; set; }
    }

    public class GradientPoint: IUpdateColor
    {
        [JsonProperty("color")]
        public Color? Color { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GradientMode
    {
        interpolated_palette, interpolated_palette_mirrored, random_pixelated
    }
}
