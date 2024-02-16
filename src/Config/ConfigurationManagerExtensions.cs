// MIT License.

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ConfigurationManagerExtensions
{
    private static readonly object _appSettingsUpdateLock = new();

    /// <summary>
    /// Adds a <c>web.config</c> to the host and optionally loads values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <param name="host">The <see cref="IHostApplicationBuilder"/> instance.</param>
    /// <param name="path">Path to the configuration file.</param>
    /// <param name="isOptional">Sets if the config source is optional or not.</param>
    /// <param name="eagerUpdateAppSettings">Flag to initiate loading values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/> early for scenarios that these values before the host itself is build.</param>
    /// <returns></returns>
    public static IHostApplicationBuilder UseWebConfig(this IHostApplicationBuilder host, string path = "web.config", bool isOptional = true)
    {
        host.Configuration.Sources.Insert(0, new WebConfigSource(path, isOptional));

        host.Configuration.Build().UpdateConfigurationManager();

        host.Services.AddHostedService<SystemConfigurationBackgroundService>();

        return host;
    }

    /// <summary>
    /// Adds a <c>web.config</c> to the host and optionally loads values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> instance.</param>
    /// <param name="path">Path to the configuration file.</param>
    /// <param name="isOptional">Sets if the config source is optional or not.</param>
    /// <param name="eagerUpdateAppSettings">Flag to initiate loading values into <see cref="System.Configuration.ConfigurationManager.AppSettings"/> early for scenarios that these values before the host itself is build.</param>
    /// <returns></returns>
    public static IHostBuilder UseWebConfig(this IHostBuilder host, string path = "web.config", bool isOptional = true) => host
        .ConfigureAppConfiguration(config =>
        {
            config.Sources.Insert(0, new WebConfigSource(path, isOptional));

            // Update app settings
            config.Build().UpdateConfigurationManager();
        })
        .ConfigureServices(services =>
        {
            // Register as singleton so we can use the service collection to dispose it.
            services.AddHostedService<SystemConfigurationBackgroundService>();
        });

    internal static void UpdateConfigurationManager(this IConfiguration configuration)
    {
        // Ensure updates are done serially
        lock (_appSettingsUpdateLock)
        {
            // Update app settings
            UpdateAppSettings(System.Configuration.ConfigurationManager.AppSettings, configuration);
            UpdateConnectionStrings(System.Configuration.ConfigurationManager.ConnectionStrings, configuration.GetSection("ConnectionStrings"), configuration.GetSection("ConnectionStringProviders"));
        }
    }

    private static void UpdateConnectionStrings(ConnectionStringSettingsCollection connectionStrings, IConfiguration configuration, IConfiguration providers)
    {
        if (connectionStrings.IsReadOnly())
        {
            var collectionField = typeof(ConfigurationElementCollection).GetField("_readOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            var field = typeof(ConfigurationElement).GetField("_readOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is null || collectionField is null)
            {
                throw new NotSupportedException("Not able to set connection strings");
            }

            collectionField.SetValue(connectionStrings, false);

            foreach (ConnectionStringSettings entry in connectionStrings)
            {
                field.SetValue(entry, false);
            }
        }

        foreach (var (key, value) in configuration.AsEnumerable())
        {
            if (connectionStrings[key] is { } existing)
            {
                existing.ConnectionString = value;
                existing.ProviderName = providers[key];
            }
            else
            {
                connectionStrings.Add(new(key, configuration[key], providers[key]));
            }
        }
    }

    private static void UpdateAppSettings(NameValueCollection appSettings, IConfiguration configuration)
    {
        foreach (var (key, value) in configuration.AsEnumerable())
        {
            appSettings[key] = value;
        }
    }
}
