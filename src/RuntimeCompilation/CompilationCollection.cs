// MIT License.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class CompilationCollection : ICompiledPagesCollection
{
    private readonly IQueue _queue;
    private readonly IFileProvider _files;
    private readonly IPageCompiler _compiler;
    private readonly ILogger<CompilationCollection> _logger;

    private ImmutableList<Timed<ICompiledPage>> _compiledPages;
    private IReadOnlyList<ICompiledPage> _wrapped;

    private CancellationTokenSource _cts;
    private CancellationChangeToken _token;

    private readonly IDisposable _aspxFilter;
    private readonly IDisposable _siteFilter;

    public CompilationCollection(IFileProvider files, IPageCompiler compiler, IQueue queue, ILoggerFactory logger)
    {
        _queue = queue;
        _files = files;
        _compiler = compiler;
        _logger = logger.CreateLogger<CompilationCollection>();
        _compiledPages = ImmutableList<Timed<ICompiledPage>>.Empty;
        _wrapped = Array.Empty<ICompiledPage>();
        _cts = new CancellationTokenSource();
        _token = new CancellationChangeToken(_cts.Token);

        _aspxFilter = ChangeToken.OnChange(() => _files.Watch("**/*.aspx*"), OnFileChange);
        _siteFilter = ChangeToken.OnChange(() => _files.Watch("**/*.Master*"), OnFileChange);

        OnFileChange();
    }

    IReadOnlyList<ICompiledPage> ICompiledPagesCollection.Pages => _wrapped;

    IChangeToken ICompiledPagesCollection.ChangeToken => _token;

    private void OnFileChange()
    {
        _logger.LogTrace("Enqueueing update tracking");
        _queue.Add(UpdateTypesAsync);
    }

    private async Task UpdateTypesAsync(CancellationToken token)
    {
        var changedFiles = GetFileChanges();
        var finalPages = _compiledPages.ToBuilder();

        foreach (var file in changedFiles.Deletions)
        {
            _logger.LogTrace("Removing page {Path}", file.Item.Path);
            finalPages.Remove(file);
            file.Item.Dispose();
        }

        foreach (var file in changedFiles.Changes)
        {
            if (file.Item.CompiledPage is { } existing)
            {
                _logger.LogTrace("Removing {Path}", existing.Item.AspxFile);
                finalPages.Remove(existing);
                existing.Item.Dispose();
            }
            else
            {
                _logger.LogTrace("Creating page {Path}", file.Item.FullPath);
            }

            var aspx = file.Item.CompiledPage is { } compiled ? compiled.Item.AspxFile : file.Item.FullPath;

            var compilation = await _compiler.CompilePageAsync(_files, aspx, token).ConfigureAwait(false);

            _logger.LogTrace("Adding page {Path}", compilation.AspxFile);
            finalPages.Add(new(compilation, file.LastModified));
        }

        Interlocked.Exchange(ref _compiledPages, finalPages.ToImmutable());
        Interlocked.Exchange(ref _wrapped, new Wrapper<ICompiledPage>(_compiledPages));

        var current = _cts;

        _cts = new();
        _token = new CancellationChangeToken(_cts.Token);

        current.Cancel();
        current.Dispose();
    }

    private class CompiledPageComparer : IEqualityComparer<Timed<ChangedPage>>
    {
        public static CompiledPageComparer Instance { get; } = new();

        public bool Equals(Timed<ChangedPage> x, Timed<ChangedPage> y)
            => string.Equals(x.Item.FullPath, y.Item.FullPath, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode([DisallowNull] Timed<ChangedPage> obj)
            => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item.FullPath);
    }

    private TrackedFiles GetFileChanges()
    {
        var dependencies = _compiledPages.SelectMany(t => t.Item.FileDependencies.Select(d => (d, t)))
            .ToLookup(d => d.d, d => d.t)
            .ToDictionary(d => d.Key, d => d.ToList(), StringComparer.OrdinalIgnoreCase);
        var changes = new HashSet<Timed<ChangedPage>>(CompiledPageComparer.Instance);

        var result = _compiledPages;

        foreach (var (file, fullpath) in GetFiles())
        {
            if (dependencies.Remove(fullpath, out var existing))
            {
                foreach (var page in existing)
                {
                    if (file.LastModified > page.LastModified)
                    {
                        changes.Add(new(new(page.Item.AspxFile, page), file.LastModified));
                    }
                }
            }
            else if (file.Name.EndsWith(".aspx"))
            {
                changes.Add(new(new(fullpath), file.LastModified));
            }
        }

        var deletions = dependencies.SelectMany(s => s.Value).Distinct();

        return new TrackedFiles(changes, deletions);
    }

    private readonly record struct TrackedFiles(IEnumerable<Timed<ChangedPage>> Changes, IEnumerable<Timed<ICompiledPage>> Deletions);

    private readonly record struct Timed<T>(T Item, DateTimeOffset LastModified);

    private readonly record struct ChangedPage(string FullPath, Timed<ICompiledPage>? CompiledPage = null);

    IEnumerable<(IFileInfo File, string FullPath)> GetFiles()
    {
        Queue<string> paths = new Queue<string>();
        paths.Enqueue(string.Empty);

        while (paths.Count > 0)
        {
            var subpath = paths.Dequeue();
            var directory = _files.GetDirectoryContents(subpath);

            foreach (var item in directory)
            {
                if (item.IsDirectory)
                {
                    paths.Enqueue(Path.Combine(subpath, item.Name));
                }
                else
                {
                    yield return (item, Path.Combine(subpath, item.Name));
                }
            }
        }
    }

    public void Dispose()
    {
        _aspxFilter.Dispose();
        _siteFilter.Dispose();
    }

    private sealed class Wrapper<T> : IReadOnlyList<T>
    {
        private readonly ImmutableList<Timed<T>> _list;

        public Wrapper(ImmutableList<Timed<T>> list)
        {
            _list = list;
        }

        public T this[int index] => _list[index].Item;

        public int Count => _list.Count;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _list)
            {
                yield return item.Item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
