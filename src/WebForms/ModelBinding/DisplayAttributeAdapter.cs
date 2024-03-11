//MIT License.

using System.ComponentModel.DataAnnotations;

namespace System.Web.ModelBinding
{
    internal sealed class DisplayAttributeAdapter {
        private readonly DisplayAttribute _displayAttribute;

        public DisplayAttributeAdapter(DisplayAttribute displayAttribute) {
            if (displayAttribute == null) {
                throw new ArgumentNullException(nameof(displayAttribute));
            }

            _displayAttribute = displayAttribute;
        }

        public string GetDescription() {
            var localizedString = GetLocalizedString(_displayAttribute.Description);
            if (localizedString == null) {
                localizedString = _displayAttribute.GetDescription();
            }

            return localizedString;
        }

        public string GetShortName() {
            var localizedString = GetLocalizedString(_displayAttribute.ShortName);
            if (localizedString == null) {
                localizedString = _displayAttribute.GetShortName();
            }

            return localizedString;
        }

        public string GetPrompt() {
            var localizedString = GetLocalizedString(_displayAttribute.Prompt);
            if (localizedString == null) {
                localizedString = _displayAttribute.GetPrompt();
            }

            return localizedString;
        }

        public string GetName() {
            var localizedString = GetLocalizedString(_displayAttribute.Name);
            if (localizedString == null) {
                localizedString = _displayAttribute.GetName();
            }

            return localizedString;
        }

        public int? GetOrder() {
            return _displayAttribute.GetOrder();
        }

        private string GetLocalizedString(string name) {
            // if developer already uses existing localization feature,
            // then we don't opt in the new localization feature.
            if (_displayAttribute.ResourceType != null) {
                return null;
            }
            return name;
            //TODO find the CulturalInfo StringLocalizerProviders
        }
    }
}
