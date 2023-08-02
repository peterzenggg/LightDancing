using Newtonsoft.Json;

namespace HueApi.Models.Requests
{
    public class CreateScene: BaseResourceRequest
    {
        public CreateScene(Metadata metadata, ResourceIdentifier group)
        {
            Metadata = metadata;
            Group = group;
        }

        [JsonProperty("group")]
        public ResourceIdentifier Group { get; set; }

        [JsonProperty("actions")]
        public List<SceneAction> Actions { get; set; } = new();

        [JsonProperty("palette")]
        public Palette? Palette { get; set; }

        [JsonProperty("speed")]
        public double? Speed { get; set; }
    }
}
