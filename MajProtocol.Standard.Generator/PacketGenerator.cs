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
    }

    private static PacketInfo? GetPacketInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        if (typeSymbol.TypeKind != TypeKind.Struct)
            return null;

        var opCodeArgument = context.Attributes[0].ConstructorArguments[0];
        var opCode         = opCodeArgument.Value?.ToString() ?? "0";

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
            Fields: fields
        );
    }

    private static string GeneratePacket(PacketInfo info)
    {
        var stringDecls = new StringBuilder();
        var sizeCalc    = new StringBuilder().Append("        int totalSize = 1");
        var fieldWrites = new StringBuilder();
        var fieldReads  = new StringBuilder();

        foreach (var field in info.Fields)
        {
            if (field.IsString)
            {
                stringDecls.AppendLine($"        var {field.Name}Bytes = Encoding.UTF8.GetBytes(this.{field.Name});");
                stringDecls.AppendLine($"        var {field.Name}BytesLength = {field.Name}Bytes.Length;");

                sizeCalc.Append($" + sizeof(int) + {field.Name}BytesLength");

                fieldWrites.AppendLine($"        MemoryMarshal.Write(remainingSpan, ref {field.Name}BytesLength);");
                fieldWrites.AppendLine("        remainingSpan = remainingSpan[sizeof(int)..];");
                fieldWrites.AppendLine($"        {field.Name}Bytes.CopyTo(remainingSpan);");
                fieldWrites.AppendLine($"        remainingSpan = remainingSpan[{field.Name}BytesLength..];");

                fieldReads.AppendLine($"        int {field.Name}Len = MemoryMarshal.Read<int>(remainingSpan);");
                fieldReads.AppendLine("        remainingSpan = remainingSpan[sizeof(int)..];");
                fieldReads.AppendLine(
                    $"        packet.{field.Name} = Encoding.UTF8.GetString(remainingSpan[..{field.Name}Len]);");
                fieldReads.AppendLine($"        remainingSpan = remainingSpan[{field.Name}Len..];");
            }
            else if (field.IsUnmanaged)
            {
                sizeCalc.Append($" + sizeof({field.Type})");

                fieldWrites.AppendLine($"        var {field.Name}Value = this.{field.Name};");
                fieldWrites.AppendLine($"        MemoryMarshal.Write(remainingSpan, ref {field.Name}Value);");
                fieldWrites.AppendLine($"        remainingSpan = remainingSpan[sizeof({field.Type})..];");

                fieldReads.AppendLine($"        packet.{field.Name} = MemoryMarshal.Read<{field.Type}>(remainingSpan);");
                fieldReads.AppendLine($"        remainingSpan = remainingSpan[sizeof({field.Type})..];");
            }
        }

        sizeCalc.AppendLine(";");

        var namespaceLine = string.IsNullOrEmpty(info.Namespace) ? "" : $"namespace {info.Namespace};\n\n";

        // Use an interpolated raw string literal for the main class structure
        // Added 'unsafe' modifiers to allow sizeof() on any unmanaged struct
        return $$"""
                 // <auto-generated/>
                 using System;
                 using System.Runtime.InteropServices;
                 using System.Text;

                 {{namespaceLine}}public partial struct {{info.Name}}
                 {
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

                     public static unsafe {{info.Name}} FromBytes(byte[] data)
                     {
                         var packet        = new {{info.Name}}();
                         var remainingSpan = data.AsSpan()[1..];

                 {{fieldReads}}
                         return packet;
                     }
                 }
                 """;
    }
}

internal record PacketInfo(string Namespace, string Name, string OpCode, ImmutableArray<FieldInfo> Fields);

internal record FieldInfo(string Name, string Type, bool IsString, bool IsUnmanaged);
