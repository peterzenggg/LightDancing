using System.Diagnostics;
using Newtonsoft.Json;

namespace HueApi.Models
{
    [DebuggerDisplay("{Rtype} | {Rid}")]
    public record ResourceIdentifier
    {
        [JsonProperty("rid")]
        public Guid Rid { get; set; }

        [JsonProperty("rtype")]
        public string Rtype { get; set; } = default!;
    }
}
