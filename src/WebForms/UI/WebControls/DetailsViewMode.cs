//MIT license

namespace System.Web.UI.WebControls; 

/// <devdoc>
/// <para>Specifies the DetailsView edit/view mode.</para>
/// </devdoc>
public enum DetailsViewMode {
    

    /// <devdoc>
    /// <para> 
    /// The control is in read-only mode.</para>
    /// </devdoc>
    ReadOnly = 0,

    /// <devdoc>
    /// <para> 
    /// The control is editing an existing record for update.</para>
    /// </devdoc>
    Edit = 1,

    /// <devdoc>
    /// <para> 
    /// The control is editing a new record for insert.</para>
    /// </devdoc>
    Insert = 2
}

