// MIT License.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;

using Location = Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols.Location;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public class CSharpPageWriter
{
    private readonly string[] DefaultUsings = new[]
    {
        "System",
        "System.Web",
        "System.Web.UI",
    };

    private readonly IndentedTextWriter _writer;
    private readonly ParsedPage _details;
    private readonly IndentClose _blockClose;
    private readonly List<Variable> _variables = new();
    private bool _needCodeRender;
    private bool _needTemplateContainer;

    public CSharpPageWriter(IndentedTextWriter writer, ParsedPage details)
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
            WriteTemplateContainer();

            // Must be last for now as we populate the list while writing - this should be moved to PageDetails
            WriteVariables();
        }
    }

    private void WriteClassDeclaration()
    {
        var info = _details.Directive;

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
            foreach (var node in _details.Root.Children)
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
                foreach (var node in template.Controls)
                {
                    WriteTag(node, level);
                }
            }
        }
    }

    private void WriteTag(Control tag, ComponentLevel level)
    {
        if (tag is LiteralControl literal)
        {
            WriteLiteral(literal.Text, level);
        }
        else if (tag is TypedControl control)
        {
            Write(control, level);
            WriteChildren(tag, level);
        }
        else if (tag is CodeControl code)
        {
            WriteCode(code, level);
        }
        else if (tag is Root root)
        {
            foreach (var child in root.Children)
            {
                WriteTag(child, level);
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void WriteProperties(string name, ImmutableArray<Property> properties)
    {
        if (properties.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var property in properties)
        {
            if (property.Type == DataType.Template)
            {
                _needTemplateContainer = true;

                _writer.Write(name);
                _writer.Write('.');
                _writer.Write(property.Name);
                _writer.WriteLine(" = (ITemplate)new DelegateTemplate(parent =>");
                _writer.WriteLine("{");
                _writer.Indent++;
                WriteChildren(property.Control, new("parent.Controls", "template"));
                _writer.Indent--;
                _writer.WriteLine("});");
            }
            else if (property.Type == DataType.Collection)
            {
                _writer.WriteLine("{");
                _writer.Indent++;
                WriteChildren(property.Control, new($"{name}.{property.Name}", $"{name}_collection"));
                _writer.Indent--;
                _writer.WriteLine("};");
            }
            else
            {
            }
        }
    }

    private void WriteChildren(Control tag, ComponentLevel level)
    {
        if (tag.Children.Length > 0)
        {
            if (tag is Root)
            {
                foreach (var child in tag.Children)
                {
                    WriteTag(child, level);
                }
            }
            else
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
    }

    private void WriteCode(CodeControl encode, ComponentLevel level)
    {
        _needCodeRender = true;

        var name = level.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);
        _writer.Write(" = new CodeRender(() => ");
        _writer.Write(encode.Expression);
        _writer.WriteLine(");");

        WriteControls(name, level);
    }

    private void Write(TypedControl control, ComponentLevel level)
    {
        var name = level.GetNextControlName();

        _writer.Write("var ");
        _writer.Write(name);

        _writer.Write(" = new ");
        _writer.Write(control.Type.Namespace);
        _writer.Write('.');
        _writer.Write(control.Type.Name);
        _writer.WriteLine("();");

        WriteId(control.Id, name, control.Type);
        WriteAttributes(control, name);
        WriteProperties(name, control.Properties);

        WriteControls(name, level);
    }

    private void WriteAttributes(TypedControl control, string name)
    {
        foreach (var attribute in control.Attributes)
        {
            if (attribute.Kind == DataType.None)
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

                _writer.Write(attribute.Key);

                if (attribute.Kind is DataType.Delegate)
                {
                    _writer.Write(" += ");
                }
                else
                {
                    _writer.Write(" = ");
                }

                WriteAttributeValue(attribute.Kind, attribute.Value);

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
        _writer.Write(" = new global::System.Web.UI.LiteralControl(");
        WriteString(text);
        _writer.WriteLine(");");

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
            foreach (var placeholder in _details.ContentPlaceholders)
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
            foreach (var child in script.Lines)
            {
                WriteLineInfo(child.Location);
                _writer.WriteLine(child.Text.Trim());
                _writer.WriteLine("#line default");
            }
        }
    }

    private void WriteCodeSnippets()
    {
        if (_needCodeRender)
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

    private void WriteTemplateContainer()
    {
        if (_needTemplateContainer)
        {
            const string CodeRender = @"private sealed class DelegateTemplate : global::System.Web.UI.ITemplate
    {
        private readonly Action<Control> _template;

        public DelegateTemplate(Action<Control> template)
        {
            _template = template;
        }

        void global::System.Web.UI.ITemplate.InstantiateIn(Control container) => _template(container);
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

    private bool HasInitializer => !_details.ContentPlaceholders.IsDefaultOrEmpty;

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
                _writer.Write("AddContentTemplate(");
                WriteString(template.PlaceholderId);
                _writer.WriteLine(", this);");
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
        _writer.Write(NormalizeLiteral(str));
        _writer.Write('\"');

        static string NormalizeLiteral(string input)
            => input
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\"", "\\\"");
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
        var start = location.Start;
        var end = location.End;
        _writer.Write(start.Line);
        _writer.Write(", ");
        _writer.Write(start.Column);
        _writer.Write(") - (");
        _writer.Write(end.Line);
        _writer.Write(", ");
        _writer.Write(end.Column);
        _writer.Write(") \"");
        _writer.Write(_details.File.Path.Trim('/'));
        _writer.WriteLine('\"');
    }

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

