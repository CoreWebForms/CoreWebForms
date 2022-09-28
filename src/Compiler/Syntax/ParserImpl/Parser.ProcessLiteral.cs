// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

partial class Parser
{
    private void AppendLiteral(int pos, string t)
    {
        if (currentLiteralStart < 0)
        {
            currentLiteralStart = pos;
        }

        currentLiteral.Append(t);
    }

    private void AppendLiteral(int pos, char c)
    {
        if (currentLiteralStart < 0)
        {
            currentLiteralStart = pos;
        }

        currentLiteral.Append(c);
    }

    private void ProcessLiteral(int pos)
    {
        if (currentLiteralStart < 0)
        {
            ignoreNextSpaceString = false;
            return;
        }

        var literal = currentLiteral.ToString();

        var ignoreLiteral = false;
        if (ignoreNextSpaceString)
        {
            ignoreNextSpaceString = false;

            if (string.IsNullOrWhiteSpace(literal))
            {
                ignoreLiteral = true;
            }
        }

        if (!ignoreLiteral)
        {
            var location = CreateLocation(currentLiteralStart, pos);
            eventListener.OnLiteral(location, literal);
        }

        currentLiteral.Clear();
        currentLiteralStart = -1;
    }
}
