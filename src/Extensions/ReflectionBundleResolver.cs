// MIT License.

using System.Reflection;
using Microsoft.Extensions.Logging;

namespace WebForms.Extensions;

internal sealed class ReflectionBundleResolver : IBundleResolver
{
    private delegate object BundleResolverCurrentDelegate();
    private delegate bool IsBundleVirtualPathDelegate(string virtualPath);
    private delegate IEnumerable<string> GetBundleContentsDelegate(string virtualPath);
    private delegate string GetBundleUrlDelegate(string virtualPath);

    private readonly Dispatcher _dispatcher;

    public ReflectionBundleResolver(ILogger<ReflectionBundleResolver> logger)
    {
        try
        {
            var assembly = Assembly.Load("System.Web.Optimization");

            if (assembly.GetType("System.Web.Optimization.BundleResolver") is { } type)
            {
                PropertyInfo bundleResolverCurrentProperty = type.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
                if (bundleResolverCurrentProperty is { GetMethod: { } get })
                {
                    _dispatcher = new Dispatcher(get.CreateDelegate<BundleResolverCurrentDelegate>(null));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("Could not fin System.Web.Optimization for bundle usage");
        }
    }

    public IEnumerable<string> GetBundleContents(string virtualPath)
        => _dispatcher?.GetBundleContents(virtualPath) ?? [];

    public string GetBundleUrl(string virtualPath)
        => _dispatcher?.GetBundleUrl(virtualPath);

    public bool IsBundleVirtualPath(string virtualPath)
        => _dispatcher?.IsBundleVirtualPath(virtualPath) ?? false;

    private sealed class Dispatcher
    {
        public Dispatcher(BundleResolverCurrentDelegate resolver)
        {
            var current = resolver();
            var type = current.GetType();

            IsBundleVirtualPath = type.GetMethod("IsBundleVirtualPath", [typeof(string)]).CreateDelegate<IsBundleVirtualPathDelegate>(current);
            GetBundleContents = type.GetMethod("GetBundleContents", [typeof(string)]).CreateDelegate<GetBundleContentsDelegate>(current);
            GetBundleUrl = type.GetMethod("GetBundleUrl", [typeof(string)]).CreateDelegate<GetBundleUrlDelegate>(current);
        }

        public IsBundleVirtualPathDelegate IsBundleVirtualPath { get; }

        public GetBundleContentsDelegate GetBundleContents { get; }

        public GetBundleUrlDelegate GetBundleUrl { get; }
    }
}
