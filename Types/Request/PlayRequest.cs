using Newtonsoft.Json;

namespace MajProtocol.Types.Request
{
    public readonly struct PlayRequest
    {
        [JsonProperty("startAt")]
        public double StartAt { get; init; }

        [JsonProperty("simaiFumen", NullValueHandling = NullValueHandling.Ignore)]
        public string? SimaiFumen { get; init; }

        [JsonProperty("offset")]
        public double Offset { get; init; }

        [JsonProperty("speed")]
        public float Speed { get; init; }
    }
}
