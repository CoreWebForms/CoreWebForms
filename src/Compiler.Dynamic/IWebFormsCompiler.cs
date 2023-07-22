// MIT License.

using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler.Dynamic;

public interface IWebFormsCompiler
{

    Task CompilePagesAsync(CancellationToken token);
}
