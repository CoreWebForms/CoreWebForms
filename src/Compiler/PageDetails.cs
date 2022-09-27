// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

using static Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax.AspxNode;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public class PageDetails
{
    private readonly List<CodeRenderEncode> _encodes = new();
    private readonly List<string> _contentPlaceHolders = new();
    private readonly List<AspxNode> _scripts = new();
    private readonly List<AspxTag> _templates = new();

    public AspxDirective Directive { get; private set; }

    public IReadOnlyCollection<AspxNode> Nodes { get; private set; }

    public IEnumerable<CodeRenderEncode> CodeSnippets => _encodes;

    public PagePath File { get; private set; }

    public IEnumerable<string> ContentPlaceHolders => _contentPlaceHolders;

    public IEnumerable<AspxNode> Scripts => _scripts;

    public Dictionary<string, ControlInfo> Controls { get; private set; }

    public PagePath? MasterPage
    {
        get
        {
            if (Directive is { } directive)
            {
                var info = new DirectiveDetails(directive);

                if (info.MasterPageFile is { } path)
                {
                    return new(path);
                }
            }

            return default;
        }
    }

    public IEnumerable<AspxTag> Templates => _templates;

    public IEnumerable<string> AdditionalFiles => MasterPage is { } page ? new[] { page.Path } : Array.Empty<string>();

    public ImmutableArray<AspxParseError> Errors { get; private set; }

    public static PageDetails Build(string path, string contents, IEnumerable<ControlInfo> controlInfo)
    {
        var controls = controlInfo.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

        var parser = new AspxParser();
        var source = new AspxSource(path, contents);
        var tree = parser.Parse(source);

        var details = new PageDetails
        {
            Errors = tree.ParseErrors,
            File = new(path),
            Controls = controls,
        };

        if (!details.Errors.IsDefaultOrEmpty)
        {
            return details;
        }

        var visitor = new NodeVisitor(details);

        details.Nodes = visitor.Visit(tree.RootNode).ToList();

        return details;
    }

    private class NodeVisitor
    {
        private readonly PageDetails _details;

        public NodeVisitor(PageDetails details)
        {
            _details = details;
        }

        public T VisitChildren<T>(T node)
             where T : AspxNode
        {
            if (node.Children.Count == 0)
            {
                return node;
            }

            var children = new List<AspxNode>();
            Literal previous = null;

            // Combine any Literal nodes that are in succession to each other
            foreach (var child in node.Children)
            {
                foreach (var visited in Visit(child))
                {
                    if (visited is Literal literal)
                    {
                        if (previous is null)
                        {
                            previous = literal;
                        }
                        else
                        {
                            var newLocation = new Location(previous.Location.Source, previous.Location.Start, previous.Location.End + literal.Location.Length);
                            previous = new Literal(previous.Text + literal.Text, newLocation);
                        }
                    }
                    else
                    {
                        if (previous is not null)
                        {
                            children.Add(previous);
                            previous = null;
                        }

                        children.Add(visited);
                    }
                }
            }

            if (previous is not null)
            {
                children.Add(previous);
            }

            node.Children = children;
            return node;
        }

        public IEnumerable<AspxNode> Visit(Root root)
        {
            foreach (var child in VisitChildren(root).Children)
            {
                yield return child;
            }
        }

        public IEnumerable<AspxNode> Visit(AspxDirective directive)
        {
            _details.Directive = directive;
            return Enumerable.Empty<AspxNode>();
        }

        public IEnumerable<AspxNode> Visit(CodeRenderEncode encode)
        {
            _details._encodes.Add(encode);

            return Enumerable.Empty<AspxNode>();
        }

        public IEnumerable<AspxNode> Visit(Literal literal)
            => new[] { literal };

        public IEnumerable<AspxNode> Visit(CloseAspxTag closeTag)
            => Enumerable.Empty<AspxNode>();

        public IEnumerable<AspxNode> Visit(CloseHtmlTag htmlTag)
        {
            // Can't tell if it is runat=server
            //yield return new Literal($"</{closeHtml.Name}>", closeHtml.Location);
            return Enumerable.Empty<AspxNode>();
        }

        public IEnumerable<AspxNode> Visit(AspxTag aspx)
        {
            if (string.Equals(aspx.ControlName, "Content", StringComparison.OrdinalIgnoreCase))
            {
                _details._templates.Add(VisitChildren(aspx));

                yield break;
            }

            if (string.Equals(aspx.ControlName, "ContentPlaceHolder", StringComparison.OrdinalIgnoreCase) && aspx.Attributes.Id is { } id)
            {
                _details._contentPlaceHolders.Add(id);
            }

            yield return VisitChildren(aspx);
        }

        public IEnumerable<AspxNode> Visit(HtmlTag html)
        {
            if (!html.Attributes.IsRunAtServer)
            {
                if (html is SelfClosingHtmlTag)
                {
                    yield return new Literal($"<{html.Name} />", html.Location);
                }
                else
                {
                    yield return new Literal($"<{html.Name}>", html.Location);

                    foreach (var visited in VisitChildren(html).Children)
                    {
                        yield return visited;
                    }

                    yield return new Literal($"</{html.Name}>", new Location());
                }
            }
            else if (html.Attributes.IsRunAtServer && string.Equals(html.Name, "script", StringComparison.OrdinalIgnoreCase))
            {
                _details._scripts.Add(html);
                yield break;
            }
            else
            {
                yield return VisitChildren(html);
            }
        }

        public IEnumerable<AspxNode> Visit(AspxNode node) => node switch
        {
            Root root => Visit(root),
            AspxDirective directive => Visit(directive),
            CodeRenderEncode encode => Visit(encode),
            Literal literal => Visit(literal),
            CloseAspxTag closeAspx => Visit(closeAspx),
            CloseHtmlTag closeHtml => Visit(closeHtml),
            AspxTag aspx => Visit(aspx),
            HtmlTag html => Visit(html),
            _ => throw new NotImplementedException($"Unknown node '{node.GetType()}'")
        };
    }
}
