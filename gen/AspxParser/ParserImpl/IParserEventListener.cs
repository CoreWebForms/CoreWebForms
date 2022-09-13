// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser.ParserImpl;

internal interface IParserEventListener
{
    void OnError(Location location, string message);
    void OnComment(Location location, string comment);
    void OnLiteral(Location location, string text);
    void OnInclude(Location location, IncludePathType pathType, string path);
    void OnDirective(Location location, string name, TagAttributes attributes);
    void OnCodeBlock(Location location, CodeBlockType blockType, string code, bool isEncode);
    void OnTag(Location location, TagType tagtype, string name, TagAttributes attributes);
}
