// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

partial class Parser
{
    private bool ProcessEndTag(Match match)
    {
        var name = match.Groups["tagname"].Value;
        if (inScriptTag)
        {
            if ("script".EqualsNoCase(name))
            {
                inScriptTag = false;
            }
            else
            {
                return false;
            }
        }

        ProcessLiteral(match.Index);

        var location = CreateLocation(match);
        eventListener.OnTag(location, TagType.Close, string.Intern(name), TagAttributes.Empty);
        return true;
    }
}
