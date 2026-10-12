using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MajProtocol.Standard.Generator;

[Generator]
public class PacketGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<PacketInfo> packetProvider = context.SyntaxProvider
                                                                      .ForAttributeWithMetadataName(
                                                                          fullyQualifiedMetadataName:
                                                                          "MajProtocol.Standard.PacketAttribute",
                                                                          predicate: static (node, _) =>
                                                                              node is StructDeclarationSyntax,
                                                                          transform: static (ctx, _) => GetPacketInfo(ctx))
                                                                      .Where(static info => info is not null)!;

        context.RegisterSourceOutput(packetProvider, static (spc, packetInfo) =>
        {
            var source = GeneratePacket(packetInfo);
            spc.AddSource($"{packetInfo.Name}_Packet.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        var collectedPackets = packetProvider.Collect();

        context.RegisterSourceOutput(collectedPackets, static (spc, packets) =>
        {
            spc.AddSource("IMajProtocolClient.g.cs",
                          SourceText.From(GenerateRoutingClass("IMajProtocolClient", packets.Where(p => !p.IsServer)),
                                          Encoding.UTF8));
            spc.AddSource("IMajProtocolServer.g.cs",
                          SourceText.From(GenerateRoutingClass("IMajProtocolServer", packets.Where(p => p.IsServer)),
                                          Encoding.UTF8));
        });
    }

    private static PacketInfo? GetPacketInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        if (typeSymbol.TypeKind != TypeKind.Struct)
            return null;

        var attrData       = context.Attributes[0];
        var opCodeArgument = attrData.ConstructorArguments[0];
        var serverArgument = attrData.ConstructorArguments[1];

        var opCode   = opCodeArgument.Value?.ToString() ?? "0";
        var isServer = (bool)(serverArgument.Value ?? false);

        var fields = typeSymbol.GetMembers()
                               .OfType<IFieldSymbol>()
                               .Where(f => f.DeclaredAccessibility == Accessibility.Public &&
                                           f is { IsStatic: false, IsConst: false })
                               .Select(f => new FieldInfo(
                                           Name: f.Name,
                                           Type: f.Type.ToDisplayString(),
                                           IsString: f.Type.SpecialType == SpecialType.System_String,
                                           IsUnmanaged: f.Type.IsUnmanagedType
                                       ))
                               .ToImmutableArray();

        return new PacketInfo(
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : typeSymbol.ContainingNamespace.ToDisplayString(),
            Name: typeSymbol.Name,
            OpCode: opCode,
            IsServer: isServer,
            Fields: fields
        );
    }

    private static string GeneratePacket(PacketInfo info)
    {
        var stringDecls = new StringBuilder();
        var sizeCalc    = new StringBuilder().Append("        int totalSize = 1");
        var fieldWrites = new StringBuilder();
        var fieldReads  = new StringBuilder();

        for (var i = 0; i < info.Fields.Length; i++)
        {
            var field = info.Fields[i];
            if (field.IsString)
            {
                stringDecls.AppendLine($"        var {field.Name}Bytes = Encoding.UTF8.GetBytes(this.{field.Name});");
                stringDecls.AppendLine($"        var {field.Name}BytesLength = {field.Name}Bytes.Length;");

                sizeCalc.Append($" + sizeof(int) + {field.Name}BytesLength");

                fieldWrites.AppendLine($"        MemoryMarshal.Write(remainingSpan, ref {field.Name}BytesLength);");
                fieldWrites.AppendLine("        remainingSpan = remainingSpan[sizeof(int)..];");
                fieldWrites.AppendLine($"        {field.Name}Bytes.CopyTo(remainingSpan);");
                if (i < info.Fields.Length - 1)
                    fieldWrites.AppendLine($"        remainingSpan = remainingSpan[{field.Name}BytesLength..];");

                fieldReads.AppendLine($"        int {field.Name}Len = MemoryMarshal.Read<int>(remainingSpan);");
                fieldReads.AppendLine("        remainingSpan = remainingSpan[sizeof(int)..];");
                fieldReads.AppendLine(
                    $"        packet.{field.Name} = Encoding.UTF8.GetString(remainingSpan[..{field.Name}Len]);");
                if (i < info.Fields.Length - 1)
                    fieldReads.AppendLine($"        remainingSpan = remainingSpan[{field.Name}Len..];");
            }
            else if (field.IsUnmanaged)
            {
                sizeCalc.Append($" + sizeof({field.Type})");

                fieldWrites.AppendLine($"        var {field.Name}Value = this.{field.Name};");
                fieldWrites.AppendLine($"        MemoryMarshal.Write(remainingSpan, ref {field.Name}Value);");
                if (i < info.Fields.Length - 1)
                    fieldWrites.AppendLine($"        remainingSpan = remainingSpan[sizeof({field.Type})..];");

                fieldReads.AppendLine($"        packet.{field.Name} = MemoryMarshal.Read<{field.Type}>(remainingSpan);");
                if (i < info.Fields.Length - 1)
                    fieldReads.AppendLine($"        remainingSpan = remainingSpan[sizeof({field.Type})..];");
            }
        }

        sizeCalc.AppendLine(";");

        var namespaceLine = string.IsNullOrEmpty(info.Namespace) ? "" : $"namespace {info.Namespace};\n\n";

        return $$"""
                 // <auto-generated/>
                 using System;
                 using System.Runtime.InteropServices;
                 using System.Text;

                 {{namespaceLine}}public partial struct {{info.Name}}
                 {
                     public delegate void Callback({{info.Name}} value);
                     
                     public static implicit operator byte[]({{info.Name}} v) => v.ToBytes();

                     public unsafe byte[] ToBytes()
                     {
                 {{stringDecls}}{{sizeCalc}}
                         var buffer        = new byte[totalSize];
                         var remainingSpan = buffer.AsSpan();

                         remainingSpan[0] = (byte){{info.OpCode}};
                         remainingSpan = remainingSpan[1..];

                 {{fieldWrites}}
                         return buffer;
                     }

                     public static unsafe {{info.Name}} FromBytes(ReadOnlySpan<byte> data)
                     {
                         var packet        = new {{info.Name}}();
                         var remainingSpan = data[1..];

                 {{fieldReads}}
                         return packet;
                     }
                 }
                 """;
    }

    private static string GenerateRoutingClass(string className, IEnumerable<PacketInfo> packets)
    {
        var switchCases     = new StringBuilder();
        var abstractMethods = new StringBuilder();

        foreach (var packet in packets)
        {
            var fullName = string.IsNullOrEmpty(packet.Namespace) ? packet.Name : $"{packet.Namespace}.{packet.Name}";

            switchCases.AppendLine($"            case {packet.OpCode}:");
            switchCases.AppendLine($"                On{packet.Name}Received({fullName}.FromBytes(data));");
            switchCases.AppendLine("                break;");

            abstractMethods.AppendLine($"    void On{packet.Name}Received({fullName} req);");
        }

        return $$"""
                 // <auto-generated/>
                 using System;

                 namespace MajProtocol.Generated;

                 public interface {{className}}
                 {
                     public void Handle(ReadOnlySpan<byte> data)
                     {
                         if (data.Length < 1) return;
                         byte opCode = data[0];

                         switch (opCode)
                         {
                 {{switchCases}}
                             default:
                                 break;
                         }
                     }

                 {{abstractMethods}}
                 }
                 """;
    }
}

internal record PacketInfo(
    string                    Namespace,
    string                    Name,
    string                    OpCode,
    bool                      IsServer,
    ImmutableArray<FieldInfo> Fields);

internal record FieldInfo(string Name, string Type, bool IsString, bool IsUnmanaged);
