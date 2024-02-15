// MIT License.

using System.Reflection;
using System.Web.Compilation;
using System.Web.UI;

namespace WebForms.Compiler.Dynamic;

public class PageCompilationOptions
{
    public PageCompilationOptions()
    {
    }

    public bool IsDebug { get; set; }

    internal Dictionary<string, Func<string, BaseCodeDomTreeGenerator>> Parsers { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void AddParser<TParser>(string extension)
        where TParser : BaseTemplateParser, new()
    {
        Parsers.Add(extension, Create);

        static BaseCodeDomTreeGenerator Create(string path)
        {
            var parser = new TParser();

            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);

            return parser.GetGenerator();
        }
    }
}
