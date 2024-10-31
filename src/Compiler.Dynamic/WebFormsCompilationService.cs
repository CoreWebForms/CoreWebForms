// MIT License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class WebFormsCompilationService(
    DynamicSystemWebCompilation compiler,
    ILogger<WebFormsCompilationService> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ProcessChanges(stoppingToken);
        return Task.CompletedTask;
    }

    private void ProcessChanges(CancellationToken token)
    {
        try
        {
            compiler.CompilePages(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error compiling assets");
        }
    }
}
