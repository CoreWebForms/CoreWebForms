// MIT License.

using System.Reflection;
using System.Web;

namespace WebForms;

internal static class HttpRuntimeHelper
{
    private static readonly Lazy<Func<IServiceProvider>> ServiceProviderFactory = new(() =>
    {
        var type = typeof(HttpRuntime).Assembly.GetType("System.Web.Hosting.HostingEnvironmentAccessor");
        var currentProperty = type.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
        var spProperty = type.GetProperty("Services", BindingFlags.Instance | BindingFlags.Public);

        var current = currentProperty.GetValue(null);

        return () => (IServiceProvider)spProperty.GetValue(current);
    }, isThreadSafe: true);

    /// <summary>
    /// Only needed until HttpRuntime.WebObjectActivator is available
    /// </summary>
    public static IServiceProvider Services => ServiceProviderFactory.Value();
}
