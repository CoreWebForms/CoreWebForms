// MIT License.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using WebForms;

namespace Compiler.Generator;

internal static class MetadataDecoderExtensions
{
    public static IEnumerable<(INamedTypeSymbol, string)> FindPreApplicationStartAttributes(this Compilation compilation)
    {
        foreach (var metadataReference in compilation.References)
        {
            if (metadataReference is PortableExecutableReference pe)
            {
                if (compilation.GetAssemblyOrModuleSymbol(metadataReference) is IAssemblySymbol assemblySymbol)
                {
                    if (pe.GetMetadata() is AssemblyMetadata assemblyMetadata)
                    {
                        foreach (var module in assemblyMetadata.GetModules())
                        {
                            var reader = module.GetMetadataReader();

                            foreach (var startAttribute in reader.GetAttributes("PreApplicationStartMethodAttribute", "System.Web"))
                            {
                                var result = startAttribute.DecodeValue(new Decoder());

                                if (result.FixedArguments is [{ Type: DecodedType, Value: Value { Text: { } type } }, { Type: PrimitiveType { Type: PrimitiveTypeCode.String }, Value: string method }])
                                {
                                    if (assemblySymbol.GetTypeByMetadataName(type) is { } typeSymbol)
                                    {
                                        yield return (typeSymbol, method);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        yield break;
    }

    private interface IDecodedInfo
    {
    }

    private record DecodedString : SystemType, IDecodedInfo;

    private record DecodedType : SystemType, IDecodedInfo;

    private record SystemType : IDecodedInfo;

    private record Value(string Text) : IDecodedInfo;

    private record UnsupportedType(string Name, string Namespace) : IDecodedInfo;

    private record PrimitiveType(PrimitiveTypeCode Type) : IDecodedInfo;

    private record ArrayType(IDecodedInfo Type) : IDecodedInfo;

    private sealed class Decoder : ICustomAttributeTypeProvider<IDecodedInfo>
    {
        public IDecodedInfo GetPrimitiveType(PrimitiveTypeCode typeCode)
            => new PrimitiveType(typeCode);

        public IDecodedInfo GetSystemType()
        {
            throw new NotImplementedException();
        }

        public IDecodedInfo GetSZArrayType(IDecodedInfo elementType)
            => new ArrayType(elementType);

        public IDecodedInfo GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var r = reader.GetTypeDefinition(handle);

            // We only support string and type right now for PreApplicationStartMethodAttribute
            if (reader.StringComparer.Equals(r.Namespace, "System"))
            {
                if (reader.StringComparer.Equals(r.Name, "String"))
                {
                    return new DecodedString();
                }
                else if (reader.StringComparer.Equals(r.Name, "Type"))
                {
                    return new DecodedType();
                }
            }

            return new UnsupportedType(reader.GetString(r.Name), reader.GetString(r.Namespace));
        }

        public IDecodedInfo GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var r = reader.GetTypeReference(handle);

            // We only support string and type right now for PreApplicationStartMethodAttribute
            if (reader.StringComparer.Equals(r.Namespace, "System"))
            {
                if (reader.StringComparer.Equals(r.Name, "String"))
                {
                    return new DecodedString();
                }
                else if (reader.StringComparer.Equals(r.Name, "Type"))
                {
                    return new DecodedType();
                }
            }

            return new UnsupportedType(reader.GetString(r.Name), reader.GetString(r.Namespace));
        }

        public IDecodedInfo GetTypeFromSerializedName(string name) => new Value(name);

        public PrimitiveTypeCode GetUnderlyingEnumType(IDecodedInfo type)
        {
            throw new NotImplementedException();
        }

        public bool IsSystemType(IDecodedInfo type) => type is SystemType;
    }
}
