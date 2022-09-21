// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;
using System;
using System.Security.Permissions;

public interface IDataBoundItemControl : IDataBoundControl
{
    DataKey DataKey
    {
        get;
    }

    DataBoundControlMode Mode
    {
        get;
    }
}
