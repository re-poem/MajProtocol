namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Simai, true)]
public partial struct SimaiRequest
{
    public double offset;
    public string content;
}

[Packet(OpCode.Simai, false)]
public partial struct SimaiResponse
{
    public bool accepted;
}
