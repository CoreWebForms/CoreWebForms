// MIT License.

using System;
using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

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
