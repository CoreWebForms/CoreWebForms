// MIT License.

using System.ComponentModel;

namespace System.Web.UI;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public delegate System.Web.UI.Control ControlSkinDelegate(Control control);

[EditorBrowsable(EditorBrowsableState.Advanced)]
public class ControlSkin
{

    private readonly Type _controlType;
    private readonly ControlSkinDelegate _controlSkinDelegate;

    public ControlSkin(Type controlType, ControlSkinDelegate themeDelegate)
    {
        _controlType = controlType;
        _controlSkinDelegate = themeDelegate;
    }

    public Type ControlType
    {
        get
        {
            return _controlType;
        }
    }

    public void ApplySkin(Control control)
    {
        _controlSkinDelegate(control);
    }
}
