namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Handshake, true)]
public partial struct HandshakeRequest
{
    public int protocolVersion;
}

[Packet(OpCode.Handshake, false)]
public partial struct HandshakeResponse
{
    public bool accepted;
}
