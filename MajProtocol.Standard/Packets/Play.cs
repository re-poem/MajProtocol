namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Play, true)]
public partial struct PlayRequest
{
    public double position;
    public float speed;
    public bool showIntroAnimation;
    public bool record;
}

[Packet(OpCode.Play, false)]
public partial struct PlayResponse
{
    public bool accepted;
}
