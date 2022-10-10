// MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Configuration;

internal class ConfigurationStartup : IStartupFilter
{
    private readonly IConfigurationRoot _root;

    public ConfigurationStartup(IConfiguration root)
    {
        _root = (IConfigurationRoot)root;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => builder =>
        {
            var knownKeys = BuildKnownKeys(_root);

            // Initialize ConfigurationManager instance
            UpdateConfigurationManager(_root, knownKeys);

            // Register for any changes
            ChangeToken.OnChange(() => _root.GetReloadToken(), () => UpdateConfigurationManager(_root, knownKeys));

            next(builder);
        };

    private static KnownKeys BuildKnownKeys(IConfigurationRoot root)
    {
        var keys = root.Providers
            .OfType<WebConfigConfigurationProvider>()
            .Select(provider => provider.Keys);
        var appSettings = new HashSet<string>();
        var strings = new HashSet<string>();

        foreach (var key in keys)
        {
            appSettings.UnionWith(key.AppSettings);
            strings.UnionWith(key.ConnectionStrings);
        }

        return new(appSettings, strings);
    }

    private static void UpdateConfigurationManager(IConfiguration configuration, KnownKeys keys)
    {
        // Update app settings
        UpdateAppSettings(System.Configuration.ConfigurationManager.AppSettings, configuration, keys.AppSettings);
        UpdateConnectionStrings(System.Configuration.ConfigurationManager.ConnectionStrings, configuration.GetSection("ConnectionStrings"), configuration.GetSection("ConnectionStringProviders"), keys.ConnectionStrings);
    }

    private static void UpdateConnectionStrings(ConnectionStringSettingsCollection connectionStrings, IConfiguration configuration, IConfiguration providers, IEnumerable<string> keys)
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

        foreach (var key in keys)
        {
            if (connectionStrings[key] is { } existing)
            {
                existing.ConnectionString = configuration[key];
                existing.ProviderName = providers[key];
            }
            else
            {
                connectionStrings.Add(new(key, configuration[key], providers[key]));
            }
        }
    }

    private static void UpdateAppSettings(NameValueCollection appSettings, IConfiguration configuration, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            appSettings[key] = configuration[key];
        }
    }
}
