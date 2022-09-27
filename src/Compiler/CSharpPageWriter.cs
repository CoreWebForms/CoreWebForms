// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public class CSharpPageWriter
{

    private readonly Dictionary<string, string> _htmlControls = new()
    {
        { "form", "HtmlForm" },
        { "head", "HtmlHead" },
    };

    private readonly string[] DefaultUsings = new[]
    {
        "System",
        "System.Web",
        "System.Web.UI",
    };

    private readonly IndentedTextWriter _writer;
    private readonly PageDetails _details;
    private readonly IndentClose _blockClose;
    private readonly List<Variable> _variables = new();

    public CSharpPageWriter(IndentedTextWriter writer, PageDetails details)
    {
        _writer = writer;
        _details = details;
        _blockClose = new IndentClose(writer, includeBrace: true);
    }

    public void Write()
    {
        foreach (var u in DefaultUsings)
        {
            _writer.Write("using ");
            _writer.Write(u);
            _writer.WriteLine(';');
        }

        WriteClassDeclaration();

        using (Block())
        {
            WriteConstructor();
            WriteFrameworkInitialize();
            WriteBuildControlTree();
            WriteBuildControlTemplate();
            WriteTemplate();
            WriteCreateMasterPage();
            WriteInitializer();
            WriteScripts();
            WriteCodeSnippets();

            // Must be last for now as we populate the list while writing - this should be moved to PageDetails
            WriteVariables();
        }
    }

    private void WriteClassDeclaration()
    {
        var info = new DirectiveDetails(_details.Directive);

        _writer.Write("[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(\"");
        _writer.Write(_details.File.Path);
        _writer.WriteLine("\")]");
        _writer.Write("internal partial class ");
        _writer.Write(_details.File.ClassName);
        _writer.Write(" : ");
        _writer.Write(info.Inherits);

        if (_details.Templates.Any())
        {
            _writer.Write(", global::System.Web.UI.ITemplate");
        }

        _writer.WriteLine();
    }

    private void WriteBuildControlTree()
    {
        _writer.WriteLine("private void BuildControlTree(Control control)");

        using (Block())
        {
            var level = new ComponentLevel("control.Controls", "control");
            foreach (var node in _details.Nodes)
            {
                WriteTag(node, level);
            }
        }
    }

    private void WriteBuildControlTemplate()
    {
        if (_details.Templates.FirstOrDefault() is { } template)
        {
            _writer.WriteLine("private void BuildControlTemplate(Control control)");

            using (Block())
            {
                var level = new ComponentLevel("control.Controls", "control");
                foreach (var node in template.Children)
                {
                    WriteTag(node, level);
                }
            }
        }
    }

    private void WriteTag(AspxNode tag, ComponentLevel level)
    {
        if (tag is AspxNode.Literal literal)
        {
            WriteLiteral(literal.Text, level);
        }
        else if (tag is AspxNode.AspxTag aspx)
        {
            Write(aspx.Prefix, aspx.ControlName, "global::System.Web.UI.WebControls", aspx.Attributes, level);
        }
        else if (tag is AspxNode.HtmlTag html)
        {
            Write(string.Empty, html.Name, "System.Web.UI.HtmlControls", html.Attributes, level);
        }
        else if (tag is AspxNode.CodeRenderEncode encode)
        {
            WriteCodeSnippets(encode, level);
        }

        if (tag.Children.Count > 0)
        {
            using (Block())
            {
                var next = level.GetNextLevel();
                foreach (var child in tag.Children)
                {
                    WriteTag(child, next);
                }
            }
        }
    }

    private void WriteCodeSnippets(AspxNode.CodeRenderEncode encode, ComponentLevel level)
    {
        var name = level.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new CodeRender(() => ");
        _writer.Write(encode.Expression);
        _writer.WriteLine(");");

        WriteControls(name, level);
    }

    private void Write(string prefix, string tagName, string ns, TagAttributes attributes, ComponentLevel level)
    {
        var name = level.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        Debug.Assert(attributes.IsRunAtServer);

        QName type;

        if (_htmlControls.TryGetValue(tagName, out var known))
        {
            tagName = known;
        }

        _writer.Write(" = new ");
        _writer.Write(ns);
        _writer.Write('.');
        _writer.Write(tagName);
        _writer.WriteLine("();");

        type = new(ns, tagName);

        WriteId(attributes.Id, name, type);
        WriteAttributes(prefix, tagName, attributes, name);

        WriteControls(name, level);
    }

    private void WriteAttributes(string prefix, string tagName, TagAttributes attributes, string name)
    {
        if (!_details.Controls.TryGetValue(tagName, out var info))
        {
            _writer.Write("// Couldn't find info for ");

            if (!string.IsNullOrEmpty(prefix))
            {
                _writer.Write(prefix);
                _writer.Write(':');
            }
            _writer.WriteLine(tagName);
            return;
        }

        foreach (var attribute in attributes)
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

    private void WriteLiteral(string text, ComponentLevel level)
    {
        var name = level.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new global::System.Web.UI.LiteralControl(\"");
        _writer.Write(NormalizeLiteral(text));
        _writer.WriteLine("\");");

        WriteControls(name, level);
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

    private void WriteControls(string name, ComponentLevel level)
    {
        _writer.Write(level.Controls);
        _writer.Write(".Add(");
        _writer.Write(name);
        _writer.WriteLine(");");
    }

    private void WriteInitializer()
    {
        if (!HasInitializer)
        {
            return;
        }

        _writer.WriteLine("private void Initialize()");

        using (Block())
        {
            foreach (var placeholder in _details.ContentPlaceHolders)
            {
                _writer.Write("ContentPlaceHolders.Add(");
                WriteString(placeholder.ToLower(CultureInfo.InvariantCulture));
                _writer.WriteLine(");");
            }
        }
    }

    private void WriteScripts()
    {
        if (!_details.Scripts.Any())
        {
            return;
        }

        foreach (var script in _details.Scripts)
        {
            foreach (var child in script.Children)
            {
                if (child is AspxNode.Literal literal)
                {
                    WriteLineInfo(literal.Location);
                    _writer.WriteLine(literal.Text.Trim());
                    _writer.WriteLine("#line default");
                }
            }
        }
    }

    private void WriteCodeSnippets()
    {
        if (_details.CodeSnippets.Any())
        {
            const string CodeRender = @"private sealed class CodeRender : global::System.Web.UI.Control
    {
        private readonly Func<object> _factory;

        public CodeRender(Func<object> factory)
        {
            _factory = factory;
        }

        public override void RenderControl(global::System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(global::System.Web.HttpUtility.HtmlEncode((object?)_factory()));

            base.RenderControl(writer);
        }
    }";

            _writer.WriteLine(CodeRender);
        }
    }

    private void WriteTemplate()
    {
        if (_details.Templates.Any())
        {
            _writer.WriteLine("void global::System.Web.UI.ITemplate.InstantiateIn(Control container)");
            using (Block())
            {
                _writer.WriteLine("BuildControlTemplate(container);");
            }
        }
    }

    private bool HasInitializer => _details.ContentPlaceHolders.Any();

    private void WriteConstructor()
    {
        if (HasInitializer)
        {
            _writer.Write("public ");
            _writer.Write(_details.File.ClassName);
            _writer.WriteLine("()");

            using (Block())
            {
                _writer.WriteLine("Initialize();");
            }
        }
    }

    private void WriteFrameworkInitialize()
    {
        _writer.WriteLine("protected override void FrameworkInitialize()");

        using (Block())
        {
            _writer.WriteLine("base.FrameworkInitialize();");

            foreach (var template in _details.Templates)
            {
                if (template.Attributes.TryGetValue("ContentPlaceHolderID", out var id))
                {
                    _writer.Write("AddContentTemplate(");
                    WriteString(id);
                    _writer.WriteLine(", this);");
                }
            }

            _writer.WriteLine("BuildControlTree(this);");
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

    private void WriteCreateMasterPage()
    {
        if (!_details.MasterPage.HasValue)
        {
            return;
        }

        _writer.Write("protected override global::System.Web.UI.MasterPage CreateMasterPage() => new ");
        _writer.Write(_details.MasterPage.Value.ClassName);
        _writer.WriteLine("();");
    }

    private void WriteString(string str)
    {
        _writer.Write('\"');
        _writer.Write(str);
        _writer.Write('\"');
    }

    private IDisposable Block()
    {
        _writer.WriteLine("{");
        _writer.Indent++;
        return _blockClose;
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
        _writer.Write(_details.File.Path.Trim('/'));
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

    private string NormalizeLiteral(string input)
        => input
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");

    private class ComponentLevel
    {
        private int _count = 0;
        private string _currentLevel;

        public ComponentLevel(string controls, string prefix)
        {
            Prefix = prefix;
            Controls = controls;
        }

        public string CurrentLevel => _currentLevel ??= GetCurrentLevel(_count);

        private string GetCurrentLevel(int count) => $"{Prefix}_{count}";

        public string GetNextControlName() => _currentLevel = GetCurrentLevel(++_count);

        public ComponentLevel GetNextLevel() => new($"{CurrentLevel}.Controls", CurrentLevel);

        public string Controls { get; }

        private string Prefix { get; }
    }
}

