// MIT License.

using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

internal class AspxOutputVisitor : DepthFirstAspxVisitor<object>
{
    private readonly TextWriter writer;

    public AspxOutputVisitor(TextWriter textWriter)
    {
        writer = textWriter;
    }

    public override object Visit(AspxNode.AspxDirective node)
    {
        writer.Write("<%@ " + node.Name);
        WriteAttributes(node.Attributes);
        writer.WriteLine(" %>");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.CloseHtmlTag node)
    {
        writer.Write("</" + node.Name + ">");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.CloseAspxTag node)
    {
        writer.Write("</" + node.Prefix + ":" + node.ControlName + ">");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.CodeRenderExpression node)
    {
        writer.Write("<%=" + node.Expression + "%>");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.OpenAspxTag node)
    {
        writer.Write($"<{node.Prefix}:{node.ControlName}");
        WriteAttributes(node.Attributes);
        writer.Write(">");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.OpenHtmlTag node)
    {
        writer.Write("<" + node.Name);
        WriteAttributes(node.Attributes);
        writer.Write(">");
        return base.Visit(node);
    }

    public override object Visit(AspxNode.SelfClosingAspxTag node)
    {
        writer.Write($"<{node.Prefix}:{node.ControlName}");
        WriteAttributes(node.Attributes);
        writer.Write("/>");
        Debug.Assert(node.Children.Count == 0);
        return null;
    }

    public override object Visit(AspxNode.SelfClosingHtmlTag node)
    {
        writer.Write("<" + node.Name);
        WriteAttributes(node.Attributes);
        writer.Write("/>");
        Debug.Assert(node.Children.Count == 0);
        return null;
    }

    public override object Visit(AspxNode.Literal node)
    {
        writer.Write(node.Text);
        Debug.Assert(node.Children.Count == 0);
        return null;
    }

    private void WriteAttributes(TagAttributes attributes)
    {
        if (attributes.IsRunAtServer)
        {
            writer.Write(" runat=\"server\"");
        }
        if (!attributes.Id.IsNullOrEmpty())
        {
            writer.Write($" id=\"{attributes.Id}\"");
        }
        foreach (var pair in attributes)
        {
            writer.Write(" ");
            writer.Write(pair.Key);
            writer.Write("=\"");
            writer.Write(pair.Value);
            writer.Write("\"");
        }
    }
}
