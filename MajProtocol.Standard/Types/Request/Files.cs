namespace MajProtocol.Standard.Types.Request;

[Packet(OpCode.Files)]
public partial struct Files
{
    public string trackPath;
    public string jacketPath;
    public string moviePath;
}
