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

    internal void AddParser<DParser, TParser>(string extension)
        where DParser : DependencyParser, new()
        where TParser : BaseTemplateParser, new()
    {
        DependencyParser Create(string path, ICompiledTypeAccessor compiledTypeAccessor)
        {
            var virtualPath = new System.Web.VirtualPath(path);
            var templateParser = new TParser
            {
                CurrentVirtualPath = virtualPath,
                WebFormsFileProvider = WebFormsFileProvider,
                CompiledTypeAccessor = compiledTypeAccessor
            };

            var dependencyParser = new DParser
            {
                WebFormsFileProvider = WebFormsFileProvider,
                TemplateParser = templateParser
            };

            dependencyParser.Init(virtualPath);
            return dependencyParser;
        }

        Parsers.Add(extension, Create);
    }

}
