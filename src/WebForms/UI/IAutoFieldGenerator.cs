//------------------------------------------------------------------------------
// <copyright file="IDynamicDataManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System.Collections;

namespace System.Web.UI
{
    public interface IAutoFieldGenerator {
        ICollection GenerateFields(Control control);
    }

}
