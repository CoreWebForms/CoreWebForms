// MIT License.

using System.Web.Optimization;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection;

public static class OptimizationExtensions
{
    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder)
    {
        builder.Services.AddTransient<IBundleResolver>(_ => BundleResolver.Current);
        builder.Services.AddSingleton<BundleTableEndpointDataSource>();

        return builder;
    }

    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder, Action<BundleCollection> configure)
    {
        builder.Services.AddTransient<IStartupFilter>(_ => new OptimizationStartup(configure));

        return builder.AddOptimization();
    }

    public static void MapBundleTable(this IEndpointRouteBuilder endpoints)
    {
        endpoints.DataSources.Add(endpoints.ServiceProvider.GetRequiredService<BundleTableEndpointDataSource>());
    }

    private sealed class BundleTableEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => CreateEndpoints().ToList();

        private static IEnumerable<Endpoint> CreateEndpoints()
        {
            var bundles = BundleTable.Bundles;

            foreach (var bundle in bundles)
            {
                var builder = new RouteEndpointBuilder(ctx =>
                {
                    bundle.ProcessRequest(new BundleContext(ctx, bundles, bundle.Path));
                    return Task.CompletedTask;
                }, RoutePatternFactory.Parse(bundle.Path), 0);

                builder.Metadata.Add(new BufferResponseStreamAttribute());

                yield return builder.Build();
            }
        }

        public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    }

    private sealed class OptimizationStartup(Action<BundleCollection> startup) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                startup(BundleTable.Bundles);

                next(builder);
            };
    }
}
