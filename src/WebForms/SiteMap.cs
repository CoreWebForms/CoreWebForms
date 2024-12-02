//MIT license

using System.Configuration.Provider;
using System.Web.Configuration;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Web;

public static class SiteMap
{
    internal const string SectionName = "system.web/siteMap";

    private static SiteMapProviderCollection _providers;
    private static  SiteMapProvider _provider;
    private static readonly object _lockObject = new object();

    public static SiteMapNode CurrentNode
    {
        get
        {
            return Provider.CurrentNode;
        }
    }

   /* public static bool Enabled
    {
        get
        {
            if (!_configEnabledEvaluated)
            {
                SiteMapSection config = RuntimeConfig.GetAppConfig().SiteMap;
                  _enabled = (config != null && config.Enabled);
                  _configEnabledEvaluated = true;
            }

            return _enabled;
        }
    }*/

    public static SiteMapProvider Provider
    {
        get
        {
            Initialize();
            return _provider;
        }
    }

    public static SiteMapProviderCollection Providers
    {
        get
        {
            Initialize();
            return _providers;
        }
    }

    public static SiteMapNode RootNode
    {
        get
        {
            SiteMapProvider rootProvider = Provider.RootProvider;
            SiteMapNode rootNode = rootProvider.RootNode;

            if (rootNode == null)
            {
                String name = ((ProviderBase)rootProvider).Name;
                throw new InvalidOperationException(SR.GetString(SR.SiteMapProvider_Invalid_RootNode, name));
            }

            return rootNode;

        }
    }

    public static event SiteMapResolveEventHandler SiteMapResolve
    {
        add
        {
            Provider.SiteMapResolve += value;
        }
        remove
        {
            Provider.SiteMapResolve -= value;
        }
    }

    private static void Initialize()
    {
        if (_providers != null)
            return;

       // HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

        lock (_lockObject)
        {
            if (_providers != null)
                return;

            var siteMapOption = HttpContext.Current.Request.AsAspNetCore().HttpContext.RequestServices
                .GetRequiredService<IOptions<SiteMapOptions>>();
            SiteMapSection config = new SiteMapSection(siteMapOption);

            if (!config.Enabled)
                throw new InvalidOperationException(SR.GetString(SR.SiteMap_feature_disabled, SiteMap.SectionName));

            // Make sure the default provider exists.
            config.ValidateDefaultProvider();
            _providers = config.ProvidersInternal;
            _provider = _providers[config.DefaultProvider];
            _providers.SetReadOnly();
        }
    }
}

public sealed class SiteMapProviderCollection : ProviderCollection
{

    public override void Add(ProviderBase provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        if (!(provider is SiteMapProvider))
            throw new ArgumentException(SR.GetString(SR.Provider_must_implement_the_interface, provider.GetType().Name, typeof(SiteMapProvider).Name), nameof(provider));

        Add((SiteMapProvider)provider);
    }

    public void Add(SiteMapProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        base.Add(provider);
    }

    public void AddArray(SiteMapProvider[] providerArray)
    {
        if (providerArray == null)
        {
            throw new ArgumentNullException(nameof(providerArray));
        }

        foreach (SiteMapProvider provider in providerArray)
        {
            if (this[provider.Name] != null)
                throw new ArgumentException(SR.GetString("SiteMapProvider_Multiple_Providers_With_Identical_Name", provider.Name));

            Add(provider);
        }
    }

    public new SiteMapProvider this[string name]
    {
        get
        {
            return (SiteMapProvider)base[name];
        }
    }
}

public delegate SiteMapNode SiteMapResolveEventHandler(Object sender, SiteMapResolveEventArgs e);

public class SiteMapResolveEventArgs : EventArgs
{
    private readonly HttpContext _context;
    private readonly SiteMapProvider _provider;

    public SiteMapResolveEventArgs(HttpContext context, SiteMapProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public SiteMapProvider Provider
    {
        get
        {
            return _provider;
        }
    }

    public HttpContext Context
    {
        get
        {
            return _context;
        }
    }
}
