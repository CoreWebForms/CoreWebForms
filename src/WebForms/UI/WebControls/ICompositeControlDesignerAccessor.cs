// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

/// <devdoc>
/// Allows the designer of a composite control to recreate the composite control's child controls.
///
/// ****************************************************************************
/// THIS IS AN INTERIM SOLUTION UNTIL FRIEND ASSEMBLY FUNCTIONALITY COMES ONLINE
/// ****************************************************************************
///
/// </devdoc>
public interface ICompositeControlDesignerAccessor
{
    /// <devdoc>
    /// Recreates the child controls.
    /// </devdoc>
    void RecreateChildControls();
}
