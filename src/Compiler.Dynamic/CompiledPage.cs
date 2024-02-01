// MIT License.

using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal class CompiledPage : ICompiledPage
{
    public CompiledPage(PagePath path, string[] dependencies)
    {
        Path = path.UrlPath;
        FileDependencies = dependencies;
        AspxFile = path.FilePath;
    }

    public static ICompiledPage FromError(PagePath page, Exception ex)
        => new CompiledPage(page, [])
        {
            Exception = ex,
        };

    public Type? Type { get; set; }

    public Exception? Exception { get; set; }

    public PathString Path { get; }

    public IReadOnlyCollection<string> FileDependencies { get; }

    public ICollection<ICompiledPage> PageDependencies { get; } = new HashSet<ICompiledPage>();

    public string AspxFile { get; }

    public MetadataReference? MetadataReference { get; set; }

    public virtual void Dispose()
    {
        if (Type is not null)
        {
            var type = Type;
            Type = null;
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
