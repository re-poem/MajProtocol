using Newtonsoft.Json;

namespace MajProtocol.Types.Request
{
    public readonly struct LoadRequest
    {
        public string? TrackPath { get; init; }
        public string? ImagePath { get; init; }
        public string? VideoPath { get; init; }
    }
}
