using System;
using MajWebSocket.Types;
using MajWebSocket.Types.Request;
using MajWebSocket.Types.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MajWebSocket.Serialization
{
    public static class MajWsSerializer
    {
        public static readonly JsonSerializerSettings DefaultSettings = new
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter()
        },
            FloatFormatHandling = FloatFormatHandling.Symbol,
            Formatting = Formatting.None,
        };

    public static string Serialize<T>(T value)
        => JsonConvert.SerializeObject(value, DefaultSettings);

    public static string Serialize<T>(T value, JsonSerializerSettings settings)
        => JsonConvert.SerializeObject(value, settings);

    public static T? Deserialize<T>(string json) where T : struct
        => JsonConvert.DeserializeObject<T>(json, DefaultSettings);

    public static T? Deserialize<T>(string json, JsonSerializerSettings settings) where T : struct
        => JsonConvert.DeserializeObject<T>(json, settings);


    private static JToken ToJToken<T>(T value) where T : struct
        => JToken.FromObject(value, JsonSerializer.Create(DefaultSettings));

    public static RequestEnvelope PackRequest<T>(RequestType type, T? payload) where T : struct
        => new()
        {
            ProtocolVersion = ProtocolVersion.Current,
            RequestType = type,
            RequestData = payload.HasValue ? ToJToken(payload.Value) : null,
        };

    public static ResponseEnvelope PackResponse<T>(ResponseType type, T? payload) where T : struct
        => new()
        {
            ProtocolVersion = ProtocolVersion.Current,
            ResponseType = type,
            ResponseData = payload.HasValue ? ToJToken(payload.Value) : null,
        };

    public static T? DeserializeRequestPayload<T>(RequestEnvelope envelope) where T : struct
        => envelope.RequestData?.ToObject<T>(JsonSerializer.Create(DefaultSettings));

    public static T? DeserializeResponsePayload<T>(ResponseEnvelope envelope) where T : struct
        => envelope.ResponseData?.ToObject<T>(JsonSerializer.Create(DefaultSettings));
}
}
