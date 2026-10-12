namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Play, true)]
public partial struct PlayRequest
{
    public double position;
    public bool   showIntroAnimation;
    public string saveRecordingPath;
}

[Packet(OpCode.Play, false)]
public partial struct PlayResponse
{
    public bool accepted;
}
