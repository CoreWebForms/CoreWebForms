// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Web.UI.WebControls;

internal static class ValidatorCompatibilityHelper
{
    public static void RegisterArrayDeclaration(Control control, string arrayName, string arrayValue)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterArrayDeclaration",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       null, /*binder*/
                                       null, /*target*/
                                       new object[] { control, arrayName, arrayValue });
    }

    [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[])", Justification = @"The default is for the thread's culture to be used, which is fine.")]
    public static void RegisterClientScriptResource(Control control, string resourceName)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterNamedClientScriptResource",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       binder: null,
                                       target: null,
                                       args: new object[] { control, resourceName });
    }

    [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[])", Justification = @"The default is for the thread's culture to be used, which is fine.")]
    public static void RegisterClientScriptResource(Control control, Type type, string resourceName)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterClientScriptResource",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       null, /*binder*/
                                       null, /*target*/
                                       new object[] { control, type, resourceName });
    }

    public static void RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterExpandoAttribute",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       null, /*binder*/
                                       null, /*target*/
                                       new object[] { control, controlId, attributeName, attributeValue, encode });
    }

    public static void RegisterOnSubmitStatement(Control control, Type type, string key, string script)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterOnSubmitStatement",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       null, /*binder*/
                                       null, /*target*/
                                       new object[] { control, type, key, script });
    }

    public static void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags)
    {
        Type scriptManagerType = control.Page.ScriptManagerType;
        Debug.Assert(scriptManagerType != null);

        scriptManagerType.InvokeMember("RegisterStartupScript",
                                       BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                       null, /*binder*/
                                       null, /*target*/
                                       new object[] { control, type, key, script, addScriptTags });
    }
}
