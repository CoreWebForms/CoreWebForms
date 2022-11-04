// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

public class AspxParseError
{
    public Location Location { get; }

    public string Message { get; }

    public AspxParseError(Location location, string message)
    {
        Location = location;
        Message = message;
    }
}
