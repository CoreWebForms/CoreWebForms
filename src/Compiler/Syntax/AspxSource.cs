// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

public class AspxSource : IAspxSource
{
    public static AspxSource Empty { get; } = new AspxSource("", "");

    public string Name { get; }
    public string Text { get; }

    public AspxSource(string name, string text)
    {
        Name = name;
        Text = text;
    }
}
