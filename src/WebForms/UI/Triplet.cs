// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;
[Serializable]
public sealed class Triplet
{

    public object First;

    public object Second;

    public object Third;

    public Triplet()
    {
    }

    public Triplet(object x, object y)
    {
        First = x;
        Second = y;
    }

    public Triplet(object x, object y, object z)
    {
        First = x;
        Second = y;
        Third = z;
    }
}
