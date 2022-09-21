// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

using System;
using System.ComponentModel;

/// <devdoc>
/// </devdoc>
public interface IDataItemContainer : INamingContainer
{

    object DataItem
    {
        get;
    }

    int DataItemIndex
    {
        get;
    }

    int DisplayIndex
    {
        get;
    }
}

