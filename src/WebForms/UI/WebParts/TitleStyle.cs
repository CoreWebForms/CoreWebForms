// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class TitleStyle : TableItemStyle {

        public TitleStyle() {
            Wrap = false;
        }

        [
        DefaultValue(false)
        ]
        public override bool Wrap {
            get {
                return base.Wrap;
            }
            set {
                base.Wrap = value;
            }
        }
    }
}
