using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MajProtocol.Types.Request
{
    public readonly struct RequestEnvelope
    {
        [JsonProperty("protocolVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int? ProtocolVersion { get; init; }

        [JsonProperty("requestType", Required = Required.Always)]
        public RequestType RequestType { get; init; }

        [JsonProperty("requestData", NullValueHandling = NullValueHandling.Ignore)]
        public JToken? RequestData { get; init; }
    }
}
