using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MajWebSocket.Types.Response
{
    public readonly struct ResponseEnvelope
    {
        [JsonProperty("protocolVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int? ProtocolVersion { get; init; }

        [JsonProperty("responseType", Required = Required.Always)]
        public ResponseType ResponseType { get; init; }

        [JsonProperty("responseData", NullValueHandling = NullValueHandling.Ignore)]
        public JToken? ResponseData { get; init; }
    }
}
