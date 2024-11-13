// MIT License.

using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using WebForms.Features;
using WebForms.Internal;

namespace Microsoft.AspNetCore.Builder;

public static class CompiledWebFormsPageExtensions
{
    public static IWebFormsBuilder AddCompiledPages(this IWebFormsBuilder builder)
    {
        builder.Services.AddSingleton<CompiledReflectionWebFormsPage>();
        builder.Services.AddSingleton<IHttpHandlerCollection>(sp => sp.GetRequiredService<CompiledReflectionWebFormsPage>());
        builder.Services.AddSingleton<ITypeResolutionService, DefaultAssemblyLoadContextResolver>();
        builder.Services.AddTransient<IStartupFilter, CompiledStartupFilter>();

        return builder;
    }

    private sealed class CompiledStartupFilter(CompiledReflectionWebFormsPage compiledPages) : IStartupFilter
    {
        Action<IApplicationBuilder> IStartupFilter.Configure(Action<IApplicationBuilder> _next)
            => builder =>
            {
                builder.Use((ctx, next) =>
                {
                    ctx.Features.Set<IWebFormsCompilationFeature>(compiledPages);
                    return next(ctx);
                });

                _next(builder);
            };
    }

    private sealed class DefaultAssemblyLoadContextResolver : ITypeResolutionService
    {
        private static AssemblyLoadContext Current => AssemblyLoadContext.Default;

        public Assembly GetAssembly(AssemblyName name) => GetAssembly(name, throwOnError: false);

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            try
            {
                return Current.LoadFromAssemblyName(name);
            }
            catch when (!throwOnError)
            {
                return null;
            }
        }

        public string GetPathOfAssembly(AssemblyName name) => GetAssembly(name).Location;

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        public Type GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name) => GetType(name, throwOnError: false, ignoreCase: false);

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        public Type GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name, bool throwOnError) => GetType(name, throwOnError, ignoreCase: false);

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        public Type GetType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] string name, bool throwOnError, bool ignoreCase)
        {
            foreach (var assembly in Current.Assemblies)
            {
                if (assembly.GetType(name, throwOnError, ignoreCase) is { } found)
                {
                    return found;
                }
            }

            return null;
        }

        public void ReferenceAssembly(AssemblyName name)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class CompiledReflectionWebFormsPage : IHttpHandlerCollection, IWebFormsCompilationFeature, IDisposable
    {
        private readonly Lazy<Dictionary<string, (IHttpHandlerMetadata Metadata, Type type)>> _metadata;
        private readonly IWebHostEnvironment _env;

        public CompiledReflectionWebFormsPage(IWebHostEnvironment env)
        {
            _env = env;
            _metadata = new(() => ParseHandlers(), isThreadSafe: true);
        }

        IEnumerable<NamedHttpHandlerRoute> IHttpHandlerCollection.NamedRoutes => [];

        IReadOnlyCollection<string> IWebFormsCompilationFeature.Paths => _metadata.Value.Keys;

        IChangeToken IHttpHandlerCollection.GetChangeToken() => NullChangeToken.Singleton;

        IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata() => _metadata.Value.Values.Select(v => v.Metadata);

        private Dictionary<string, (IHttpHandlerMetadata, Type)> ParseHandlers()
        {
            var result = new Dictionary<string, (IHttpHandlerMetadata, Type)>(PathComparer.Instance);

            if (GetWebFormsFile(_env) is { } path)
            {
                var results = JsonSerializer.Deserialize<WebFormsDetails[]>(File.ReadAllText(path));
                var context = new WebFormsAssemblyLoadContext();

                if (results is not null)
                {
                    foreach (var type in results)
                    {
                        if (context.LoadFromAssemblyName(new AssemblyName(type.Assembly)).GetType(type.Type) is { } pageType)
                        {
                            result.Add(type.Path, (HandlerMetadata.Create(type.Path, pageType), pageType));
                        }
                    }
                }
            }

            return result;
        }

        private static string GetWebFormsFile(IWebHostEnvironment env)
        {
            var path = $"{env.ApplicationName}.webforms.json";

            var fullPath = Path.Combine(env.ContentRootPath, path);

            if (!File.Exists(fullPath) && env.IsDevelopment())
            {
                fullPath = Path.Combine(AppContext.BaseDirectory, path);
            }

            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            return null;
        }

        public void Dispose()
        {
            if (AssemblyLoadContext.All.OfType<WebFormsAssemblyLoadContext>().FirstOrDefault() is { } context)
            {
                context.Unload();
            }
        }

        Type IWebFormsCompilationFeature.GetForPath(string virtualPath) => _metadata.Value.TryGetValue(virtualPath, out var result) ? result.type : null;

        bool IWebFormsCompilationFeature.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
        {
            exception = null;
            return false;
        }

        private sealed class WebFormsAssemblyLoadContext : AssemblyLoadContext
        {
            public WebFormsAssemblyLoadContext()
                : base("WebForms Load Context", isCollectible: true)
            {
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "webforms", assemblyName.Name + ".dll");

                if (File.Exists(path))
                {
                    return LoadFromAssemblyPath(path);
                }

                return null;
            }
        }

        private sealed record WebFormsDetails(string Path, string Type, string Assembly);
    }
}
