// MIT License.

#if !NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
#endif
using System.Reflection.Metadata;

namespace WebForms;

internal static class AssemblyAttributeUtility
{
    public static bool HasAttribute<T>(this MetadataReader reader)
        => reader.HasAttribute(typeof(T).Name, typeof(T).Namespace);

    public static bool HasAttribute(this MetadataReader reader, string typeName, string typeNamespace)
        => reader.GetAttributes(typeName, typeNamespace).Any();

    public static IEnumerable<CustomAttribute> GetAttributes(this MetadataReader reader, string typeName, string typeNamespace)
    {
        foreach (var a in reader.CustomAttributes)
        {
            var attribute = reader.GetCustomAttribute(a);
            var attributeCtor = attribute.Constructor;

            StringHandle attributeTypeName = default;
            StringHandle attributeTypeNamespace = default;

            if (attributeCtor.Kind == HandleKind.MemberReference)
            {
                var attributeMemberParent = reader.GetMemberReference((MemberReferenceHandle)attributeCtor).Parent;
                if (attributeMemberParent.Kind == HandleKind.TypeReference)
                {
                    var attributeTypeRef = reader.GetTypeReference((TypeReferenceHandle)attributeMemberParent);
                    attributeTypeName = attributeTypeRef.Name;
                    attributeTypeNamespace = attributeTypeRef.Namespace;
                }
            }
            else if (attributeCtor.Kind == HandleKind.MethodDefinition)
            {
                var attributeTypeDefHandle = reader.GetMethodDefinition((MethodDefinitionHandle)attributeCtor).GetDeclaringType();
                var attributeTypeDef = reader.GetTypeDefinition(attributeTypeDefHandle);
                attributeTypeName = attributeTypeDef.Name;
                attributeTypeNamespace = attributeTypeDef.Namespace;
            }

            if (!attributeTypeName.IsNil &&
                !attributeTypeNamespace.IsNil &&
                reader.StringComparer.Equals(attributeTypeName, typeName) &&
                reader.StringComparer.Equals(attributeTypeNamespace, typeNamespace))
            {
                yield return attribute;
            }
        }
    }
}
