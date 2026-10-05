using MajProtocol.Types.Enums;
using Newtonsoft.Json;

namespace MajProtocol.Types
{
    public readonly struct ViewSummary
    {
        public ViewStatus State { get; init; }
        public string? ErrMsg { get; init; }
        public float Timeline { get; init; }
    }
}
