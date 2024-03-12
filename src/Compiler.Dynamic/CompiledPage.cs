// MIT License.

using System.Runtime.Loader;
using System.Web;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal sealed class CompiledPage(VirtualPath path)
{
    private Type? _type;

    public Type? Type
    {
        get => _type;
        init => _type = value;
    }

    public Exception? Exception { get; init; }

    public VirtualPath Path { get; } = path;

    public MetadataReference? MetadataReference { get; init; }

    public void Dispose()
    {
        if (_type is { } type)
        {
            _type = null;
            RemovePage(type);
        }
    }

    private static void RemovePage(Type type)
    {
        var alc = AssemblyLoadContext.GetLoadContext(type.Assembly);

        if (alc is not PageAssemblyLoadContext)
        {
            throw new InvalidOperationException("Tried to unload something that is not a page");
        }

        alc.Unload();
    }
}
