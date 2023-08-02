using System.Diagnostics;
using Newtonsoft.Json;

namespace HueApi.Models
{
    [DebuggerDisplay("{Type} | {IdV1} | {Id}")]
    public class MatterItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = default!;

        [JsonProperty("id_v1")]
        public string? IdV1 { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = default!;

        [JsonProperty("max_fabrics")]
        public int MaxFabrics { get; set; } = default!;

        [JsonProperty("has_qr_code")]
        public bool HasQrCode { get; set; }
    }
}
