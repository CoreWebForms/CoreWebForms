// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

public sealed class ClientScriptManager
{
    private readonly Page _page;

    public ClientScriptManager(Page page)
    {
        _page = page;
    }

    internal void ValidateEvent(string uniqueID, string eventArgument)
    {
        throw new NotImplementedException();
    }
}
