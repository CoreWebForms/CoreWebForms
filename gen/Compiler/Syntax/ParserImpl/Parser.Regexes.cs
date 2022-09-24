// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.RegexOptions;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

partial class Parser
{
    private readonly Regex tagRegex;

    private static readonly Regex tagRegex35 = new Regex(
        "\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex tagRegex40 = new Regex(
        "\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=\"'/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex directiveRegex = new Regex(
        "\\G<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s\"'%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex endtagRegex = new Regex(
        "\\G</(?<tagname>[\\w:\\.]+)\\s*>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex aspCodeRegex = new Regex(
        "\\G<%(?!@)(?<code>.*?)%>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex aspExprRegex = new Regex(
        "\\G<%\\s*?=(?<code>.*?)?%>",
         Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex aspEncodedExprRegex = new Regex(
        "\\G<%:(?<code>.*?)?%>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex databindExprRegex = new Regex(
        "\\G<%#(?<encode>:)?(?<code>.*?)?%>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex commentRegex = new Regex(
        "\\G<%--(([^-]*)-)*?-%>",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex includeRegex = new Regex(
        "\\G<!--\\s*#(?i:include)\\s*(?<pathtype>[\\w]+)\\s*=\\s*[\"']?(?<filename>[^\\\"']*?)[\"']?\\s*-->",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));

    private static readonly Regex textRegex = new Regex(
        "\\G[^<]+",
        Compiled | Multiline | Singleline, TimeSpan.FromTicks(-10000L));
}
