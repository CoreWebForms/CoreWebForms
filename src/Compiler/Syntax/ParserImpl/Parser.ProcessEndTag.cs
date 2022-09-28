// MIT License.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

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
