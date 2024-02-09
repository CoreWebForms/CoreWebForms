// MIT License.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace WebForms.Compiler.Dynamic;

internal sealed class WebFormsCompilationService : BackgroundService
{
    private readonly ILogger<WebFormsCompilationService> _logger;
    private readonly ManualResetEventSlim _event;
    private readonly IWebFormsCompiler _compiler;
    private readonly IOptions<PageCompilationOptions> _options;

    public WebFormsCompilationService(
        IWebFormsCompiler compiler,
        IOptions<PageCompilationOptions> options,
        ILogger<WebFormsCompilationService> logger)
    {
        _compiler = compiler;
        _options = options;
        _logger = logger;
        _event = new ManualResetEventSlim(true);
    }

    public override void Dispose()
    {
        base.Dispose();
        _event.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var files = _compiler.Files;

        using var matcher = new CompositeWatcher(files, OnFileChange);

        foreach (var extension in _options.Value.Parsers.Keys)
        {
            matcher.AddExtension(extension);
        }

        await ProcessChanges(stoppingToken).ConfigureAwait(false);
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

            await _compiler.CompilePagesAsync(token).ConfigureAwait(false);

            _event.Reset();
        }
    }

    private sealed class CompositeWatcher(IFileProvider files, Action action) : IDisposable
    {
        private readonly List<IDisposable> _disposables = [];

        public void AddExtension(string extension)
        {
            var disposable = ChangeToken.OnChange(() => files.Watch($"**/*{extension}*"), action);

            _disposables.Add(disposable);
        }

        public void Dispose()
        {
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
        }
    }
}
