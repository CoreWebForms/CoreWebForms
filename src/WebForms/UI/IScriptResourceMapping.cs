// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;
using System.Reflection;

internal interface IScriptResourceMapping
{
    IScriptResourceDefinition GetDefinition(string resourceName);
    IScriptResourceDefinition GetDefinition(string resourceName, Assembly resourceAssembly);
}
