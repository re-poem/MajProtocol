using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MajProtocol.Types.Response
{
    public enum ResponseType
    {
        Error,
        Ok,
    }
}
