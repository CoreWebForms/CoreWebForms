// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
