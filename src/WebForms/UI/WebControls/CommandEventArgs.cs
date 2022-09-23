// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI.WebControls;
/// <devdoc>
/// <para>Provides data for the <see langword='Command'/> event.</para>
/// </devdoc>
public class CommandEventArgs : EventArgs
{

    private readonly string commandName;
    private readonly object argument;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.CommandEventArgs'/> class with another <see cref='System.Web.UI.WebControls.CommandEventArgs'/>.</para>
    /// </devdoc>
    public CommandEventArgs(CommandEventArgs e) : this(e.CommandName, e.CommandArgument)
    {
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.CommandEventArgs'/> class with the specified command name 
    ///    and argument.</para>
    /// </devdoc>
    public CommandEventArgs(string commandName, object argument)
    {
        this.commandName = commandName;
        this.argument = argument;
    }

    /// <devdoc>
    ///    <para>Gets the name of the command. This property is read-only.</para>
    /// </devdoc>
    public string CommandName => commandName;

    /// <devdoc>
    ///    <para>Gets the argument for the command. This property is read-only.</para>
    /// </devdoc>
    public object CommandArgument => argument;
}

