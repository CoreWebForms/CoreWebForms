// MIT License.

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

    internal Dictionary<string, Func<string, BaseTemplateParser>> Parsers { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void AddParser<TParser>(string extension)
        where TParser : BaseTemplateParser, new()
    {
        Parsers.Add(extension, Create);

        BaseTemplateParser Create(string path) => new TParser
        {
            CurrentVirtualPath = path,
            WebFormsFileProvider = WebFormsFileProvider
        };
    }
}
