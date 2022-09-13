// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class CompilationCollection : ICompiledPagesCollection
{
    private readonly ILogger<CompilationCollection> _logger;
    private readonly IQueue _queue;
    private readonly IFileProvider _files;
    private readonly IPageCompiler _compiler;

    private Dictionary<string, TypeInfo> _types;
    private List<Type> _endpointTypes;

    private CancellationTokenSource _cts;
    private CancellationChangeToken _token;

    public CompilationCollection(IFileProvider files, IPageCompiler compiler, IQueue queue, ILoggerFactory logger)
    {
        _logger = logger.CreateLogger<CompilationCollection>();
        _queue = queue;
        _files = files;
        _compiler = compiler;
        _types = new();
        _endpointTypes = new();
        _cts = new CancellationTokenSource();
        _token = new CancellationChangeToken(_cts.Token);

        ChangeToken.OnChange(() => _files.Watch("**/*.aspx"), OnFileChange);

        OnFileChange();
    }

    IReadOnlyList<Type> ICompiledPagesCollection.PageTypes => _endpointTypes;

    IChangeToken ICompiledPagesCollection.ChangeToken => _token;

    private void OnFileChange()
        => _queue.Add(UpdateTypesAsync);

    private async Task UpdateTypesAsync(CancellationToken token)
    {
        var tracker = new UpdateTracker(_types);
        DirectorySearch("/", tracker);

        var updatedCollection = new Dictionary<string, TypeInfo>();
        var items = new List<Type>();

        foreach (var unchanged in tracker.Unchanged)
        {
            updatedCollection.Add(unchanged, _types[unchanged]);
        }

        foreach (var removed in tracker.Removed)
        {
            if (_types.TryGetValue(removed, out var existing))
            {
                _compiler.RemovePage(existing.type);
            }
        }

        foreach (var added in tracker.Added)
        {
            var type = await _compiler.CompilePageAsync(added, token).ConfigureAwait(false);

            if (type is not null)
            {
                updatedCollection.Add(added.Id, new(added.File.LastModified, type));
            }
        }

        foreach (var changed in tracker.Changed)
        {
            _types.Remove(changed.Id, out var existing);
            _compiler.RemovePage(existing.type);

            var type = await _compiler.CompilePageAsync(changed, token).ConfigureAwait(false);

            if (type is not null)
            {
                updatedCollection.Add(changed.Id, new(changed.File.LastModified, type));
            }
        }

        _types = updatedCollection;
        _endpointTypes = updatedCollection.Select(u => u.Value.type).ToList();

        var current = _cts;

        _cts = new();
        _token = new CancellationChangeToken(_cts.Token);

        current.Cancel();
        current.Dispose();
    }

    private void DirectorySearch(string directory, UpdateTracker tracker)
    {
        foreach (var contents in _files.GetDirectoryContents(directory))
        {
            if (contents.IsDirectory)
            {
                DirectorySearch(Path.Combine(directory, contents.Name), tracker);
            }
            else
            {
                tracker.Visit(new(directory, contents));
            }
        }
    }

    private sealed class UpdateTracker
    {
        private readonly Dictionary<string, TypeInfo> _items;
        private readonly HashSet<string> _existing;
        private List<PageFile>? _added;
        private List<PageFile>? _changed;
        private List<string>? _unchanged;

        public UpdateTracker(Dictionary<string, TypeInfo> items)
        {
            _items = items;
            _existing = new HashSet<string>(items.Keys);
        }

        public void Visit(PageFile file)
        {
            if (!file.File.Name.EndsWith(".aspx"))
            {
                return;
            }

            var id = file.Id;
            _existing.Remove(id);

            if (_items.TryGetValue(id, out var existing))
            {
                if (file.File.LastModified > existing.LastModified)
                {
                    (_changed ??= new()).Add(file);
                }
                else
                {
                    (_unchanged ??= new()).Add(id);
                }
            }
            else
            {
                (_added ??= new()).Add(file);
            }
        }

        public IEnumerable<PageFile> Changed => _changed ?? Enumerable.Empty<PageFile>();

        public IEnumerable<PageFile> Added => _added ?? Enumerable.Empty<PageFile>();

        public IEnumerable<string> Unchanged => _unchanged ?? Enumerable.Empty<string>();

        public IEnumerable<string> Removed => _existing;
    }

    private readonly record struct TypeInfo(DateTimeOffset LastModified, Type type);
}
