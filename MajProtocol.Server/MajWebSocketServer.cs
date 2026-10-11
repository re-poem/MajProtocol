using System;
using System.Runtime.InteropServices;
using MajProtocol.Standard;
using MajProtocol.Standard.Types.Request;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MajProtocol.Server;

public class MajWebSocketServer : WebSocketBehavior
{
    private static WebSocketServer? _instance;
    public static  bool             active;

    public static void Start(int port)
    {
        _instance?.Stop();
        var socket = new WebSocketServer($"ws://0.0.0.0:{port}");
        socket.AddWebSocketService<MajWebSocketServer>("/majdata");
        socket.Start();
        _instance = socket;
        active    = true;
    }

    public static void Close()
    {
        _instance?.Stop();
        active = false;
    }

    protected override void OnClose(CloseEventArgs e)
    {
    }

    protected override void OnError(ErrorEventArgs e)
    {
    }

    protected override void OnOpen()
    {
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        var span   = e.RawData.AsSpan();
        var opCode = (OpCode)span[0];
        var data   = span[1..];
        switch (opCode)
        {
            case OpCode.Handshake:
                OnHandshake(data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnHandshake(Span<byte> data)
    {
        var handshake = MemoryMarshal.Read<Handshake>(data);
        handshake.protocolVersion
    }
}
