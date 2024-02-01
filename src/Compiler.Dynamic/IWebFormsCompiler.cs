// MIT License.

using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler.Dynamic;

public interface IWebFormsCompiler
{
    IFileProvider Files { get; }

    Task CompilePagesAsync(CancellationToken token);
}
