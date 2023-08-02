using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueApi.Models
{
    public class Light: HueResource
    {
        [JsonProperty("owner")]
        public ResourceIdentifier Owner { get; set; } = default!;

        [JsonProperty("on")]
        public On On { get; set; } = default!;

        [JsonProperty("dimming")]
        public Dimming? Dimming { get; set; }

        [JsonProperty("dimming_delta")]
        public DimmingDelta? DimmingDelta { get; set; }

        [JsonProperty("color_temperature")]
        public ColorTemperature? ColorTemperature { get; set; }

        [JsonProperty("color_temperature_delta")]
        public ColorTemperatureDelta? ColorTemperatureDelta { get; set; }

        [JsonProperty("color")]
        public Color? Color { get; set; }

        [JsonProperty("dynamics")]
        public Dynamics? Dynamics { get; set; }

        [JsonProperty("alert")]
        public Alert? Alert { get; set; }

        [JsonProperty("signaling")]
        public Signaling? Signaling { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; } = default!;

        [JsonProperty("gradient")]
        public Gradient? Gradient { get; set; }

        [JsonProperty("effects")]
        public Effects? Effects { get; set; }

        [JsonProperty("timed_effects")]
        public TimedEffects? TimedEffects { get; set; }

        [JsonProperty("powerup")]
        public PowerUp? PowerUp { get; set; }

    }

    public class Alert
    {
        [JsonProperty("action_values")]
        public List<string> ActionValues { get; set; } = new List<string>();
    }

    public class Signaling
    {
        [JsonProperty("status")]
        public SignalingStatus? Status { get; set; }
    }

    public class SignalingStatus
    {
        /// <summary>
        /// Indicates which signal is currently active.
        /// </summary>
        [JsonProperty("signal")]
        public Signal Signal { get; set; }

        /// <summary>
        /// Timestamp indicating when the active signal is expected to end. Value is not set if there is no_signal
        /// </summary>
        [JsonProperty("estimated_end")]
        public DateTimeOffset? EstimatedEnd { get; set; }
    }

    public class Dynamics
    {
        /// <summary>
        /// Duration of a light transition or timed effects in ms.
        /// </summary>
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("speed_valid")]
        public bool SpeedValid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = default!;

        [JsonProperty("status_values")]
        public List<string> StatusValues { get; set; } = new();
    }

    public class On
    {
        [JsonProperty("on")]
        public bool IsOn { get; set; }
    }

    public class PowerUpOn
    {
        /// <summary>
        /// State to activate after powerup. On will use the value specified in the “on” property. When setting mode “on”, the on property must be included. Toggle will alternate between on and off on each subsequent power toggle. Previous will return to the state it was in before powering off.
        /// </summary>
        [JsonProperty("mode")]
        public PowerUpOnMode? Mode { get; set; }

        [JsonProperty("on")]
        public On? On { get; set; }
    }

    public class XyPosition
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public class Gamut
    {
        [JsonProperty("blue")]
        public XyPosition Blue { get; set; } = new();

        [JsonProperty("green")]
        public XyPosition Green { get; set; } = new();

        [JsonProperty("red")]
        public XyPosition Red { get; set; } = new();
    }


    public class Color
    {
        [JsonProperty("gamut")]
        public Gamut? Gamut { get; set; }

        [JsonProperty("gamut_type")]
        public string? GamutType { get; set; }

        [JsonProperty("xy")]
        public XyPosition Xy { get; set; } = new();
    }

    public class MirekSchema
    {
        [JsonProperty("mirek_maximum")]
        public int MirekMaximum { get; set; }

        [JsonProperty("mirek_minimum")]
        public int MirekMinimum { get; set; }
    }

    public class ColorTemperature
    {
        /// <summary>
        /// minimum: 153 – maximum: 500
        /// </summary>
        [JsonProperty("mirek")]
        public int? Mirek { get; set; }

        [JsonProperty("mirek_schema")]
        public MirekSchema MirekSchema { get; set; } = default!;

        [JsonProperty("mirek_valid")]
        public bool MirekValid { get; set; }
    }

    public class ColorTemperatureDelta
    {
        [JsonProperty("action")]
        public DeltaAction Action { get; set; }

        /// <summary>
        ///  maximum: 347
        ///  Mirek delta to current mirek. Clip at mirek_minimum and mirek_maximum of mirek_schema.
        /// </summary>
        [JsonProperty("mirek_delta")]
        public int MirekDelta { get; set; }
    }

    public class Dimming
    {
        [JsonProperty("brightness")]
        public double Brightness { get; set; } = 100;

        [JsonProperty("min_dim_level")]
        public double? MinDimLevel { get; set; }
    }

    public class DimmingDelta
    {
        [JsonProperty("action")]
        public DeltaAction Action { get; set; }

        [JsonProperty("brightness_delta")]
        public double BrightnessDelta { get; set; }
    }

    public class Effects
    {
        [JsonProperty("effect")]
        public Effect Effect { get; set; } = new();

        [JsonProperty("effect_values")]
        public List<string>? EffectValues { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("status_values")]
        public List<string>? StatusValues { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Effect
    {
        no_effect, fire, candle, sparkle
    }

    public class TimedEffects
    {
        [JsonProperty("effect")]
        public TimedEffect Effect { get; set; } = new();

        /// <summary>
        /// Duration is mandatory when timed effect is set except for no_effect. Resolution decreases for a larger duration. e.g Effects with duration smaller than a minute will be rounded to a resolution of 1s, while effects with duration larger than an hour will be arounded up to a resolution of 300s. Duration has a max of 21600000 ms.
        /// </summary>
        [JsonProperty("duration")]
        public int Duration { get; set; } = new();

        [JsonProperty("effect_values")]
        public List<string>? EffectValues { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("status_values")]
        public List<string>? StatusValues { get; set; }

    }

    public class PowerUp
    {
        [JsonProperty("preset")]
        public PowerUpPreset Preset { get; set; } = new();

        [JsonProperty("on")]
        public PowerUpOn? On { get; set; }

        [JsonProperty("dimming")]
        public Dimming? Dimming { get; set; }

        [JsonProperty("color")]
        public Color? Color { get; set; }

        [JsonProperty("configured")]
        public bool? Configured { get; set; }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimedEffect
    {
        no_effect, sunrise
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeltaAction
    {
        up, down, stop
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PowerUpPreset
    {
        safety, powerfail, last_on_state, custom
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PowerUpOnMode
    {
        on, toggle, previous
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Signal
    {
        no_signal, on_off
    }
}
