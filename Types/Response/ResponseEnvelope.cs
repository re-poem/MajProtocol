using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MajProtocol.Types.Response
{
    public readonly struct ResponseEnvelope
    {
        public int? ProtocolVersion { get; init; }
        public ResponseType ResponseType { get; init; }
        public JToken? ResponseData { get; init; }
    }
}
