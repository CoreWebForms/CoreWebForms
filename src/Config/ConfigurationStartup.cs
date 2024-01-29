// MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Configuration;

internal sealed class ConfigurationStartup : IStartupFilter, IDisposable
{
    private readonly IConfigurationRoot _root;
    private readonly IOptions<ConfigurationManagerOptions> _options;
    private IDisposable? _onChange;

    public ConfigurationStartup(IConfiguration root, IOptions<ConfigurationManagerOptions> options)
    {
        _root = (IConfigurationRoot)root;
        _options = options;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => builder =>
        {
            var options = _options.Value;
            var knownKeys = _root.BuildKnownKeys(options);

            // Initialize ConfigurationManager instance
            _root.UpdateConfigurationManager(knownKeys);

            if (options.HandleReload)
            {
                var @lock = new object();
                _onChange = ChangeToken.OnChange(() => _root.GetReloadToken(), () =>
                {
                    // Ensure updates are done serially
                    lock (@lock)
                    {
                        _root.UpdateConfigurationManager(knownKeys);
                    }
                });
            }

            next(builder);
        };

    public void Dispose()
    {
        _onChange?.Dispose();
    }
}
