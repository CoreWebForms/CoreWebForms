// MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration;

public static class WebConfigExtensions
{
    public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder config, string path = "web.config", bool isOptional = true)
        => config.Add(new WebConfigSource(path, isOptional));

    public static ISystemWebAdapterBuilder AddConfigurationToConfigurationManager(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddTransient<IStartupFilter, ConfigurationStartup>();
        return builder;
    }
}
