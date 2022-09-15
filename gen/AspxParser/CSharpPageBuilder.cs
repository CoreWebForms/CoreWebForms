// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class CSharpPageBuilder : DepthFirstAspxWithoutCloseTagVisitor<object>
{
    private readonly IndentedTextWriter _writer;
    private readonly IDisposable _indentClose;
    private readonly IDisposable _blockClose;
    private readonly AspxParseResult _tree;

    public CSharpPageBuilder(string path, IndentedTextWriter writer, string contents)
    {
        Path = NormalizePath(path);
        ClassName = ConvertPathToClassName(Path);

        _writer = writer;
        _indentClose = new IndentClose(writer, includeBrace: false);
        _blockClose = new IndentClose(writer, includeBrace: true);

        var parser = new AspxParser();
        var source = new AspxSource(Path, contents);

        _tree = parser.Parse(source);
    }

    public string ClassName { get; }

    public string Path { get; }

    public void WriteSource()
    {
        if (_tree.RootNode.Children.OfType<AspxNode.AspxDirective>().FirstOrDefault() is not { } d)
        {
            // Should raise error
            return;
        }

        WriteDirectiveDetails(d);

        using (Block())
        {
            _tree.RootNode.Accept(this);
        }
    }

    private readonly Stack<ComponentLevel> _componentsStack = new();

    private ComponentLevel Current => _componentsStack.Peek();

    private class ComponentLevel
    {
        private int _count = 0;

        public ComponentLevel(string controls, string prefix)
        {
            Prefix = prefix;
            Controls = controls;
        }

        public string CurrentLevel { get; private set; }

        public string GetNextControlName() => CurrentLevel = $"{Prefix}_{++_count}";

        public ComponentLevel GetNextLevel() => new($"{CurrentLevel}.Controls", CurrentLevel);

        public string Controls { get; }

        private string Prefix { get; }
    }

    private void WriteDirectiveDetails(AspxNode.AspxDirective node)
    {
        var info = new DirectiveDetails(node);

        _writer.Write("[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(\"");
        _writer.Write(Path);
        _writer.WriteLine("\")]");
        _writer.Write("internal partial class ");
        _writer.Write(ClassName);
        _writer.Write(" : ");
        _writer.WriteLine(info.Inherits);
    }

    protected override object VisitChildren(AspxNode node)
    {
        if (node.Children.Count == 0)
        {
            return _tree;
        }

        if (_componentsStack.Count == 0)
        {
            _writer.WriteLine("protected override void InitializeComponents()");
            _componentsStack.Push(new("Controls", "control"));
        }
        else
        {
            _componentsStack.Push(Current.GetNextLevel());
        }

        _writer.WriteLine("{");
        _writer.Indent++;

        base.VisitChildren(node);

        _componentsStack.Pop();

        _writer.Indent--;
        _writer.WriteLine("}");

        return _tree;
    }

    public override object Visit(AspxNode.Literal node)
    {
        var name = Current.GetNextControlName();
        var normalized = NormalizeLiteral(node.Text);

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
        _writer.Write(normalized);
        _writer.WriteLine("\");");
        _writer.Write(Current.Controls);
        _writer.Write(".Add(");
        _writer.Write(name);
        _writer.WriteLine(");");

        return _tree;
    }

    public override object Visit(AspxNode.AspxTag tag)
    {
        // TODO: Handle prefix
        // node.Prefix
        var name = Current.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        _writer.Write(" = new global::System.Web.UI.WebControls.");
        _writer.Write(tag.ControlName);
        _writer.WriteLine("();");

        if (!string.IsNullOrEmpty(tag.Attributes.Id))
        {
            _writer.Write(name);
            _writer.Write(".Id = \"");
            _writer.Write(tag.Attributes.Id);
            _writer.WriteLine("\";");
        }

        _writer.Write(Current.Controls);
        _writer.Write(".Add(");
        _writer.Write(name);
        _writer.WriteLine(");");

        return base.Visit(tag);
    }

    private readonly Dictionary<string, string> _htmlControls = new()
    {
        { "form", "HtmlForm" },
    };

    public override object Visit(AspxNode.HtmlTag tag)
    {
        var name = Current.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        if (tag.Attributes.IsRunAtServer)
        {
            if (_htmlControls.TryGetValue(tag.Name, out var known))
            {
                _writer.Write(" = new global::System.Web.UI.HtmlControls.");
                _writer.Write(known);
                _writer.WriteLine("();");
            }
            else
            {
                _writer.Write(" = new global::System.Web.UI.HtmlControls.HtmlGenericControl(\"");
                _writer.Write(tag.Name);
                _writer.WriteLine("\");");
            }
        }
        else
        {
            _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
            _writer.Write("<");
            _writer.Write(tag.Name);
            _writer.Write(" />");
            _writer.WriteLine("\");");
        }

        if (!string.IsNullOrEmpty(tag.Attributes.Id))
        {
            _writer.Write(name);
            _writer.Write(".Id = \"");
            _writer.Write(tag.Attributes.Id);
            _writer.WriteLine("\";");
        }

        _writer.Write(Current.Controls);
        _writer.Write(".Add(");
        _writer.Write(name);
        _writer.WriteLine(");");

        return base.Visit(tag);
    }

    private string NormalizeLiteral(string input)
        => input
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");

    private static string NormalizePath(string path)
    {
        var sb = new StringBuilder(path);

        if (sb[0] != '/')
        {
            sb.Insert(0, '/');
        }

        sb.Replace("\\", "/");

        return sb.ToString();
    }

    private IDisposable Block()
    {
        _writer.WriteLine("{");
        _writer.Indent++;
        return _blockClose;
    }

    private string ConvertPathToClassName(string input)
    {
        var sb = new StringBuilder(input);

        sb.Replace(".", "_");
        sb.Replace("/", "_");
        sb.Replace("\\", "_");

        return sb.ToString();
    }

    private sealed class IndentClose : IDisposable
    {
        private readonly bool _includeBrace;
        private readonly IndentedTextWriter _writer;

        public IndentClose(IndentedTextWriter writer, bool includeBrace)
        {
            _includeBrace = includeBrace;
            _writer = writer;
        }

        public void Dispose()
        {
            _writer.Indent--;

            if (_includeBrace)
            {
                _writer.WriteLine("}");
            }
        }
    }
}
