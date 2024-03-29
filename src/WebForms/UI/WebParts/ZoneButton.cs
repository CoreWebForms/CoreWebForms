// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    [SupportsEventValidation]
    internal sealed class ZoneButton : Button {

        private readonly WebZone _owner;
        private readonly string _eventArgument;

        public ZoneButton(WebZone owner, string eventArgument) {
            if (owner == null) {
                throw new ArgumentNullException(nameof(owner));
            }
            _owner = owner;
            _eventArgument = eventArgument;
        }

        [
        DefaultValue(false),
        ]
        public override bool UseSubmitBehavior {
            get {
                return false;
            }
            set {
                // This is an internal sealed class so we know the setter is never called.
                throw new InvalidOperationException();
            }
        }

        protected override PostBackOptions GetPostBackOptions() {
            // _owner.Page may be null in the designer
            if (!String.IsNullOrEmpty(_eventArgument) && _owner.Page != null) {
                PostBackOptions options = new PostBackOptions(_owner, _eventArgument);
                options.ClientSubmit = true;

                return options;
            }

            return base.GetPostBackOptions();
        }
    }
}
