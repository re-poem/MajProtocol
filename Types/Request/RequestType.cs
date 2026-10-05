using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MajProtocol.Types.Request
{
    public enum RequestType
    {
        Reset = 0,
        Load = 1,
        Parse = 2,
        Play = 3,
        Pause = 4,
        Resume = 5,
        Stop = 6,
        State = 7,
        Setting = 8,
    }
}
