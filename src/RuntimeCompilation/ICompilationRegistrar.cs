// MIT License.

using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface ICompilationRegistrar
{
    ICompiledPagesCollection Register(IFileProvider files);
}
