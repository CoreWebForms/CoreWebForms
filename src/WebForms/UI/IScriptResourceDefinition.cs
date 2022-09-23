// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

#nullable disable

namespace System.Web.UI;
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
