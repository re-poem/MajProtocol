using Newtonsoft.Json;

namespace MajWebSocket.Types.Request
{
    public readonly struct LoadRequest
    {
        [JsonProperty("trackPath", NullValueHandling = NullValueHandling.Ignore)]
        public string? TrackPath { get; init; }

        [JsonProperty("imagePath", NullValueHandling = NullValueHandling.Ignore)]
        public string? ImagePath { get; init; }

        [JsonProperty("videoPath", NullValueHandling = NullValueHandling.Ignore)]
        public string? VideoPath { get; init; }
    }
}
