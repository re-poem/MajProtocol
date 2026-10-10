using MajProtocol.Types.Enums;
using Newtonsoft.Json;

namespace MajProtocol.Types.Request
{
    public readonly struct PlayRequest
    {
        public PlaybackMode PlayMode { get; init; }
        public double StartAt { get; init; }
        public float Speed { get; init; }

        public string? MaidataPath { get; init; }
    }
}
