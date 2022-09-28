// MIT License.

using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface IPageCompiler
{
    Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token);
}
