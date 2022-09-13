// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

internal readonly struct DirectiveDetails
{
    private readonly AspxNode.AspxDirective _directive;

    public DirectiveDetails(AspxNode.AspxDirective directive)
    {
        _directive = directive;
    }

    public string MasterPageFile => GetAttribute();

    public string Language => GetAttribute();

    public string Inherits => GetAttribute() ?? "global::System.Web.UI.Page";

    public bool AutoEventWireup => bool.TryParse(GetAttribute(), out var result) ? result : false;

    public string Title => GetAttribute();

    public string CodeBehind => GetAttribute();

    private string GetAttribute([CallerMemberName] string name = null!)
        => _directive.Attributes[name];
}
