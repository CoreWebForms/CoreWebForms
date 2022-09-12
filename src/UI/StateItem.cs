// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;
/*
 * The StateItem class * by the StateBag class.
 * The StateItem has an object value, a dirty flag.
 */

/// <devdoc>
/// <para>Represents an item that is saved in the <see cref='System.Web.UI.StateBag'/> class when view state 
///    information is persisted between Web requests.</para>
/// </devdoc>
public sealed class StateItem
{
    private object value;
    private bool isDirty;

    /*
     * Constructs a StateItem with an initial value.
     */
    internal StateItem(object initialValue)
    {
        value = initialValue;
        isDirty = false;
    }

    /*
     * Property to indicate StateItem has been modified.
     */

    /// <devdoc>
    /// <para>Indicates whether the <see cref='System.Web.UI.StateItem'/> object has been modified.</para>
    /// </devdoc>
    public bool IsDirty
    {
        get
        {
            return isDirty;
        }
        set
        {
            isDirty = value;
        }
    }

    /*
     * Property to access the StateItem value.
     */

    /// <devdoc>
    /// <para>Indicates the value of the item that is stored in the <see cref='System.Web.UI.StateBag'/> 
    /// object.</para>
    /// </devdoc>
    public object Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
        }
    }
}
