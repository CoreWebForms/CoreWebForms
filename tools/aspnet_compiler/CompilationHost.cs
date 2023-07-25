// MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WebForms.Compiler.Dynamic;

internal sealed class CompilationHost
{
    public static Task RunAsync(DirectoryInfo path, DirectoryInfo targetDir)
        => Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
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

                services.AddWebFormsCompilation(options =>
                {
                    options.Files = new PhysicalFileProvider(path.FullName);
                });
            })
            .RunConsoleAsync();
}
