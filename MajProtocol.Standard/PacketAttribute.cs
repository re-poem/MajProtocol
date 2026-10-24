using System;

namespace MajProtocol.Standard;

#pragma warning disable CS9113 // Parameter is unread.
public sealed class PacketAttribute(OpCode opCode, bool server) : Attribute;
