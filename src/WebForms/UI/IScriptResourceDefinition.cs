// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;
using System.Reflection;

internal interface IScriptResourceDefinition
{
    string Path { get; }
    string DebugPath { get; }
    string CdnPath { get; }
    string CdnDebugPath { get; }
    string CdnPathSecureConnection { get; }
    string CdnDebugPathSecureConnection { get; }
    string ResourceName { get; }
    Assembly ResourceAssembly { get; }
}
