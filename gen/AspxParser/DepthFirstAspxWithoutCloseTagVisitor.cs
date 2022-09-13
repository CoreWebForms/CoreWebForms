// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class DepthFirstAspxWithoutCloseTagVisitor<T> : IAspxVisitor<T>
{
    public virtual T Visit(AspxNode.Root node)
    {
        return VisitChildren(node);
    }

    public T Visit(AspxNode.OpenHtmlTag node)
    {
        return Visit((AspxNode.HtmlTag)node);
    }

    public T Visit(AspxNode.SelfClosingHtmlTag node)
    {
        return Visit((AspxNode.HtmlTag)node);
    }

    public virtual T Visit(AspxNode.HtmlTag node)
    {
        return VisitChildren(node);
    }

    public T Visit(AspxNode.OpenAspxTag node)
    {
        return Visit((AspxNode.AspxTag)node);
    }

    public T Visit(AspxNode.SelfClosingAspxTag node)
    {
        return Visit((AspxNode.AspxTag)node);
    }

    public virtual T Visit(AspxNode.AspxTag node)
    {
        return VisitChildren(node);
    }

    public T Visit(AspxNode.CloseHtmlTag node)
    {
        return default;
    }

    public T Visit(AspxNode.CloseAspxTag node)
    {
        return default;
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
