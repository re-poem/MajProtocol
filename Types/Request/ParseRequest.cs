namespace MajProtocol.Types.Request
{
    public readonly struct ParseRequest
    {
        public string? SimaiFumen { get; init; }
        public int ClockCount { get; init; }
    }
}
