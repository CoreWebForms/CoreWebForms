// MIT License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebForms.Compiler.Dynamic;

namespace WebForms.Compiler;

internal sealed class StaticCompilationService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<StaticCompilationService> _logger;
    private readonly IWebFormsCompiler _compiler;

    public StaticCompilationService(IWebFormsCompiler compiler, IHostApplicationLifetime lifetime, ILogger<StaticCompilationService> logger)
    {
        _compiler = compiler;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Starting static compilation");

        try
        {
            await _compiler.CompilePagesAsync(stoppingToken).ConfigureAwait(false);
            _logger.LogInformation("Completed compilation");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception while compiling occurred");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
