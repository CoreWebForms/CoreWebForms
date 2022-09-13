// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

partial class Parser
{
    private TagAttributes ProcessAttributes(Match match, bool isDirectiveAttributes, out string directiveName)
    {
        directiveName = string.Empty;

        string id = null;
        bool isRunAtServer = false;
        var attributes = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
        var attributeNames = match.Groups["attrname"].Captures;
        var attributeValues = match.Groups["attrval"].Captures;
        for (var i = 0; i < attributeNames.Count; ++i)
        {
            var attributeName = attributeNames[i].Value;

            if (isDirectiveAttributes && i == 0 && match.Groups["equal"].Captures[0].Length == 0)
            {
                directiveName = attributeName;
                continue;
            }

            var attributeValue = WebUtility.HtmlDecode(attributeValues[i].Value);

            if ("id".EqualsNoCase(attributeName))
            {
                id = attributeValue;
            }
            else if ("runat".EqualsNoCase(attributeName))
            {
                isRunAtServer = "server".EqualsNoCase(attributeValue);
            }
            else if (attributes.ContainsKey(attributeName))
            {
                var location = CreateLocation(match);
                eventListener.OnError(location, $"Duplicated tag attribute `{attributeName}`.");
            }
            else
            {
                attributes.Add(string.Intern(attributeName), attributeValue);
            }
        }

        return new TagAttributes(id, isRunAtServer, attributes.ToImmutable());
    }
}
