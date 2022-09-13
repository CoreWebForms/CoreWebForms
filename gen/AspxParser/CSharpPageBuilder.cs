// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class CSharpPageBuilder
{
    private readonly string _path;
    private readonly IndentedTextWriter _writer;
    private readonly IDisposable _indentClose;
    private readonly IDisposable _blockClose;
    private readonly AspxParseResult _tree;

    private int _count;

    public CSharpPageBuilder(string path, IndentedTextWriter writer, string contents)
    {
        _path = path;
        _writer = writer;
        _indentClose = new IndentClose(writer, includeBrace: false);
        _blockClose = new IndentClose(writer, includeBrace: true);

        var parser = new AspxParser();
        var source = new AspxSource(_path, contents);

        _tree = parser.Parse(source);
    }

    public string Name { get; private set; }

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
            WriteInitializeComponents(_tree.RootNode.Children);
        }
    }

    private void WriteDirectiveDetails(AspxNode.AspxDirective d)
    {
        var info = new DirectiveDetails(d);
        var className = ConvertPathToClassName(info.CodeBehind);

        Name = className;

        _writer.Write("[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(\"");
        _writer.Write(_path);
        _writer.WriteLine("\")]");
        _writer.Write("internal partial class ");
        _writer.Write(className);
        _writer.Write(" : ");
        _writer.WriteLine(info.Inherits);
    }

    private void WriteInitializeComponents(IEnumerable<AspxNode> nodes)
    {
        _writer.WriteLine("protected override void InitializeComponents()");

        using (Block())
        {
            foreach (var node in nodes)
            {
                if (node is AspxNode.Literal literal)
                {
                    Write(literal);
                }
                else if (node is AspxNode.SelfClosingHtmlTag selfClosing)
                {
                    Write(selfClosing);
                }
            }
        }
    }

    private void Write(AspxNode.Literal literal)
    {
        var name = GetNextControlName();
        var normalized = NormalizeLiteral(literal.Text);

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
        _writer.Write(normalized);
        _writer.WriteLine("\");");
        _writer.Write("Controls.Add(");
        _writer.Write(name);
        _writer.WriteLine(");");
    }

    private void Write(AspxNode.SelfClosingHtmlTag tag)
    {
        var name = GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
        _writer.Write("<");
        _writer.Write(tag.Name);
        _writer.Write(" />");
        _writer.WriteLine("\");");
        _writer.Write("Controls.Add(");
        _writer.Write(name);
        _writer.WriteLine(");");
    }

    private string NormalizeLiteral(string input)
        => input
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");

    private string GetNextControlName() => $"control{_count++}";

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
