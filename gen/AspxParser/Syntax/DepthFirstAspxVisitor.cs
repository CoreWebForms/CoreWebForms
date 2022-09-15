// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax;

public class DepthFirstAspxVisitor<T> : IAspxVisitor<T>
{
    public virtual T Visit(AspxNode.Root node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.OpenHtmlTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.SelfClosingHtmlTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.OpenAspxTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.SelfClosingAspxTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.CloseHtmlTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.CloseAspxTag node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.AspxDirective node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.DataBinding node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.CodeRender node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.CodeRenderExpression node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.CodeRenderEncode node)
    {
        return VisitChildren(node);
    }

    public virtual T Visit(AspxNode.Literal node)
    {
        return VisitChildren(node);
    }

    protected virtual T VisitChildren(AspxNode node)
    {
        foreach (var child in node.Children)
        {
            child.Accept(this);
        }

        return default;
    }
}
