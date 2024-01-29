// MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Configuration;

internal static class AppSettingExtensions
{
    public static KnownKeys BuildKnownKeys(this IConfigurationRoot root, ConfigurationManagerOptions options)
    {
        var keys = root.Providers
            .OfType<WebConfigConfigurationProvider>()
            .Select(provider => provider.Keys);
        var appSettings = new HashSet<string>(options.KnownAppSettings);
        var strings = new HashSet<string>(options.KnownConnectionStrings);

        foreach (var key in keys)
        {
            appSettings.UnionWith(key.AppSettings);
            strings.UnionWith(key.ConnectionStrings);
        }

        return new(appSettings, strings);
    }

    public static void UpdateConfigurationManager(this IConfigurationRoot root)
    {
        var keys = root.BuildKnownKeys(new ConfigurationManagerOptions { HandleReload = false });
        root.UpdateConfigurationManager(keys);
    }

    public static void UpdateConfigurationManager(this IConfiguration configuration, KnownKeys keys)
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
