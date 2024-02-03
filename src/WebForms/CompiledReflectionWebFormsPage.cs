// MIT License.

using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

public static class CompiledWebFormsPageExtensions
{
    public static ISystemWebAdapterBuilder AddCompiledWebFormsPages(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton<IHttpHandlerCollection, CompiledReflectionWebFormsPage>();
        return builder;
    }

    private sealed class CompiledReflectionWebFormsPage(IWebHostEnvironment env) : IHttpHandlerCollection, IDisposable
    {
        IEnumerable<NamedHttpHandlerRoute> IHttpHandlerCollection.NamedRoutes => [];

        IChangeToken IHttpHandlerCollection.GetChangeToken() => NullChangeToken.Singleton;

        public IEnumerable<IHttpHandlerMetadata> GetHandlerMetadata()
        {
            if (GetWebFormsFile(env) is { Exists: true } file)
            {
                var results = JsonSerializer.Deserialize<WebFormsDetails[]>(file.CreateReadStream());
                var context = GetLoadContext();

                if (results is not null)
                {
                    foreach (var type in results)
                    {
                        if (context.LoadFromAssemblyName(new AssemblyName($"{type.Assembly}")).GetType(type.Type) is { } pageType)
                        {
                            yield return HandlerMetadata.Create(type.Path, pageType);
                        }
                    }
                }
            }
        }

        private static IFileInfo GetWebFormsFile(IWebHostEnvironment env)
        {
            const string DetailsPath = "webforms.pages.json";

            if (env.ContentRootFileProvider.GetFileInfo(DetailsPath) is { Exists: true } file)
            {
                return file;
            }

            if (env.IsDevelopment() && new PhysicalFileProvider(AppContext.BaseDirectory).GetFileInfo(DetailsPath) is { } debug)
            {
                return debug;
            }

            return null;
        }

        private static WebFormsAssemblyLoadContext GetLoadContext()
          => AssemblyLoadContext.All.OfType<WebFormsAssemblyLoadContext>().FirstOrDefault() ?? new WebFormsAssemblyLoadContext();

        public void Dispose()
        {
            if (AssemblyLoadContext.All.OfType<WebFormsAssemblyLoadContext>().FirstOrDefault() is { } context)
            {
                context.Unload();
            }
        }

        private sealed class WebFormsAssemblyLoadContext : AssemblyLoadContext
        {
            public WebFormsAssemblyLoadContext()
                : base("WebForms Load Context", isCollectible: true)
            {
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                if (assemblyName.Name is { } name && name.StartsWith("WebForms.ASP.", StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(AppContext.BaseDirectory, $"{name}.dll");

                    if (File.Exists(path))
                    {
                        return LoadFromAssemblyPath(path);
                    }
                }

                return null;
            }
        }

        private sealed record WebFormsDetails(string Path, string Type, string Assembly);
    }
}
