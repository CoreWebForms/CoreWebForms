// MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class HostBuilderExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Adds a <c>web.config</c> to the host and optionally loads values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <param name="host">The <see cref="IHostApplicationBuilder"/> instance.</param>
    /// <param name="path">Path to the configuration file.</param>
    /// <param name="isOptional">Sets if the config source is optional or not.</param>
    /// <param name="eagerUpdateAppSettings">Flag to initiate loading values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/> early for scenarios that these values before the host itself is build.</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddWebConfig(this IHostApplicationBuilder host, string path = "web.config", bool isOptional = true, bool eagerUpdateAppSettings = false)
    {
        host.Configuration.Sources.Insert(0, new WebConfigSource(path, isOptional));

        // Update app settings
        if (eagerUpdateAppSettings)
        {
            host.Configuration.Build().UpdateConfigurationManager();
        }

        // Register as singleton so we can use the service collection to dispose it.
        host.Services.AddSingleton<IStartupFilter, ConfigurationStartup>();

        return host;
    }
#endif

    /// <summary>
    /// Adds a <c>web.config</c> to the host and optionally loads values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> instance.</param>
    /// <param name="path">Path to the configuration file.</param>
    /// <param name="isOptional">Sets if the config source is optional or not.</param>
    /// <param name="eagerUpdateAppSettings">Flag to initiate loading values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/> early for scenarios that these values before the host itself is build.</param>
    /// <returns></returns>
    public static IHostBuilder AddWebConfig(this IHostBuilder host, string path = "web.config", bool isOptional = true, bool eagerUpdateAppSettings = false) => host
        .ConfigureAppConfiguration(config =>
        {
            config.Sources.Insert(0, new WebConfigSource(path, isOptional));

            // Update app settings
            if (eagerUpdateAppSettings)
            {
                config.Build().UpdateConfigurationManager();
            }
        })
        .ConfigureServices(services =>
        {
            // Register as singleton so we can use the service collection to dispose it.
            services.AddSingleton<IStartupFilter, ConfigurationStartup>();
        });
}
