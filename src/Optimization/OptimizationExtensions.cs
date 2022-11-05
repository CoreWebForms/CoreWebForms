// MIT License.

using System.Web.Optimization;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class OptimizationExtensions
{
    public static ISystemWebAdapterBuilder AddOptimization(this ISystemWebAdapterBuilder builder, Action<BundleReferenceOptions> configure)
    {
        builder.Services.AddOptions<BundleReferenceOptions>()
            .Configure(configure);

        return builder;
    }
}
