//MIT license

using System.Configuration;

namespace System.Web.Configuration;

/*         <!-- Configuration for siteMap:
           Attributes:
              defaultProvider="string"  Name of provider to use by default
              enabled="[true|false]"    Determine if the feature is enabled.

            <providers>              Providers (class must inherit from SiteMapProvider)

                <add                 Add a provider
                    name="string"    Required string by which the SiteMap class identifies this provider
                    type="string"    Required string which represents the type to instantiate: type must inherit from SiteMapProvider
                    securityTrimmingEnabled="[true|false]"   Determine if security trimming is enabled. (default is false)
                    provider-specific-configuration />

                <remove              Remove a provider
                    name="string" /> Name of provider to remove

                <clear/>             Remove all providers
    -->
    <siteMap defaultProvider="AspNetXmlSiteMapProvider" enabled="true">
        <providers>
            <add name="AspNetXmlSiteMapProvider"
                 description="SiteMap provider which reads in .sitemap XML files."
                 type="System.Web.XmlSiteMapProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                 siteMapFile="web.sitemap" />
        </providers>
    </siteMap>
*/

internal sealed class SiteMapSection : ConfigurationSection
{

    private static readonly ConfigurationPropertyCollection _properties;

    private static readonly ConfigurationProperty _propDefaultProvider =
        new ConfigurationProperty("defaultProvider",
                                    typeof(string),
                                    "AspNetXmlSiteMapProvider",
                                    null,
                                    null,
                                    ConfigurationPropertyOptions.None);

    private static readonly ConfigurationProperty _propEnabled =
        new ConfigurationProperty("enabled",
                                    typeof(bool),
                                    true,
                                    ConfigurationPropertyOptions.None);

    private static readonly ConfigurationProperty _propProviders =
        new ConfigurationProperty("providers",
                                    typeof(ProviderSettingsCollection),
                                    null,
                                    ConfigurationPropertyOptions.None);

    private SiteMapProviderCollection _siteMapProviders;

#pragma warning disable CA1810 // Initialize reference type static fields inline
    static SiteMapSection()
#pragma warning restore CA1810 // Initialize reference type static fields inline
    {
        // Property initialization
        _properties = [_propDefaultProvider, _propEnabled, _propProviders];
    }

    public SiteMapSection()
    {
    }

    protected override ConfigurationPropertyCollection Properties
    {
        get
        {
            return _properties;
        }
    }

    [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetXmlSiteMapProvider")]
    [StringValidator(MinLength = 1)]
    public string DefaultProvider
    {
        get
        {
            return (string)base[_propDefaultProvider];
        }
        set
        {
            base[_propDefaultProvider] = value;
        }
    }

    [ConfigurationProperty("enabled", DefaultValue = true)]
    public bool Enabled
    {
        get
        {
            return (bool)base[_propEnabled];
        }
        set
        {
            base[_propEnabled] = value;
        }
    }

    [ConfigurationProperty("providers")]
    public ProviderSettingsCollection Providers
    {
        get
        {
            if (base[_propProviders] == null || ((ProviderSettingsCollection)base[_propProviders]).Count == 0)
            {
                //TODO: let's handle better way
                var ps = new ProviderSettings() { Name = "AspNetXmlSiteMapProvider", Type = typeof(XmlSiteMapProvider).AssemblyQualifiedName };
                ps.Parameters.Add("siteMapFile", "~/Web.sitemap");
                var col = new ProviderSettingsCollection{ ps };
                base[_propProviders] = col;
            }
            return (ProviderSettingsCollection)base[_propProviders];
        }
    }

    internal SiteMapProviderCollection ProvidersInternal
    {
        get
        {
            if (_siteMapProviders == null)
            {
                lock (this)
                {
                    if (_siteMapProviders == null)
                    {
                        SiteMapProviderCollection siteMapProviders = new SiteMapProviderCollection();
                        ProvidersHelper.InstantiateProviders(Providers, siteMapProviders, typeof(SiteMapProvider));

                        _siteMapProviders = siteMapProviders;
                    }
                }
            }

            return _siteMapProviders;
        }
    }

    internal void ValidateDefaultProvider()
    {
        if (!String.IsNullOrEmpty(DefaultProvider)) // make sure the specified provider has a provider entry in the collection
        {
#if Integrate_Sitemap_WebConfig
            if (Providers[DefaultProvider] == null) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Config_provider_must_exist, DefaultProvider),
                    ElementInformation.Properties[_propDefaultProvider.Name].Source, 
                    ElementInformation.Properties[_propDefaultProvider.Name].LineNumber);
            }
#endif
        }
    }
} // class SiteMapSection
