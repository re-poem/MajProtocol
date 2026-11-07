namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Files, true)]
public partial struct FilesRequest
{
    public byte[]? track;
    public byte[]? jacket;
    public byte[]? movie;
}

[Packet(OpCode.Files, false)]
public partial struct FilesResponse
{
    public bool accepted;
}
