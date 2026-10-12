using System;
using MajProtocol.Generated;
using MajProtocol.Standard;
using MajProtocol.Standard.Packets;
using WebSocketSharp;

namespace MajProtocol.Client.WebSocketSharp;

public class MajWebSocketClient : IMajProtocolClient, IDisposable
{
    private static MajWebSocketClient? _instance;

    private readonly WebSocket _webSocket;

    public static event HandshakeResponse.Callback? OnHandshake;
    public static event FilesResponse.Callback?     OnFiles;
    public static event SimaiResponse.Callback?     OnSimai;
    public static event PlayResponse.Callback?      OnPlay;
    public static event PauseResponse.Callback?     OnPause;
    public static event OptionsResponse.Callback?   OnOptions;
    public static event StateResponse.Callback?     OnState;
    public static event UnloadResponse.Callback?    OnUnload;

    public static WebSocketState State => _instance?._webSocket.ReadyState ?? WebSocketState.Closed;

    private MajWebSocketClient(string url)
    {
        _webSocket           =  new WebSocket(url);
        _webSocket.OnMessage += OnMessage;
        _webSocket.OnOpen    += OnOpen;
        _webSocket.Connect();
    }

    private static void OnOpen(object sender, EventArgs e) =>
        Send(new HandshakeRequest { protocolVersion = Properties.ProtocolVersion });


    public static void Send(byte[] data) => _instance?._webSocket.Send(data);

    public static void Start(string url)
    {
        _instance?.Dispose();
        _instance = new MajWebSocketClient(url);
    }

    private static void OnMessage(object sender, MessageEventArgs e)
    {
        var span = e.RawData.AsSpan();
        ((IMajProtocolClient?)_instance)?.Handle(span);
    }

    public static void Close()
    {
        _instance?.Dispose();
    }

    public void OnFilesResponseReceived(FilesResponse req)
    {
        OnFiles?.Invoke(req);
    }

    public void OnHandshakeResponseReceived(HandshakeResponse req)
    {
        OnHandshake?.Invoke(req);
    }

    public void OnOptionsResponseReceived(OptionsResponse req)
    {
        OnOptions?.Invoke(req);
    }

    public void OnPauseResponseReceived(PauseResponse req)
    {
        OnPause?.Invoke(req);
    }

    public void OnPlayResponseReceived(PlayResponse req)
    {
        OnPlay?.Invoke(req);
    }

    public void OnSimaiResponseReceived(SimaiResponse req)
    {
        OnSimai?.Invoke(req);
    }

    public void OnStateResponseReceived(StateResponse req)
    {
        OnState?.Invoke(req);
    }

    public void OnUnloadResponseReceived(UnloadResponse req)
    {
        OnUnload?.Invoke(req);
    }

    public void Dispose() => Close();
}
