//MIT license

namespace System.Web.UI.WebControls;

/// <devdoc>
///    <para>Specifies the type of the item in a list.</para>
/// </devdoc>
public enum SiteMapNodeItemType
{

    /// <devdoc>
    ///    <para> 
    ///       A root node. It is databound.</para>
    /// </devdoc>
    Root = 0,

    /// <devdoc>
    ///    <para> 
    ///       A parent node. It is databound.</para>
    /// </devdoc>
    Parent = 1,

    /// <devdoc>
    ///    A current node. It is databound.
    /// </devdoc>
    Current = 2,

    /// <devdoc>
    ///    <para> A path separator. It is not databound.</para>
    /// </devdoc>
    PathSeparator = 3
}
