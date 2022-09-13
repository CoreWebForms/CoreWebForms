// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser;
using Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser.ParserImpl;

namespace AspxParser;

partial class Parser
{
    public Tuple<Location, string, TagAttributes> TryParseDirective()
    {
        var match = directiveRegex.Match(text);
        if (match.Success)
        {
            var location = CreateLocation(match);
            string directiveName;
            var attributes = ProcessAttributes(match, true, out directiveName);
            return Tuple.Create(location, directiveName, attributes);
        }
        return null;
    }

    public void Parse()
    {
        var lastTagEnd = text.LastIndexOf('>');
        var pos = 0;
        while (pos < text.Length)
        {
            Match match;
            var skipParsedTag = false;

            if (IsMatch(textRegex, pos, out match))
            {
                AppendLiteral(pos, match.Value);
            }
            else if (!inScriptTag && IsMatch(directiveRegex, pos, out match))
            {
                ProcessDirective(match);
            }
            else if (IsMatch(includeRegex, pos, out match))
            {
                ProcessServerInclude(match);
            }
            else if (IsMatch(commentRegex, pos, out match))
            {
                var location = CreateLocation(match);
                eventListener.OnComment(location, match.Value);
            }
            else if (!inScriptTag && IsMatch(aspExprRegex, pos, out match))
            {
                ProcessCodeBlock(match, CodeBlockType.Expression);
            }
            else if (!inScriptTag && IsMatch(aspEncodedExprRegex, pos, out match))
            {
                ProcessCodeBlock(match, CodeBlockType.EncodedExpression);
            }
            else if (!inScriptTag && IsMatch(databindExprRegex, pos, out match))
            {
                ProcessCodeBlock(match, CodeBlockType.DataBinding);
            }
            else if (!inScriptTag && IsMatch(aspCodeRegex, pos, out match))
            {
                ProcessCodeBlock(match, CodeBlockType.Code);
            }
            else if (!inScriptTag && pos < lastTagEnd && IsMatch(tagRegex, pos, out match))
            {
                if (!ProcessBeginTag(match))
                {
                    skipParsedTag = true;
                }
            }
            else if (IsMatch(endtagRegex, pos, out match))
            {
                if (!ProcessEndTag(match))
                {
                    skipParsedTag = true;
                }
            }

            if (match.Success && !skipParsedTag)
            {
                pos = match.Index + match.Length;
            }
            else
            {
                AppendLiteral(pos, text[pos]);
                ++pos;
            }
        } // while

        if (inScriptTag)
        {
            var location = CreateLocation(pos, text.Length);
            eventListener.OnError(location, "Unexpected end of file while processing tag `script`.");
        }
        else
        {
            ProcessLiteral(text.Length);
        }
    }

    private bool IsMatch(Regex regex, int pos, out Match match)
    {
        match = regex.Match(text, pos);
        return match.Success;
    }
}
