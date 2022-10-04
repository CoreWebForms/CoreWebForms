// MIT License.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;

internal class SymbolCreator : DepthFirstAspxVisitor<Control?>
{
    private readonly IControlLookup _webControlLookup;

    private AspxNode.AspxDirective? _directive;

    private readonly List<string> _errors = new();
    private readonly List<Script> _scripts = new();
    private readonly List<Template> _content = new();
    private readonly List<string> _contentPlaceHolders = new();

    public static ParsedPage ParsePage(string path, string contents, IControlLookup controls)
    {
        var parser = new AspxParser();
        var source = new AspxSource(path, contents);
        var tree = parser.Parse(source);

        var visitor = new SymbolCreator(controls);

        var result = tree.RootNode.Accept(visitor);

        var page = new ParsedPage
        {
            Directive = new(visitor._directive),
            Root = result,
            Errors = visitor._errors.ToImmutableArray(),
            Scripts = visitor._scripts.ToImmutableArray(),
            Templates = visitor._content.ToImmutableArray(),
            ContentPlaceholders = visitor._contentPlaceHolders.ToImmutableArray()
        };

        if (page.Directive.MasterPageFile is { } masterFile)
        {
            page = page with { AdditionalFiles = ImmutableArray.Create(new PagePath(masterFile).Path) };
        }

        return page with
        {
            File = new(path)
        };
    }

    private SymbolCreator(IControlLookup webcontrols)
    {
        _webControlLookup = webcontrols;
    }

    public override Control? Visit(AspxNode.Root node)
    {
        var builder = new LiteralCombiningBuilder();

        foreach (var child in node.Children)
        {
            builder.Add(child.Accept(this));
        }

        return new Root
        {
            Children = builder.Build(),
        };
    }

    public override Control? Visit(AspxNode.AspxDirective node)
    {
        _directive = node;
        return null;
    }

    public override Control? Visit(AspxNode.CloseAspxTag node)
    {
        return base.Visit(node);
    }

    public override Control? Visit(AspxNode.CloseHtmlTag node)
    {
        return base.Visit(node);
    }

    public override Control? Visit(AspxNode.CodeRender node)
    {
        return base.Visit(node);
    }

    public override Control? Visit(AspxNode.CodeRenderEncode node)
        => new CodeControl(node.Expression, Convert(node.Location));

    public override Control? Visit(AspxNode.CodeRenderExpression node)
    {
        return base.Visit(node);
    }

    public override Control? Visit(AspxNode.DataBinding node)
    {
        return base.Visit(node);
    }

    public override Control? Visit(AspxNode.Literal node)
        => new LiteralControl(node.Text, Convert(node.Location));

    public override Control? Visit(AspxNode.OpenAspxTag node)
        => Visit((AspxNode.AspxTag)node);

    private Control? Visit(AspxNode.AspxTag aspxTag)
    {
        if (string.Equals(aspxTag.ControlName, "Content", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new LiteralCombiningBuilder();

            foreach (var child in aspxTag.Children)
            {
                builder.Add(child.Accept(this));
            }

            _content.Add(new Template(aspxTag.Attributes.Id, aspxTag.Attributes["ContentPlaceHolderID"], builder.Build()));

            return null;
        }
        else if (string.Equals(aspxTag.ControlName, "ContentPlaceHolder", StringComparison.OrdinalIgnoreCase) && aspxTag.Attributes.Id is { } id)
        {
            _contentPlaceHolders.Add(id);
            return new TypedControl(new("System.Web.UI.WebControls", "ContentPlaceHolder"), Convert(aspxTag.Location)) { Id = id };
        }
        else if (_webControlLookup.TryGetControl(aspxTag.Prefix, aspxTag.ControlName, out var known))
        {
            var builder = new LiteralCombiningBuilder();
            var properties = ImmutableArray.CreateBuilder<Property>();

            var asProperties = known.ChildrenAsProperties;

            foreach (var child in aspxTag.Children)
            {
                if (asProperties && child is AspxNode.HtmlTag html && VisitChildren(html.Children, removeLiterals: true) is { } propertyChildren && known.GetDataType(html.Name) is { } type)
                {
                    properties.Add(new Property(html.Name, propertyChildren, type.Item1));
                }
                else
                {
                    builder.Add(child.Accept(this));
                }
            }

            return new TypedControl(known.QName, Convert(aspxTag.Location))
            {
                Attributes = BuildAttributes(aspxTag.Attributes, known),
                Id = aspxTag.Attributes.Id,
                Properties = properties.ToImmutable(),
                Children = builder.Build(),
            };
        }
        else
        {
            _errors.Add($"Unknown web control: {aspxTag.Prefix}:{aspxTag.ControlName}");
            return null;
        }
    }

    public override Control? Visit(AspxNode.OpenHtmlTag node)
        => Visit((AspxNode.HtmlTag)node);

    public override Control? Visit(AspxNode.SelfClosingAspxTag node)
        => Visit((AspxNode.AspxTag)node);

    public override Control? Visit(AspxNode.SelfClosingHtmlTag node)
    {
        if (node.Attributes.IsRunAtServer)
        {
            return Visit((AspxNode.HtmlTag)node);
        }
        else
        {
            return new LiteralControl(node.GetOriginalText(), Convert(node.Location));
        }
    }

    private Control? Visit(AspxNode.HtmlTag htmlTag)
    {
        if (htmlTag.Attributes.IsRunAtServer)
        {
            if (string.Equals(htmlTag.Name, "script", StringComparison.OrdinalIgnoreCase))
            {
                var builder = ImmutableArray.CreateBuilder<TextLine>();

                foreach (var child in htmlTag.Children)
                {
                    var literal = (AspxNode.Literal)child;
                    builder.Add(new(literal.Text, Convert(literal.Location)));
                }

                _scripts.Add(new(builder.ToImmutable()));
            }
            else
            {
                var known = HtmlTagNameToTypeMapper.Instance.GetControlType(htmlTag.Name, htmlTag.Attributes);
                var builder = new LiteralCombiningBuilder();

                foreach (var child in htmlTag.Children)
                {
                    builder.Add(child.Accept(this));
                }

                return new TypedControl(known, Convert(htmlTag.Location))
                {
                    Attributes = BuildAttributes(htmlTag.Attributes, null),
                    Id = htmlTag.Attributes.Id,
                    Children = builder.Build(),
                };
            }
        }
        else
        {
            var builder = new LiteralCombiningBuilder();

            builder.Add(new LiteralControl(htmlTag.GetOriginalText(), Convert(htmlTag.Location)));

            foreach (var child in htmlTag.Children)
            {
                builder.Add(child.Accept(this));
            }

            builder.Add(new LiteralControl($"</{htmlTag.Name}>", default));

            return new Root()
            {
                Children = builder.Build(),
            };
        }

        return null;
    }

    private ImmutableArray<Attribute> BuildAttributes(TagAttributes attributes, ControlInfo? control)
    {
        var builder = ImmutableArray.CreateBuilder<Attribute>();

        foreach (var attribute in attributes)
        {
            if (control is not null)
            {
                var (kind, key) = control.GetDataType(attribute.Key);
                builder.Add(new(key, attribute.Value, kind));
            }
            else
            {
                builder.Add(new(attribute.Key, attribute.Value, DataType.None));
            }
        }

        return builder.ToImmutable();
    }

    private Control? VisitChildren(List<AspxNode> children, bool removeLiterals)
    {
        var builder = new LiteralCombiningBuilder(removeLiterals);

        foreach (var child in children)
        {
            builder.Add(child.Accept(this));
        }

        return new Root() { Children = builder.Build() };
    }

    protected override Control? VisitChildren(AspxNode node)
    {
        var result = base.VisitChildren(node);

        if (result is null)
        {
            return null;
        }

        var builder = new LiteralCombiningBuilder();

        foreach (var child in result.Children)
        {
            builder.Add(child);
        }

        return result with { Children = builder.Build() };
    }

    private Location Convert(Syntax.Location location)
    {
        return new(location.Source.Name, GetSpan(location.Start, location.Source.Text), GetSpan(location.End, location.Source.Text));

        static Position GetSpan(int offset, string line)
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

            return new(count, n - p);
        }
    }

    private class LiteralCombiningBuilder
    {
        private readonly ImmutableArray<Control>.Builder _builder;
        private readonly bool _removeLiterals;

        public LiteralCombiningBuilder(bool removeLiterals = false)
        {
            _builder = ImmutableArray.CreateBuilder<Control>();
            _removeLiterals = removeLiterals;
        }

        public ImmutableArray<Control> Build()
        {
            var result = _builder.ToImmutable();

            return result.IsDefault ? ImmutableArray<Control>.Empty : result;
        }

        public void Add(Control? control)
        {
            if (control is LiteralControl && _removeLiterals)
            {
            }
            else if (control is null)
            {
                return;
            }
            else if (control is Root root)
            {
                foreach (var child in root.Children)
                {
                    Add(child);
                }
            }
            else if (_builder.Count == 0)
            {
                _builder.Add(control);
            }
            else if (control is LiteralControl newLiteral && _builder[_builder.Count - 1] is LiteralControl existing)
            {
                _builder[_builder.Count - 1] = existing.Combine(newLiteral);
            }
            else
            {
                _builder.Add(control);
            }
        }
    }
}

