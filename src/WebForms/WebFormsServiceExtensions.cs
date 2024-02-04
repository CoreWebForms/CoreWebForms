// MIT License.

using Microsoft.AspNetCore.Hosting;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsServiceExtensions
{
    public static IWebFormsBuilder AddWebForms(this ISystemWebAdapterBuilder builder)
    {
        builder.AddHttpHandlers();
        builder.AddRouting();

        builder.Services.AddHostedService<VirtualFileEnvService>();

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

    /// <summary>
    /// This is just used until we can get the <see cref="IServiceProvider"/> off of <see cref="HttpRuntime"/>.
    /// </summary>
    /// <param name="env"></param>
    private sealed class VirtualFileEnvService(IWebHostEnvironment env) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tcs = new TaskCompletionSource();
            using var registration = stoppingToken.Register(tcs.SetResult);

            VirtualPath.Files = env.ContentRootFileProvider;

            await tcs.Task.ConfigureAwait(false);

            VirtualPath.Files = null;
        }
    }
}
