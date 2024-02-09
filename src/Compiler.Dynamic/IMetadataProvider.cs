// MIT License.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal interface IMetadataProvider
{
    IEnumerable<MetadataReference> References { get; }

    IEnumerable<Assembly> ControlAssemblies { get; }
}
