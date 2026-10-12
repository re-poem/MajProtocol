using System;
using MajProtocol.Generated;
using MajProtocol.Standard;
using MajProtocol.Standard.Packets;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MajProtocol.Server.WebSocketSharp;

public class MajWebSocketServer : WebSocketBehavior, IMajProtocolServer
{
    private static WebSocketServer? _instance;

    public static bool Active => _instance?.IsListening ?? false;

    public static event FilesRequest.Callback?   OnFiles;
    public static event SimaiRequest.Callback?   OnSimai;
    public static event PlayRequest.Callback?    OnPlay;
    public static event PauseRequest.Callback?   OnPause;
    public static event OptionsRequest.Callback? OnOptions;
    public static event StateRequest.Callback?   OnState;
    public static event UnloadRequest.Callback?  OnUnload;

    public static void Start(int port)
    {
        _instance?.Stop();
        var socket = new WebSocketServer($"ws://0.0.0.0:{port}");
        socket.AddWebSocketService<MajWebSocketServer>("/majdata");
        socket.Start();
        _instance = socket;
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        var span = e.RawData.AsSpan();
        ((IMajProtocolServer)this).Handle(span);
    }

    public static void Close()
    {
        _instance?.Stop();
    }

    public void OnFilesRequestReceived(FilesRequest req)
    {
        OnFiles?.Invoke(req);
    }

    public void OnHandshakeRequestReceived(HandshakeRequest req)
    {
        if (req.protocolVersion != Properties.ProtocolVersion)
        {
            Send(new HandshakeResponse { accepted = false });
            Close();
        }
    }

    public void OnOptionsRequestReceived(OptionsRequest req)
    {
        OnOptions?.Invoke(req);
    }

    public void OnPauseRequestReceived(PauseRequest req)
    {
        OnPause?.Invoke(req);
    }

    public void OnPlayRequestReceived(PlayRequest req)
    {
        OnPlay?.Invoke(req);
    }

    public void OnSimaiRequestReceived(SimaiRequest req)
    {
        OnSimai?.Invoke(req);
    }

    public void OnStateRequestReceived(StateRequest req)
    {
        OnState?.Invoke(req);
    }

    public void OnUnloadRequestReceived(UnloadRequest req)
    {
        OnUnload?.Invoke(req);
    }
}
