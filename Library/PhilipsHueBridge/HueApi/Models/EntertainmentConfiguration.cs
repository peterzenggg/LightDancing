using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace HueApi.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntertainmentConfigurationType
    {
        screen,
        monitor,
        music,
        [EnumMember(Value = "3dspace")]
        _3dspace,
        other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntertainmentConfigurationStatus
    {
        inactive, active
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntertainmentConfigurationStreamProxyMode
    {
        auto, manual
    }

    public class EntertainmentConfiguration: HueResource
    {
        [JsonProperty("configuration_type")]
        public EntertainmentConfigurationType ConfigurationType { get; set; }

        [JsonProperty("locations")]
        public Locations Locations { get; set; } = new();

        [JsonProperty("stream_proxy")]
        public StreamProxy StreamProxy { get; set; } = new();

        [Obsolete("Deprecated: resolve via entertainment services in locations object")]
        [JsonProperty("light_services")]
        public List<ResourceIdentifier> LightServices { get; set; } = new();

        [JsonProperty("channels")]
        public List<EntertainmentChannel> Channels { get; set; } = new();

        [Obsolete("Deprecated: use metadata/name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("status")]
        public EntertainmentConfigurationStatus Status { get; set; }

        [JsonProperty("active_streamer")]
        public ResourceIdentifier? ActiveStreamer { get; set; }


    }

    [DebuggerDisplay("{ChannelId} | {Position}")]
    public class EntertainmentChannel
    {
        [JsonProperty("channel_id")]
        public int ChannelId { get; set; }

        [JsonProperty("position")]
        public HuePosition Position { get; set; } = new();

        [JsonProperty("members")]
        public List<Member> Members { get; set; } = new();
    }

    [DebuggerDisplay("{Index} | {Service}")]
    public class Member
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("service")]
        public ResourceIdentifier? Service { get; set; }
    }

    public class StreamProxy
    {
        [JsonProperty("mode")]
        public EntertainmentConfigurationStreamProxyMode Mode { get; set; }

        [JsonProperty("node")]
        public ResourceIdentifier? Node { get; set; }
    }

    public class HueServiceLocation
    {
        [JsonProperty("positions")]
        public List<HuePosition> Positions { get; set; } = new();

        [JsonProperty("service")]
        public ResourceIdentifier? Service { get; set; }

        [Obsolete("Use Positions")]
        [JsonProperty("position")]
        public HuePosition? Position { get; set; }

        /// <summary>
        /// Relative equalization factor applied to the entertainment service, to compensate for differences in brightness in the entertainment configuration. Value cannot be 0, writing 0 changes it to lowest possible value.
        /// </summary>
        [JsonProperty("equalization_factor")]
        public double? EqualizationFactor { get; set; }
    }

    public class Locations
    {
        [JsonProperty("service_locations")]
        public List<HueServiceLocation> ServiceLocations { get; set; } = new();
    }
}
