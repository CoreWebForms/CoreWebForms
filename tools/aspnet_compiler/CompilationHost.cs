// MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebForms.Compiler.Dynamic;

internal sealed class CompilationHost
{
    public static Task RunAsync(DirectoryInfo path, DirectoryInfo targetDir)
        => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, _) =>
            {
                ctx.HostingEnvironment.ContentRootPath = path.FullName;
                ctx.HostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(path.FullName);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            })

            // This is needed so we can enable HttpRuntime APIs
            .ConfigureWebHost(web =>
            {
                web.Configure(app => { });
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IServer, WebFormsCompilationServer>();
                services.AddHostedService<PersistedCompilationService>();
                services.AddOptions<PersistentCompilationOptions>()
                    .Configure(options =>
                    {
                        options.TargetDirectory = targetDir.FullName;
                        options.InputDirectory = path.FullName;

                        foreach (var r in Basic.Reference.Assemblies.Net60.References.All)
                        {
                            options.MetadataReferences.Add(r);
                        }
                    })
                    .ValidateDataAnnotations();

                services
                    .AddSystemWebAdapters()
                    .AddPersistentWebFormsCompilation(options =>
                    {
                        options.Files = new PhysicalFileProvider(path.FullName);
                    });

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
