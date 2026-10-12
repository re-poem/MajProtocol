namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Files, true)]
public partial struct FilesRequest
{
    public string trackPath;
    public string jacketPath;
    public string moviePath;
}

[Packet(OpCode.Files, false)]
public partial struct FilesResponse
{
    public bool accepted;
}
