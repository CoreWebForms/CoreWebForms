// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Web.UI.WebControls;

namespace System.Web.UI;

public interface IDataKeysControl
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
                     Justification = "Required by ASP.NET Parser.")]
    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
    string[] ClientIDRowSuffix { get; }

    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
    DataKeyArray ClientIDRowSuffixDataKeys { get; }
}
