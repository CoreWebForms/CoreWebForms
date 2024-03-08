// MIT License.

namespace System.Web.UI;

/// <devdoc>
/// Specifies a conflict resolution mode.
/// </devdoc>
public enum ConflictOptions
{
    /// <devdoc>
    /// Specifies that only the new values and the keys will be passed to the update or delete query
    /// </devdoc>
    OverwriteChanges = 0,

    /// <devdoc>
    /// Specifies that the old values will also be passed to the update or delete query
    /// </devdoc>
    CompareAllValues = 1
}
