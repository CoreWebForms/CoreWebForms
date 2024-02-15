// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using WebForms.Compiler.Dynamic;

namespace WebForms.Compiler;

internal sealed class CompilationHost
{
    public static Task RunAsync(DirectoryInfo path, DirectoryInfo targetDir, FileInfo[] references, bool isDebug)
        => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, _) =>
            {
                ctx.HostingEnvironment.ContentRootPath = path.FullName;
                ctx.HostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(path.FullName);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.ColorBehavior = LoggerColorBehavior.Default;
                });

                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("WebForms", LogLevel.Trace);
            })

            // This is needed so we can enable HttpRuntime APIs
            .ConfigureWebHost(web =>
            {
                web.Configure(app => { });
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IServer, WebFormsCompilationServer>();
                services.AddOptions<StaticCompilationOptions>()
                    .Configure(options =>
                    {
                        options.TargetDirectory = targetDir.FullName;
                        options.InputDirectory = path.FullName;
                    })
                    .ValidateDataAnnotations();

                services
                    .AddWebForms()
                    .AddPersistentWebFormsCompilation(references.Select(r => r.FullName));

                services.AddHostedService<StaticCompilationService>();

                services.AddOptions<PageCompilationOptions>()
                    .Configure(options => options.IsDebug = isDebug);

                services.AddOptions<SystemWebAdaptersOptions>().Configure(options =>
                {
                    options.AppDomainAppPath = path.FullName;
                });
            })
            .RunConsoleAsync();
}

sealed class WebFormsCompilationServer : IServer
{
    public IFeatureCollection Features { get; } = new FeatureCollection();

    public void Dispose()
    {
    }

    public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
