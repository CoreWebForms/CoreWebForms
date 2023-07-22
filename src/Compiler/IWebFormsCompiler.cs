// MIT License.

using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler;

public interface IWebFormsCompiler
{

    Task CompilePagesAsync(CancellationToken token);
}
