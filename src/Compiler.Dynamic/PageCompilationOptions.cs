// MIT License.

using System.Web.UI;
using Microsoft.Extensions.FileProviders;
using WebForms.Internal;

namespace WebForms.Compiler.Dynamic;

public class PageCompilationOptions
{
    public PageCompilationOptions()
    {
    }

    public bool IsDebug { get; set; }

    internal IFileProvider WebFormsFileProvider { get; set; } = default!;

    internal Dictionary<string, Func<string, ICompiledTypeAccessor, DependencyParser>> Parsers { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void AddParser<DParser>(string extension)
        where DParser : DependencyParser, new()
    {
        DependencyParser Create(string path, ICompiledTypeAccessor compiledTypeAccessor)
        {
            var virtualPath = new System.Web.VirtualPath(path);
            var dependencyParser = new DParser
            {
                WebFormsFileProvider = WebFormsFileProvider,
                CompiledTypeAccessor = compiledTypeAccessor,
            };
            dependencyParser.Init(virtualPath);

            return dependencyParser;
        }

        Parsers.Add(extension, Create);
    }

}
