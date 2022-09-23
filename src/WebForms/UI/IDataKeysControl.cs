//------------------------------------------------------------------------------
// <copyright file="IDataKeysControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Web.UI.WebControls;

namespace System.Web.UI {
    public interface IDataKeysControl {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
                         Justification="Required by ASP.NET Parser.")]
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        string[] ClientIDRowSuffix { get; }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        DataKeyArray ClientIDRowSuffixDataKeys { get; }
    }
}
