//------------------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

#nullable disable

namespace System.Web.UI
{
    using System.Reflection;

    internal static class Util
    {

        internal static object InvokeMethod(
                                           MethodInfo methodInfo,
                                           object obj,
                                           object[] parameters)
        {
            try
            {
                return methodInfo.Invoke(obj, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
