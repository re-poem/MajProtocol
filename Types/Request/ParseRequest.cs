namespace MajProtocol.Types.Request
{
    public readonly struct ParseRequest
    {
        public string? SimaiFumen { get; init; }
        public int ClockCount { get; init; }
        public float Offset { get; init; }

        public string? Title { get; init; }
        public string? Artist { get; init; }
        public string? Level { get; init; }
        public string? Designer { get; init; }
        public int Difficulty { get; init; }
    }
}
