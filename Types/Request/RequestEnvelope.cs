using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MajProtocol.Types.Request
{
    public readonly struct RequestEnvelope
    {
        public int? ProtocolVersion { get; init; }
        public RequestType RequestType { get; init; }
        public JToken? RequestData { get; init; }
    }
}
