using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class SmartScene: HueResource
    {
        [JsonProperty("group")]
        public ResourceIdentifier? Group { get; set; }

        [JsonProperty("week_timeslots")]
        public List<SmartSceneDayTimeslot> WeekTimeslots { get; set; } = default!;

        [JsonProperty("active_timeslot")]
        public ActiveTimeslot ActiveTimeslot { get; set; } = default!;

        [JsonProperty("state")]
        public SmartSceneState State { get; set; }
    }

    public class SmartSceneDayTimeslot
    {
        [JsonProperty("timeslots")]
        public List<SmartSceneTimeslot> Timeslots { get; set; } = default!;

        [JsonProperty("recurrence")]
        public List<Weekday> Recurrence { get; set; } = default!;
    }

    public class SmartSceneTimeslot
    {
        [JsonProperty("start_time")]
        public TimeslotStartTime StartTime { get; set; } = default!;

        [JsonProperty("target")]
        public ResourceIdentifier Target { get; set; } = default!;


    }

    public class TimeslotStartTime
    {
        [JsonProperty("kind")]
        public string Kind { get; set; } = "time";

        [JsonProperty("time")]
        public TimeslotStartTimeTime Time { get; set; } = default!;
    }

    public class TimeslotStartTimeTime
    {
        [JsonProperty("hour")]
        public int Hour { get; set; } = default!;

        [JsonProperty("minute")]
        public int Minute { get; set; } = default!;

        [JsonProperty("second")]
        public int Second { get; set; } = default!;
    }

    public class ActiveTimeslot
    {
        [JsonProperty("timeslot_id")]
        public int TimeslotId { get; set; }

        [JsonProperty("weekday")]
        public Weekday Weekday { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Weekday
    {
        monday, tuesday, wednesday, thursday, friday, saturday, sunday
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SmartSceneState
    {
        inactive, active
    }
}
