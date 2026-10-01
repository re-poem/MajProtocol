using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MajWebSocket.Types.Request
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestType
    {
        [EnumMember(Value = "Reset")]
        Reset = 0,

        [EnumMember(Value = "Load")]
        Load = 1,

        [EnumMember(Value = "Parse")]
        Parse = 2,

        [EnumMember(Value = "Play")]
        Play = 3,

        [EnumMember(Value = "Pause")]
        Pause = 4,

        [EnumMember(Value = "Resume")]
        Resume = 5,

        [EnumMember(Value = "Stop")]
        Stop = 6,

        [EnumMember(Value = "State")]
        State = 7,
    }
}
