namespace MajProtocol.Standard.Packets;

[Packet(OpCode.State, true)]
public partial struct StateRequest;

[Packet(OpCode.State, false)]
public partial struct StateResponse
{
    public double position;
    public bool   isPlaying;
}
