// MIT License.

using System.ComponentModel;
using System.Web.Resources;

namespace System.Web.UI;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class ResourceDisplayNameAttribute : DisplayNameAttribute
{
    private bool _resourceLoaded;
    private readonly string _displayNameResourceName;

    public ResourceDisplayNameAttribute(string displayNameResourceName)
    {
        _displayNameResourceName = displayNameResourceName;
    }

    public override string DisplayName
    {
        get
        {
            if (!_resourceLoaded)
            {
                _resourceLoaded = true;
                DisplayNameValue = AtlasWeb.ResourceManager.GetString(_displayNameResourceName, AtlasWeb.Culture);
            }
            return base.DisplayName;
        }
    }
}
