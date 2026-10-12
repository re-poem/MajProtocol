namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Pause, true)]
public partial struct PauseRequest;

[Packet(OpCode.Pause, false)]
public partial struct PauseResponse
{
    public bool accepted;
}
