// MIT License.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace WebForms.Compiler.Dynamic;

internal sealed class WebFormsCompilationService : BackgroundService
{
    private readonly ILogger<WebFormsCompilationService> _logger;
    private readonly RouteCollection _routes;
    private readonly ManualResetEventSlim _event;
    private readonly IPageCompiler _compiler;
    private readonly IFileProvider _files;

    private ImmutableList<Timed<ICompiledPage>> _compiledPages;

    public WebFormsCompilationService(
        IPageCompiler compiler,
        IOptions<HttpHandlerOptions> handlerOptions,
        IOptions<PageCompilationOptions> options,
        ILogger<WebFormsCompilationService> logger)
    {
        _compiledPages = ImmutableList<Timed<ICompiledPage>>.Empty;
        _files = options.Value.Files!;
        _compiler = compiler;
        _logger = logger;
        _routes = handlerOptions.Value.Routes;
        _event = new ManualResetEventSlim(true);
    }

    public override void Dispose()
    {
        base.Dispose();
        _event.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_files is null)
        {
            return;
        }

        using (ChangeToken.OnChange(() => _files.Watch("**/*.aspx*"), OnFileChange))
        using (ChangeToken.OnChange(() => _files.Watch("**/*.Master*"), OnFileChange))
        {
            await ProcessChanges(stoppingToken).ConfigureAwait(false);
        }
    }

    private void OnFileChange()
    {
        _event.Set();
        _logger.LogInformation("File change detected");
    }

    private async Task ProcessChanges(CancellationToken token)
    {
        while (true)
        {
            await _event.WaitHandle.WaitAsync(token).ConfigureAwait(false);

            try
            {
                _logger.LogInformation("Running compilation task");
                await UpdateTypesAsync(token).ConfigureAwait(false);
                _logger.LogInformation("Finished compilation task");
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogError(e, "Unexpected error compiling");
            }

            _event.Reset();
        }
    }

    private async Task UpdateTypesAsync(CancellationToken token)
    {
        var changedFiles = GetFileChanges();
        var finalPages = _compiledPages.ToBuilder();

        using (_routes.GetWriteLock())
        {
            foreach (var file in changedFiles.Deletions)
            {
                _logger.LogTrace("Removing page {Path}", file.Item.Path);
                _routes.Remove(file.Item.Path);
                finalPages.Remove(file);
                file.Item.Dispose();
            }

            foreach (var file in changedFiles.Changes)
            {
                if (file.Item.CompiledPage is { } existing)
                {
                    _logger.LogTrace("Replacing '{Path}'", existing.Item.AspxFile);
                    finalPages.Remove(existing);
                    existing.Item.Dispose();
                }
                else
                {
                    _logger.LogTrace("Creating page for '{Path}'", file.Item.FullPath);
                }

                var aspx = file.Item.CompiledPage is { } compiled ? compiled.Item.AspxFile : file.Item.FullPath;

                var compilation = await _compiler.CompilePageAsync(aspx, token).ConfigureAwait(false);

                if (compilation.Type is { } type)
                {
                    _logger.LogTrace("Adding page {Path}", compilation.Path);
                    _routes.Replace(compilation.Path, type);
                }
                else
                {
                    _logger.LogWarning("No type found for {Path}", compilation.Path);
                    _routes.Replace(compilation.Path, new ErrorHandler(compilation.Exception!));
                }

                finalPages.Add(new(compilation, file.LastModified));
            }

            Interlocked.Exchange(ref _compiledPages, finalPages.ToImmutable());
        }
    }

    private sealed class CompiledPageComparer : IEqualityComparer<Timed<ChangedPage>>
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

        foreach (var (file, fullpath) in _files.GetFiles())
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
            //TODO https://github.com/twsouthwick/systemweb-adapters-ui/issues/27 // next PR
            //|| file.Name.EndsWith(".ascx"))
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
}
