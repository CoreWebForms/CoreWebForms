// MIT License.

namespace WebForms.Compiler.Dynamic;

internal interface IPageCompiler
{
    Task<ICompiledPage> CompilePageAsync(string path, CancellationToken token);
}
