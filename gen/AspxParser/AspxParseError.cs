// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser;

public class AspxParseError
{
    public Location Location { get; }

    public string Message { get; }

    public AspxParseError(Location location, string message)
    {
        Location = location;
        Message = message;
    }
}
