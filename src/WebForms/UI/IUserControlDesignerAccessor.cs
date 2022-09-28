// MIT License.

namespace System.Web.UI;

/// <devdoc>
///   <para>Allows designer functionality to access information about a UserControl, that is
///     applicable at design-time only.
///   </para>
/// </devdoc>
public interface IUserControlDesignerAccessor
{

    string InnerText
    {
        get;
        set;
    }

    string TagName
    {
        get;
        set;
    }
}
