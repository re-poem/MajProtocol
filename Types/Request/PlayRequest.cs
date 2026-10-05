using MajProtocol.Types.Enums;
using Newtonsoft.Json;

namespace MajProtocol.Types.Request
{
    public readonly struct PlayRequest
    {
        public PlaybackMode PlayMode { get; init; }
        public double StartAt { get; init; }
        public float Speed { get; init; }


        public string? Title { get; init; }
        public string? Artist { get; init; }
        public float Offset { get; init; }

        public int Difficulty { get; init; }
        public string? Level { get; init; }
        public string? Designer { get; init; }

        public string? MaidataPath { get; init; }
    }
}
