// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AspxPageAttribute : Attribute
{
    public AspxPageAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
