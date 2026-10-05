using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MajProtocol.Types.Response
{
    public enum ResponseType
    {
        Error = 400,
        Ok = 200,
        PlayStarted = 201,
        PlayResumed = 202,
        Heartbeat = 203,
        PlayPaused = 204,
        PlayStopped = 205,
        LoadOk = 206,
    }
}
