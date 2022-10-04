// MIT License.

#nullable enable

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;

public class AspNetCompiler
{
    public static ParsedPage ParsePage(string path, string contents, IControlLookup controlInfo)
        => SymbolCreator.ParsePage(path, contents, controlInfo);
}

