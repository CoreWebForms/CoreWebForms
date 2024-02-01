// MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebForms.Compiler.Dynamic;

internal sealed class PersistedCompilationService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<PersistedCompilationService> _logger;
    private readonly IWebFormsCompiler _compiler;

    public PersistedCompilationService(IWebFormsCompiler compiler, IHostApplicationLifetime lifetime, ILogger<PersistedCompilationService> logger)
    {
        _compiler = compiler;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Debugger.Launch();

        try
        {
            await _compiler.CompilePagesAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception while compiling occured");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
