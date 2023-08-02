using Newtonsoft.Json;

namespace HueApi.Models.Requests.SmartScene
{
    public class CreateSmartScene: BaseResourceRequest
    {
        [JsonProperty("group")]
        public ResourceIdentifier? Group { get; set; }

        [JsonProperty("week_timeslots")]
        public List<SmartSceneDayTimeslot> WeekTimeslots { get; set; } = default!;

        [JsonProperty("active_timeslot")]
        public ActiveTimeslot ActiveTimeslot { get; set; } = default!;

        [JsonProperty("recall")]
        public SmartSceneRecall Recall { get; set; } = default!;
    }
}
