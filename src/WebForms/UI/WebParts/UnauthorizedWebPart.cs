// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    [
    ToolboxItem(false)
    ]
    public sealed class UnauthorizedWebPart : ProxyWebPart {

        public UnauthorizedWebPart(WebPart webPart) : base(webPart) {
        }

        public UnauthorizedWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID) :
            base(originalID, originalTypeName, originalPath, genericWebPartID) {
        }
    }
}
