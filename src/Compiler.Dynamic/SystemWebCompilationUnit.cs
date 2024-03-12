// MIT License.

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using WebForms.Features;
using WebForms.Internal;

namespace WebForms.Compiler.Dynamic;

internal sealed class SystemWebCompilationUnit(ICompilationStrategy provider) : IWebFormsCompilationFeature
{
    private readonly Dictionary<VirtualPath, CompiledPage> _cache = new();

    public ICompilationStrategy Strategy { get; } = provider;

    public IEnumerable<CompiledPage> Values => _cache.Values;

    IReadOnlyCollection<string> IWebFormsCompilationFeature.Paths => _cache.Keys.Select(k => k.Path).ToList();

    public CompiledPage this[VirtualPath path]
    {
        get => _cache[path];
        set => _cache[path] = value;
    }

    bool IWebFormsCompilationFeature.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
    {
        if (_cache.TryGetValue(new(path), out var page) && page.Exception is { } e)
        {
            exception = e;
            return true;
        }

        exception = null;
        return false;
    }

    Type? IWebFormsCompilationFeature.GetForPath(string virtualPath)
        => _cache.TryGetValue(new(virtualPath), out var page) && page.Type is { } type ? type : null;

    public ICompilationResult Build() => new BuiltDynamicCompilation(_cache);

    private sealed class BuiltDynamicCompilation : IWebFormsCompilationFeature, ICompilationResult
    {
        private FrozenDictionary<string, CompiledPage>? _pages;

        public BuiltDynamicCompilation(Dictionary<VirtualPath, CompiledPage> pages)
        {
            _pages = pages.ToFrozenDictionary(p => p.Key.Path, p => p.Value, PathComparer.Instance);
        }

        IWebFormsCompilationFeature ICompilationResult.Types => this;

        IReadOnlyCollection<string> IWebFormsCompilationFeature.Paths => _pages?.Keys ?? throw new ObjectDisposedException(GetType().FullName);

        Type? IWebFormsCompilationFeature.GetForPath(string virtualPath)
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

        bool IWebFormsCompilationFeature.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
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
