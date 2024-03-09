// MIT License.

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using WebForms.Internal;

namespace WebForms.Compiler.Dynamic;

internal sealed class SystemWebCompilationUnit(ICompilationStrategy provider) : ICompiledTypeAccessor
{
    private readonly Dictionary<string, CompiledPage> _cache = new(PathComparer.Instance);

    public ICompilationStrategy OutputProvider { get; } = provider;

    public IEnumerable<CompiledPage> Values => _cache.Values;

    IReadOnlyCollection<string> ICompiledTypeAccessor.Paths => _cache.Keys;

    public CompiledPage this[string path]
    {
        get => _cache[path];
        set => _cache[path] = value;
    }

    bool ICompiledTypeAccessor.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
    {
        if (_cache.TryGetValue(path, out var page) && page.Exception is { } e)
        {
            exception = e;
            return true;
        }

        exception = null;
        return false;
    }

    Type? ICompiledTypeAccessor.GetForPath(string virtualPath)
        => _cache.TryGetValue(virtualPath, out var page) && page.Type is { } type ? type : null;

    public ICompilationResult Build() => new BuiltDynamicCompilation(_cache);

    private sealed class BuiltDynamicCompilation : ICompiledTypeAccessor, ICompilationResult
    {
        private FrozenDictionary<string, CompiledPage>? _pages;

        public BuiltDynamicCompilation(Dictionary<string, CompiledPage> pages)
        {
            _pages = pages.ToFrozenDictionary(PathComparer.Instance);
        }

        ICompiledTypeAccessor ICompilationResult.Types => this;

        IReadOnlyCollection<string> ICompiledTypeAccessor.Paths => _pages?.Keys ?? throw new ObjectDisposedException(GetType().FullName);

        Type? ICompiledTypeAccessor.GetForPath(string virtualPath)
        {
            if (_pages is not { } pages)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            return pages.TryGetValue(virtualPath, out var page) && page.Type is { } type ? type : null;
        }

        public void Dispose()
        {
            if (_pages is { } pages)
            {
                _pages = null;

                foreach (var page in pages.Values)
                {
                    page.Dispose();
                }
            }
        }

        bool ICompiledTypeAccessor.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
        {
            if (_pages is not { } pages)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (pages.TryGetValue(path, out var page) && page.Exception is { } e)
            {
                exception = e;
                return true;
            }

            exception = null;
            return false;
        }
    }
}
