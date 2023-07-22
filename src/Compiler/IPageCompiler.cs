// MIT License.

using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler;

internal interface IPageCompiler
{
    Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token);
}
