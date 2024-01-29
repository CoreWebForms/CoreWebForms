// MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Configuration;

internal sealed class SystemConfigurationBackgroundService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IOptions<ConfigurationManagerOptions> _options;

    public SystemConfigurationBackgroundService(IConfiguration configuration, IOptions<ConfigurationManagerOptions> options)
    {
        _configuration = configuration;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize ConfigurationManager instance
        _configuration.UpdateConfigurationManager();

        if (_options.Value.HandleReload)
        {
            var tcs = new TaskCompletionSource();
            using var registration = stoppingToken.Register(tcs.SetResult);
            using var onChange = ChangeToken.OnChange(
                () => _configuration.GetReloadToken(),
                () => _configuration.UpdateConfigurationManager());

            await tcs.Task.ConfigureAwait(false);
        }
    }
}
