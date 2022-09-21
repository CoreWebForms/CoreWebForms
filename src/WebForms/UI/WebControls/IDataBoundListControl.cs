// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

public interface IDataBoundListControl : IDataBoundControl
{
    DataKeyArray DataKeys
    {
        get;
    }

    DataKey SelectedDataKey
    {
        get;
    }

    int SelectedIndex
    {
        get;
        set;
    }

    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required by ASP.NET parser.")]
    string[] ClientIDRowSuffix
    {
        get;
        set;
    }

    bool EnablePersistedSelection
    {
        get;
        set;
    }
}
