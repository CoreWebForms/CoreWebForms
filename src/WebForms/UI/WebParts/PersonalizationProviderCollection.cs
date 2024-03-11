// MIT License.

using System.Configuration.Provider;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class PersonalizationProviderCollection : ProviderCollection {

        new public PersonalizationProvider this[string name] {
            get {
                return (PersonalizationProvider)base[name];
            }
        }

        public override void Add(ProviderBase provider) {
            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!(provider is PersonalizationProvider)) {
                throw new ArgumentException(
                    SR.GetString(SR.Provider_must_implement_the_interface,
                        provider.GetType().FullName, "PersonalizationProvider"));
            }

            base.Add(provider);
        }

        public void CopyTo(PersonalizationProvider[] array, int index) {
            base.CopyTo(array, index);
        }
    }
}
