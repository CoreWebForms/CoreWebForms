// MIT License.

using System.ComponentModel;
using System.Web.Resources;

namespace System.Web.UI;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
internal sealed class ResourceDescriptionAttribute : DescriptionAttribute
{
    private bool _resourceLoaded;
    private readonly string _descriptionResourceName;

    public ResourceDescriptionAttribute(string descriptionResourceName)
    {
        _descriptionResourceName = descriptionResourceName;
    }

    public override string Description
    {
        get
        {
            if (!_resourceLoaded)
            {
                _resourceLoaded = true;
                DescriptionValue = AtlasWeb.ResourceManager.GetString(_descriptionResourceName, AtlasWeb.Culture);
            }
            return base.Description;
        }
    }
}
