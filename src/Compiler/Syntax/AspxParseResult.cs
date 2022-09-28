// MIT License.

using System.Collections.Immutable;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

public class AspxParseResult
{
    public AspxNode.Root RootNode { get; }

    public ImmutableArray<AspxParseError> ParseErrors { get; }

    public AspxParseResult(AspxNode.Root rootNode, ImmutableArray<AspxParseError> parseErrors)
    {
        RootNode = rootNode;
        ParseErrors = parseErrors;
    }
}
