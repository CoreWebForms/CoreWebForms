// MIT License.

using Microsoft.Extensions.DependencyInjection;
using WebForms;

namespace System.Web.Optimization;

public static class BundleTable
{
    public static BundleCollection Bundles => HttpRuntimeHelper.Services.GetRequiredService<BundleCollection>();
}
