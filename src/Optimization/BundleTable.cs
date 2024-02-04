// MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebForms;

namespace System.Web.Optimization;

public static class BundleTable
{
    public static BundleCollection Bundles => HttpRuntimeHelper.Services.GetRequiredService<IOptions<BundleReferenceOptions>>().Value.Bundles;
}
