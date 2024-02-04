// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsServiceExtensions
{
    public static IWebFormsBuilder AddWebForms(this ISystemWebAdapterBuilder builder)
    {
        builder.AddHttpHandlers();
        builder.AddRouting();

        return new Builder(builder);
    }

    public static IWebFormsBuilder AddWebForms(this IServiceCollection builder)
        => builder
            .AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession()
            .AddWebForms();

    private record Builder(ISystemWebAdapterBuilder SystemWebAdapterBuilder) : IWebFormsBuilder
    {
        public IServiceCollection Services => SystemWebAdapterBuilder.Services;
    }
}
