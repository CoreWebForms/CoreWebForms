// MIT License.

using System.Web.Optimization;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class OptimizationExtensions
{
    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder, Action<BundleReferenceOptions> configure)
    {
        builder.Services.AddOptions<BundleReferenceOptions>()
            .Configure(configure);

        return builder;
    }
}
