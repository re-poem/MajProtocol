namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Options, true)]
public partial struct OptionsRequest
{
    public string json;
}

[Packet(OpCode.Options, false)]
public partial struct OptionsResponse
{
    public bool accepted;
}
