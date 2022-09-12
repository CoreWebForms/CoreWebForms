// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

#nullable disable

namespace System.Web.UI;

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
