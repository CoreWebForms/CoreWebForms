// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

[Flags]
public enum DataSourceCapabilities
{

    None = 0x0,

    Sort = 0x1,

    Page = 0x2,

    RetrieveTotalRowCount = 0x4
}
