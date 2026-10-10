using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MajProtocol.Types.Response
{
    public enum ResponseType
    {
        Error = 0,
        Ok = 1,
        Event = 2,
    }
}
