using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace MajProtocol.Types.Enums
{
    public enum ViewStatus
    {
        Idle,
        Loaded,
        Ready,
        Error,
        Playing,
        Paused,
        Busy,
    }
}
