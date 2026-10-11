namespace MajProtocol.Standard.Types.Request;

[Packet(OpCode.Handshake)]
public partial struct Handshake
{
    public int protocolVersion;
}
