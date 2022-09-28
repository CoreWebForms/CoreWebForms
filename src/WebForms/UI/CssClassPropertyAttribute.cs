// MIT License.

namespace System.Web.UI;
/// <devdoc>
///     CssClassPropertyAttribute 
///     The CssClassPropertyAttribute is applied to properties that contain CssClass names.
///     The designer uses this attribute to add a design-time CssClass editor experience
///     to the property in the property grid.
/// </devdoc>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CssClassPropertyAttribute : Attribute
{

    public CssClassPropertyAttribute()
    {
    }
}
