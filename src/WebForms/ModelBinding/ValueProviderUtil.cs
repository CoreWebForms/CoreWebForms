// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

using System;

internal static class ValueProviderUtil
{

    public static string CreateSubPropertyName(string prefix, string propertyName)
    {
        if (String.IsNullOrEmpty(prefix))
        {
            return propertyName;
        }
        else if (String.IsNullOrEmpty(propertyName))
        {
            return prefix;
        }
        else
        {
            return prefix + "." + propertyName;
        }
    }

}
