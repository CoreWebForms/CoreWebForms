// MIT License.

using System.Web.Optimization;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;

[assembly: TagPrefix("System.Web.Optimization", "webopt")]

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
