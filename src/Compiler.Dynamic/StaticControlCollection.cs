// MIT License.

using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Web.UI;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal sealed class StaticControlCollection : AssemblyLoadContext, IDisposable, ITypeResolutionService, IMetadataProvider
{
    private readonly DynamicControlCollection _other;

    public StaticControlCollection(DynamicControlCollection other)
        : base("WebForms Compilation Context")
    {
        _other = other;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return base.Load(assemblyName);
    }

    public IEnumerable<MetadataReference> References => ((IMetadataProvider)_other).References;

    public IEnumerable<Assembly> ControlAssemblies => ((IMetadataProvider)_other).ControlAssemblies;

    public IEnumerable<TagNamespaceRegisterEntry> TagRegistrations => ((IMetadataProvider)_other).TagRegistrations;

    public void Dispose()
    {
        ((IDisposable)_other).Dispose();
    }

    public Assembly? GetAssembly(AssemblyName name)
    {
        return ((ITypeResolutionService)_other).GetAssembly(name);
    }

    public Assembly? GetAssembly(AssemblyName name, bool throwOnError)
    {
        return ((ITypeResolutionService)_other).GetAssembly(name, throwOnError);
    }

    public string? GetPathOfAssembly(AssemblyName name)
    {
        return ((ITypeResolutionService)_other).GetPathOfAssembly(name);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name)
    {
        return ((ITypeResolutionService)_other).GetType(name);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name, bool throwOnError)
    {
        return ((ITypeResolutionService)_other).GetType(name, throwOnError);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name, bool throwOnError, bool ignoreCase)
    {
        return ((ITypeResolutionService)_other).GetType(name, throwOnError, ignoreCase);
    }

    public void ReferenceAssembly(AssemblyName name)
    {
        ((ITypeResolutionService)_other).ReferenceAssembly(name);
    }
}
