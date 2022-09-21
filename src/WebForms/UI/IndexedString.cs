// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

using System;

[Serializable]
public sealed class IndexedString
{
    public IndexedString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            throw new ArgumentNullException(nameof(s));
        }

        Value = s;
    }

    public string Value { get; }
}
