// MIT License.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigurationManagerExtensions
{
    public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder config, string path = "web.config", bool isOptional = true)
    {
        config.Sources.Insert(0, new WebConfigSource(path, isOptional));
        return config;
    }

    public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder config, string path = "app.config", bool isOptional = true)
    {
        config.Sources.Insert(0, new WebConfigSource(path, isOptional));
        return config;
    }

    public static IServiceCollection AddConfigurationManager(this IServiceCollection services, Action<ConfigurationManagerOptions>? configure = null)
    {
        services.AddTransient<IStartupFilter, ConfigurationStartup>();

        if (configure is not null)
        {
            services.AddOptions<ConfigurationManagerOptions>()
                .Configure(configure);
        }

        return services;
    }
}
