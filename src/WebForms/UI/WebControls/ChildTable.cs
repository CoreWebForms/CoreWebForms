// MIT License.

namespace System.Web.UI.WebControls;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.UI;

[
ToolboxItem(false),
SupportsEventValidation,
]

/// <internalonly/>
/// <devdoc>
///   Used by composite controls that are based on a table, that only render
///   their contents.
///   Used to render out an ID attribute representing the parent composite control
///   if an ID is not actually set on this table.
/// </devdoc>
internal class ChildTable : Table
{

    private readonly int _parentLevel;
    private readonly string _parentID;
    private readonly bool _parentIDSet;

    /// <internalonly/>
    internal ChildTable() : this(1)
    {
    }

    /// <internalonly/>
    internal ChildTable(int parentLevel)
    {
        Debug.Assert(parentLevel >= 1);
        _parentLevel = parentLevel;
        _parentIDSet = false;
    }

    internal ChildTable(string parentID)
    {
        _parentID = parentID;
        _parentIDSet = true;
    }

    /// <internalonly/>
    protected override void AddAttributesToRender(HtmlTextWriter writer)
    {
        base.AddAttributesToRender(writer);
        string parentID = _parentID;

        if (!_parentIDSet)
        {
            parentID = GetParentID();
        }

        if (parentID != null)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, parentID);
        }
    }

    /// <devdoc>
    /// Gets the ClientID of the parent whose ID is supposed to be used in the rendering.
    /// </devdoc>
    private string GetParentID()
    {
        if (ID != null)
        {
            return null;
        }

        Control parent = this;
        for (int i = 0; i < _parentLevel; i++)
        {
            parent = parent.Parent;
            if (parent == null)
            {
                break;
            }
        }

        Debug.Assert(parent != null);
        if (parent != null)
        {
            string id = parent.ID;
            if (!String.IsNullOrEmpty(id))
            {
                return parent.ClientID;
            }
        }

        return null;
    }
}
