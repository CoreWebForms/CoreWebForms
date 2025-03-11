// MIT License.

using System.Reflection;
using System.Web.UI;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal interface IMetadataProvider
{
    IEnumerable<MetadataReference> References { get; }

    IEnumerable<Assembly> ControlAssemblies { get; }

    IEnumerable<TagNamespaceRegisterEntry> TagRegistrations { get; }
}
