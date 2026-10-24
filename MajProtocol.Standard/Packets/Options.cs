namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Options, true)]
public partial struct OptionsRequest
{
    public float tapSpeed;
    public float touchSpeed;

    public float trackVolume;
    public float answerVolume;
    public float tapVolume;
    public float slideVolume;
    public float breakVolume;
    public float breakSlideVolume;
    public float exVolume;
    public float touchVolume;
    public float hanabiVolume;

    public string extra;
}

[Packet(OpCode.Options, false)]
public partial struct OptionsResponse
{
    public bool accepted;
}
