// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

internal partial class Parser
{
    private readonly IParserEventListener eventListener;
    private readonly IAspxSource source;
    private readonly string text;
    private readonly StringBuilder currentLiteral = new StringBuilder();
    private int currentLiteralStart = -1;
    private bool inScriptTag;
    private bool ignoreNextSpaceString;

    public Parser(IParserEventListener eventListener, bool isFw40, IAspxSource source)
    {
        this.eventListener = eventListener;
        this.source = source;
        text = source.Text;
        tagRegex = isFw40 ? tagRegex40 : tagRegex35;
    }

    private Location CreateLocation(Match match) =>
        CreateLocation(match.Index, match.Index + match.Length);

    private Location CreateLocation(int startPos, int endPos) =>
        new Location(source, startPos, endPos);
}
