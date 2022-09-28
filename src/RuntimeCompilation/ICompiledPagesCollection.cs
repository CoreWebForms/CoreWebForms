// MIT License.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface ICompiledPagesCollection : IDisposable
{
    IReadOnlyList<ICompiledPage> Pages { get; }

    IChangeToken ChangeToken { get; }
}
