// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.Transpiler.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.Transpiler;

partial class Parser
{
    private void ProcessCodeBlock(Match match, CodeBlockType blockType)
    {
        ProcessLiteral(match.Index);

        var code = match.Groups["code"];
        var isEncode = match.Groups["encode"].Success;

        var startPos = code.Index;
        var endPos = code.Index + code.Length;

        // trim whitespaces at the start
        while (startPos < endPos && char.IsWhiteSpace(text[startPos]))
        {
            ++startPos;
        }

        // trim whitespaces at the end
        while (startPos < endPos && char.IsWhiteSpace(text[endPos - 1]))
        {
            --endPos;
        }

        var codeText = text.Substring(startPos, endPos - startPos).Replace("%\\>", "%>");
        var location = CreateLocation(startPos, endPos);
        eventListener.OnCodeBlock(location, blockType, codeText, isEncode);

        if (blockType == CodeBlockType.Code)
        {
            ignoreNextSpaceString = true;
        }
    }
}
