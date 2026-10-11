using System;

namespace MajProtocol.Standard;

public sealed class PacketAttribute(OpCode opCode) : Attribute
{
    public OpCode OpCode { get; } = opCode;
}
