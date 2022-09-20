// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Web.UI;

internal class AssemblyResourceLoader
{
    internal static Assembly GetAssemblyFromType(Type type) => type.Assembly;

    internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager, bool enableCdn)
    {
        return "";
    }
}
