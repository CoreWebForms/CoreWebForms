// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

#nullable disable

namespace System.Web.UI;
internal interface IScriptResourceMapping
{
    IScriptResourceDefinition GetDefinition(string resourceName);
    IScriptResourceDefinition GetDefinition(string resourceName, Assembly resourceAssembly);
}
