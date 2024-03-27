//MIT License

using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Reflection;

namespace System.Web.Configuration
{

    internal static class ProvidersHelper {

        internal static IServiceProvider WebObjectActivator { get; set; }

        internal static ProviderBase InstantiateProvider(ProviderSettings providerSettings, Type providerType)
        {
            ProviderBase provider = null;
            
            try {
                string pnType = (providerSettings.Type == null) ? null : providerSettings.Type.Trim();
                if (string.IsNullOrEmpty(pnType))
                    throw new ArgumentException(SR.GetString(SR.Provider_no_type_name));
                Type t = ConfigUtil.GetType(pnType, "type", providerSettings, true, true);

                if (!providerType.IsAssignableFrom(t))
                    throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, providerType.ToString()));
                provider = (ProviderBase)CreatePublicInstanceByWebObjectActivator(t, null);

                // Because providers modify the parameters collection (i.e. delete stuff), pass in a clone of the collection
                NameValueCollection pars = providerSettings.Parameters;
                NameValueCollection cloneParams = new NameValueCollection(pars.Count, StringComparer.Ordinal);
                foreach (string key in pars)
                    cloneParams[key] = pars[key];
                provider.Initialize(providerSettings.Name, cloneParams);
            } catch (Exception e) {
                if (e is ConfigurationException)
                    throw;
                throw new ConfigurationErrorsException(e.Message, e, providerSettings.ElementInformation.Properties["type"].Source, providerSettings.ElementInformation.Properties["type"].LineNumber);
            }

            return provider;
        }

        private static object CreatePublicInstanceByWebObjectActivator(Type type, object[] args)
        {
            var activator = WebObjectActivator;

            if (activator != null)
            {
                return activator.GetService(type);
            }
            return Activator.CreateInstance(
                type,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                args,
                null);
        }

        // [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
        internal static void InstantiateProviders(ProviderSettingsCollection configProviders, ProviderCollection providers, Type providerType)
        {
            foreach (ProviderSettings ps in configProviders) {
                providers.Add(InstantiateProvider(ps, providerType));
            }
        }

    }
}
