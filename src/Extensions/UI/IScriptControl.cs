//------------------------------------------------------------------------------
// <copyright file="IScriptControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI
{
    public interface IScriptControl {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        IEnumerable<ScriptDescriptor> GetScriptDescriptors();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        IEnumerable<ScriptReference> GetScriptReferences();
    }
}
