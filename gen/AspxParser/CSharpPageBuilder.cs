// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax;

using static Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax.AspxNode;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class CSharpPageBuilder : DepthFirstAspxVisitor<object>
{
    private readonly Dictionary<string, ControlInfo> _controls;
    private readonly IndentedTextWriter _writer;
    private readonly IDisposable _blockClose;
    private readonly AspxParseResult _tree;

    private readonly string[] DefaultUsings = new[]
    {
        "System",
        "System.Web",
    };

    public CSharpPageBuilder(string path, IndentedTextWriter writer, string contents, IEnumerable<ControlInfo> controls)
    {
        _controls = controls.ToDictionary(c => c.Name, c => c);

        Path = NormalizePath(path);
        ClassName = ConvertPathToClassName(Path);

        _writer = writer;
        _blockClose = new IndentClose(writer, includeBrace: true);

        var parser = new AspxParser();
        var source = new AspxSource(Path, contents);

        _tree = parser.Parse(source);
    }

    public ImmutableArray<AspxParseError> Errors => _tree.ParseErrors;

    public string ClassName { get; }

    public string Path { get; }

    public bool HasDirective { get; private set; }

    public void WriteSource()
    {
        if (_tree.RootNode.Children.OfType<AspxDirective>().FirstOrDefault() is not { } d)
        {
            return;
        }

        HasDirective = true;

        foreach (var u in DefaultUsings)
        {
            _writer.Write("using ");
            _writer.Write(u);
            _writer.WriteLine(';');
        }

        _writer.WriteLine();

        WriteDirectiveDetails(d);

        using (Block())
        {
            _tree.RootNode.Accept(this);

            WriteScripts();
            WriteVariables();
        }
    }

    private void WriteVariables()
    {
        if (_variables.Count == 0)
        {
            return;
        }

        foreach (var variable in _variables)
        {
            _writer.Write("protected ");
            _writer.Write(variable.Type.Namespace);
            _writer.Write('.');
            _writer.Write(variable.Type.Name);
            _writer.Write(' ');
            _writer.Write(variable.Name);
            _writer.WriteLine(';');
        }
    }

    private void WriteScripts()
    {
        if (_scripts.Count == 0)
        {
            return;
        }

        foreach (var script in _scripts)
        {
            foreach (var child in script.Children)
            {
                if (child is Literal literal)
                {
                    WriteLineInfo(literal.Location);
                    _writer.WriteLine(literal.Text.Trim());
                    _writer.WriteLine("#line default");
                }
            }
        }
    }

    private void WriteLineInfo(Location location)
    {
        _writer.Write("#line (");
        var start = GetSpan(location.Start, location.Source.Text);
        var end = GetSpan(location.End, location.Source.Text);
        _writer.Write(start.line);
        _writer.Write(", ");
        _writer.Write(start.column);
        _writer.Write(") - (");
        _writer.Write(end.line);
        _writer.Write(", ");
        _writer.Write(end.column);
        _writer.Write(") \"");
        _writer.Write(Path.Trim('/'));
        _writer.WriteLine('\"');
    }

    private (int line, int column) GetSpan(int offset, string line)
    {
        var count = 1;
        var n = 0;
        var p = 0;

        while (n < offset)
        {
            p = n;
            var idx = line.IndexOf('\n', n);

            if (idx < 0)
            {
                break;
            }

            n = idx + 1;
            count++;
        }

        return (count, column: n - p);
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

    private void WriteDirectiveDetails(AspxDirective node)
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

        if (node is HtmlTag { Attributes.IsRunAtServer: false })
        {
            return base.VisitChildren(node);
        }

        bool includeBaseCall = false;
        if (_componentsStack.Count == 0)
        {
            _writer.WriteLine("protected override void FrameworkInitialize()");
            includeBaseCall = true;
            _componentsStack.Push(new("Controls", "control"));
        }
        else
        {
            _componentsStack.Push(Current.GetNextLevel());
        }

        _writer.WriteLine("{");
        _writer.Indent++;

        if (includeBaseCall)
        {
            _writer.WriteLine("base.FrameworkInitialize();");
        }

        base.VisitChildren(node);

        _componentsStack.Pop();

        _writer.Indent--;
        _writer.WriteLine("}");

        return _tree;
    }

    public override object Visit(Literal node)
        => WriteLiteral(NormalizeLiteral(node.Text));

    public override object Visit(SelfClosingHtmlTag node)
    {
        VisitTag(node, true);
        return default;
    }

    private object WriteLiteral(string text)
    {
        var name = Current.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
        _writer.Write(text);
        _writer.WriteLine("\");");
        WriteControls(name);

        return _tree;
    }

    public override object Visit(CloseHtmlTag node)
        => default;

    public override object Visit(CloseAspxTag node)
        => default;

    public override object Visit(SelfClosingAspxTag tag)
    {
        VisitTag(tag);
        return base.Visit(tag);
    }

    public override object Visit(OpenAspxTag tag)
    {
        VisitTag(tag);
        return base.Visit(tag);
    }

    private void VisitTag(AspxTag tag)
    {
        // TODO: Handle prefix
        // node.Prefix
        var name = Current.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        _writer.Write(" = new global::System.Web.UI.WebControls.");
        _writer.Write(tag.ControlName);
        _writer.WriteLine("();");

        WriteId(tag.Attributes.Id, name, new("global::System.Web.UI.WebControls", tag.ControlName));

        WriteAttributes(tag, name);

        WriteControls(name);
    }

    private readonly struct QName
    {
        public QName(string ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public string Namespace { get; }

        public string Name { get; }
    }

    private void WriteId(string id, string name, QName qname)
    {
        if (!string.IsNullOrEmpty(id))
        {
            _writer.Write(name);
            _writer.Write(".ID = \"");
            _writer.Write(id);
            _writer.WriteLine("\";");

            _writer.Write(id);
            _writer.Write(" = ");
            _writer.Write(name);
            _writer.WriteLine(";");

            _variables.Add(new(id, qname));
        }
    }

    private void WriteAttributes(AspxTag tag, string name)
    {
        if (!_controls.TryGetValue(tag.ControlName, out var info))
        {
            _writer.Write("// Couldn't find info for ");
            _writer.Write(tag.Prefix);
            _writer.Write(':');
            _writer.WriteLine(tag.ControlName);
            return;
        }

        foreach (var attribute in tag.Attributes)
        {
            var (kind, normalized) = info.GetDataType(attribute.Key);

            if (kind == DataType.None)
            {
                _writer.Write(name);
                _writer.Write(".Attributes.Add(\"");
                _writer.Write(attribute.Key);
                _writer.Write("\", \"");
                _writer.Write(attribute.Value);
                _writer.WriteLine("\");");
            }
            else
            {
                _writer.Write(name);
                _writer.Write(".");

                _writer.Write(normalized);

                if (kind is DataType.Delegate)
                {
                    _writer.Write(" += ");
                }
                else
                {
                    _writer.Write(" = ");
                }

                WriteAttributeValue(kind, attribute.Value);

                _writer.WriteLine(";");
            }
        }
    }

    private void WriteAttributeValue(DataType kind, string value)
    {
        if (kind == DataType.String)
        {
            _writer.Write('\"');
            _writer.Write(value);
            _writer.Write('\"');
        }
        else
        {
            _writer.Write(value);
        }
    }

    private readonly Dictionary<string, string> _htmlControls = new()
    {
        { "form", "HtmlForm" },
    };

    public override object Visit(OpenHtmlTag tag)
    {
        if (VisitTag(tag, false))
        {
            base.Visit(tag);

            if (tag is { Attributes.IsRunAtServer: false })
            {
                WriteLiteral($"</{tag.Name}>");
            }
        }

        return default;
    }

    private readonly List<Variable> _variables = new();

    private readonly struct Variable
    {
        public Variable(string name, QName qname)
        {
            Name = name;
            Type = qname;
        }

        public string Name { get; }

        public QName Type { get; }
    }

    private readonly List<HtmlTag> _scripts = new();

    private bool VisitTag(HtmlTag tag, bool isClosing)
    {
        if (tag.Attributes.IsRunAtServer && string.Equals(tag.Name, "script", StringComparison.OrdinalIgnoreCase))
        {
            _scripts.Add(tag);
            return false;
        }

        var name = Current.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        if (tag.Attributes.IsRunAtServer)
        {
            QName type;

            if (_htmlControls.TryGetValue(tag.Name, out var known))
            {
                _writer.Write(" = new global::System.Web.UI.HtmlControls.");
                _writer.Write(known);
                _writer.WriteLine("();");

                type = new("global::System.Web.UI.HtmlControls", known);
            }
            else
            {
                type = new("global::System.Web.UI.HtmlControls", "HtmlGenericControl");
                _writer.Write(" = new global::System.Web.UI.HtmlControls.HtmlGenericControl(\"");
                _writer.Write(tag.Name);
                _writer.WriteLine("\");");
            }

            WriteId(tag.Attributes.Id, name, type);
        }
        else
        {
            _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
            _writer.Write("<");
            _writer.Write(tag.Name);
            _writer.Write(isClosing ? " />" : ">");
            _writer.WriteLine("\");");
        }

        WriteControls(name);

        return true;
    }

    private void WriteControls(string name)
    {
        _writer.Write(Current.Controls);
        _writer.Write(".Add(");
        _writer.Write(name);
        _writer.WriteLine(");");
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
