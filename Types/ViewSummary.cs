using Newtonsoft.Json;

namespace MajWebSocket.Types
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
