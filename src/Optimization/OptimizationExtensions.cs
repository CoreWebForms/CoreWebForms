// MIT License.

using System.Web.Optimization;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

public static class OptimizationExtensions
{
    public static ISystemWebAdapterBuilder AddOptimization(this ISystemWebAdapterBuilder builder, Action<BundleReferenceOptions> configure)
    {
        builder.Services.AddOptions<BundleReferenceOptions>()
            .Configure(configure);

        return builder;
    }
}
