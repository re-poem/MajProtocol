namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Simai, true)]
public partial struct SimaiRequest
{
    public double offset;
    public string title;
    public string artist;
    public string designer;
    public string difficultyValue;
    public int    difficultyIndex;
    public string content;
}

[Packet(OpCode.Simai, false)]
public partial struct SimaiResponse
{
    public bool accepted;
}
