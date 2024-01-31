// MIT License.

namespace WebForms.Compiler.Dynamic;

public interface IWebFormsCompiler
{

    Task CompilePagesAsync(CancellationToken token);
}
