// MIT License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace WebForms.Compiler.Dynamic;

internal sealed class WebFormsCompilationService : BackgroundService
{
    private readonly ILogger<WebFormsCompilationService> _logger;
    private readonly ManualResetEventSlim _event;
    private readonly IWebFormsCompiler _compiler;

    public WebFormsCompilationService(
        IWebFormsCompiler compiler,
        ILogger<WebFormsCompilationService> logger)
    {
        _compiler = compiler;
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

        using (ChangeToken.OnChange(() => files.Watch("**/*.aspx*"), OnFileChange))
        using (ChangeToken.OnChange(() => files.Watch("**/*.Master*"), OnFileChange))
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

            await _compiler.CompilePagesAsync(token).ConfigureAwait(false);

            _event.Reset();
        }
    }
}
