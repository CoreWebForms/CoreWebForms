// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

partial class Parser
{
    private void ProcessDirective(Match match)
    {
        ProcessLiteral(match.Index);

        string directiveName;
        var attributes = ProcessAttributes(match, true, out directiveName);

        var location = CreateLocation(match);
        eventListener.OnDirective(location, directiveName, attributes);

        ignoreNextSpaceString = true;
    }
}
