using Newtonsoft.Json;

namespace MajProtocol.Types.Enums
{
    public readonly struct ViewSummary
    {
        [JsonProperty("state")]
        public ViewStatus State { get; init; }

        [JsonProperty("errMsg", NullValueHandling = NullValueHandling.Ignore)]
        public string? ErrMsg { get; init; }

        [JsonProperty("timeline")]
        public float Timeline { get; init; }
    }
}
