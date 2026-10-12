namespace MajProtocol.Standard.Packets;

[Packet(OpCode.Unload, true)]
public partial struct UnloadRequest;

[Packet(OpCode.Unload, false)]
public partial struct UnloadResponse
{
    public bool accepted;
}
