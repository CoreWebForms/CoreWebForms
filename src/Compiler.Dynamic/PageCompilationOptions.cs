// MIT License.

using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler.Dynamic;

public class PageCompilationOptions
{
    public PageCompilationOptions()
    {
    }

    public bool IsDebug { get; set; }
    internal IFileProvider WebFormsFileProvider { get; set; } = default!;

    internal Dictionary<string, Func<string, BaseCodeDomTreeGenerator>> Parsers { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void AddParser<TParser>(string extension)
        where TParser : BaseTemplateParser, new()
    {
        Parsers.Add(extension, Create);

        BaseCodeDomTreeGenerator Create(string path)
        {
            var parser = new TParser();

            parser.WebFormsFileProvider = WebFormsFileProvider;
            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);

            return parser.GetGenerator();
        }
    }
}
