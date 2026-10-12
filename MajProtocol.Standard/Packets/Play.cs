namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Play, true)]
public partial struct PlayRequest
{
    public double position;
}

[Packet(OpCode.Play, false)]
public partial struct PlayResponse
{
    public bool accepted;
}
