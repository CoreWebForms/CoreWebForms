// MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration;

public static class WebConfigExtensions
{
    public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder config, string path = "web.config")
        => config.Add(new WebConfigSource(path));

    public static ISystemWebAdapterBuilder AddConfigurationToConfigurationManager(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddTransient<IStartupFilter, ConfigurationStartup>();
        return builder;
    }
}
