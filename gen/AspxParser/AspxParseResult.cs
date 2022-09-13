// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

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
