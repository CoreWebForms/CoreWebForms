// MIT License.

namespace System.Web.UI.WebControls;

using System;

/// <devdoc>
///    Provides doata for TreeView events
/// </devdoc>
public sealed class TreeNodeEventArgs : EventArgs
{
    private readonly TreeNode _node;

    /// <devdoc>
    ///    Initializes a new instance of the <see cref='System.Web.UI.WebControls.TreeNodeEventArgs' />
    /// </devdoc>
    public TreeNodeEventArgs(TreeNode node)
    {
        _node = node;
    }

    /// <devdoc>
    ///    The node on which the event is occurring.
    /// </devdoc>
    public TreeNode Node
    {
        get
        {
            return _node;
        }
    }
}
