// MIT License.


using System.ComponentModel;
using System.Web.Resources;

namespace System.Web.UI;
[AttributeUsage(AttributeTargets.All)]
internal sealed class ResourceCategoryAttribute : CategoryAttribute
{

    internal ResourceCategoryAttribute(string category)
        : base(category)
    {
    }

    public override object TypeId
    {
        get
        {
            return typeof(CategoryAttribute);
        }
    }

    protected override string GetLocalizedString(string value)
    {
        string localizedValue = base.GetLocalizedString(value);
        if (localizedValue == null)
        {
            localizedValue = AtlasWeb.ResourceManager.GetString("Category_" + value, AtlasWeb.Culture);
        }
        // This attribute is internal, and we should never have a missing resource string.
        //
        System.Diagnostics.Debug.Assert(localizedValue != null, "All WebForms category attributes should have localized strings.  Category '" + value + "' not found.");
        return localizedValue;
    }
}
