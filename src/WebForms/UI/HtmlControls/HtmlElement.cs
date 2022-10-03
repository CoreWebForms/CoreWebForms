// MIT License.

using System.ComponentModel;

namespace System.Web.UI.HtmlControls;
public class HtmlElement : HtmlContainerControl
{

    public HtmlElement() : base("html")
    {
    }

    [
    WebCategory("Behavior"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    UrlProperty()
    ]
    public string Manifest
    {
        get
        {
            string s = Attributes["manifest"];
            return s ?? String.Empty;
        }
        set
        {
            Attributes["manifest"] = MapStringAttributeToString(value);
        }
    }

    /*
     * Override to process manifest attribute
     */
    protected override void RenderAttributes(HtmlTextWriter writer)
    {
        PreProcessRelativeReferenceAttribute(writer, "manifest");
        base.RenderAttributes(writer);
    }

}
