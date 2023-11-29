// MIT License.

namespace System.Web.Optimization;

public static class BundleTable
{
    private static BundleCollection _instance = new BundleCollection();
    private static bool _enableOptimizations = true;
    private static bool _enableOptimizationsSet;

    // private static VirtualPathProvider _vpp = null;

    private static bool _readBundleManifest;

    //
    // Summary:
    //     Gets the default bundle collection.
    //
    // Returns:
    //     The default bundle collection.
    public static BundleCollection Bundles
    {
        get
        {
            EnsureBundleSetup();
            return _instance;
        }
    }

    //
    // Summary:
    //     Gets or sets whether bundling and minification of bundle references is enabled.
    //
    // Returns:
    //     true if bundling and minification of bundle references is enabled; otherwise,
    //     false.
    public static bool EnableOptimizations
    {
        get
        {
            if (!_enableOptimizationsSet && HttpContext.Current != null)
            {
                return !HttpContext.Current.IsDebuggingEnabled;
            }

            return _enableOptimizations;
        }
        set
        {
            _enableOptimizations = value;
            _enableOptimizationsSet = true;
        }
    }

    private static void EnsureBundleSetup()
    {
        if (!_readBundleManifest)
        {
            _readBundleManifest = true;
            BundleManifest.ReadBundleManifest()?.Register(Bundles);
        }
    }
}
