// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;
[Serializable]
public sealed class Pair
{

    public object First;

    public object Second;

    public Pair()
    {
    }

    public Pair(object x, object y)
    {
        First = x;
        Second = y;
    }
}
