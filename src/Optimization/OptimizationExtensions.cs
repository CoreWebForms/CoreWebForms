// MIT License.

using System.Web.Optimization;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

[assembly: TagPrefix("Microsoft.AspNet.Web.Optimization.WebForms", "webopt")]
[assembly: TagPrefix("System.Web.Optimization", "webopt")]

namespace Microsoft.Extensions.DependencyInjection;

public static class OptimizationExtensions
{
    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder, Action<BundleCollection> configure)
    {
        builder.Services.AddTransient<IStartupFilter>(_ => new OptimizationStartup(configure));

        return builder;
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
