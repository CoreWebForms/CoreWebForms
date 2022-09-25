// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

using static Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax.AspxNode;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public readonly struct PagePath
{
    public PagePath(string path, string className)
    {
        Path = path;
        ClassName = className;
    }

    public string Path { get; }

    public string ClassName { get; }
}

public class PageDetails
{
    private readonly List<CodeRenderEncode> _encodes = new();
    private readonly List<string> _contentPlaceHolders = new();
    private readonly List<AspxNode> _scripts = new();
    private readonly List<AspxTag> _templates = new();

    public AspxDirective Directive { get; private set; }

    public IReadOnlyCollection<AspxNode> Nodes { get; private set; }

    public IEnumerable<CodeRenderEncode> CodeSnippets => _encodes;

    public string Path { get; private set; }

    public string ClassName { get; private set; }

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
                    return new(NormalizePath(path), ConvertPathToClassName(path));
                }
            }

            return default;
        }
    }

    public IEnumerable<AspxTag> Templates => _templates;

    public IEnumerable<string> AdditionalFiles => MasterPage is { } page ? new[] { NormalizePath(page.Path) } : Array.Empty<string>();

    public ImmutableArray<AspxParseError> Errors { get; private set; }

    public static PageDetails Build(string path, string contents, IEnumerable<ControlInfo> controlInfo)
    {
        var controls = controlInfo.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);
        var Path = NormalizePath(path);
        var ClassName = ConvertPathToClassName(path);

        var parser = new AspxParser();
        var source = new AspxSource(Path, contents);
        var tree = parser.Parse(source);

        var details = new PageDetails
        {
            Errors = tree.ParseErrors,
            Path = Path,
            ClassName = ClassName,
            Controls = controls,
        };

        if (!details.Errors.IsDefaultOrEmpty)
        {
            return details;
        }

        details.Nodes = Visit(tree.RootNode).ToList();

        return details;

        T VisitChildren<T>(T node)
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

        IEnumerable<AspxNode> Visit(AspxNode node)
        {
            if (node is Root root)
            {
                foreach (var child in VisitChildren(root).Children)
                {
                    yield return child;
                }
            }
            else if (node is AspxDirective directive)
            {
                details.Directive = directive;
                yield break;
            }
            else if (node is CodeRenderEncode encode)
            {
                details._encodes.Add(encode);
                yield return node;
            }
            else if (node is Literal literal)
            {
                yield return literal;
            }
            else if (node is CloseAspxTag closeAspx)
            {
                yield break;
            }
            else if (node is CloseHtmlTag closeHtml)
            {
                yield break;
                // Can't tell if it is runat=server
                //yield return new Literal($"</{closeHtml.Name}>", closeHtml.Location);
            }
            else if (node is AspxTag aspx)
            {
                if (string.Equals(aspx.ControlName, "Content", StringComparison.OrdinalIgnoreCase))
                {
                    details._templates.Add(VisitChildren(aspx));

                    yield break;
                }

                if (string.Equals(aspx.ControlName, "ContentPlaceHolder", StringComparison.OrdinalIgnoreCase) && aspx.Attributes.Id is { } id)
                {
                    details._contentPlaceHolders.Add(id);
                }

                yield return VisitChildren(aspx);
            }
            else if (node is HtmlTag html)
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

                        foreach (var visited in VisitChildren(node).Children)
                        {
                            yield return visited;
                        }

                        yield return new Literal($"</{html.Name}>", new Location());
                    }
                }
                else if (html.Attributes.IsRunAtServer && string.Equals(html.Name, "script", StringComparison.OrdinalIgnoreCase))
                {
                    details._scripts.Add(html);
                    yield break;
                }
                else
                {
                    yield return VisitChildren(html);
                }
            }
            else
            {
                throw new NotImplementedException($"Unknown node '{node.GetType()}'");
            }
        }
    }

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

    private static string NormalizePath(string path)
    {
        var sb = new StringBuilder(path);

        if (sb[0] != '/')
        {
            sb.Insert(0, '/');
        }


        sb.Replace("~", string.Empty);
        sb.Replace("\\", "/");
        sb.Replace("//", "/");

        return sb.ToString();
    }

    private static string ConvertPathToClassName(string input)
    {
        var sb = new StringBuilder(input);

        sb.Replace("~", string.Empty);
        sb.Replace(".", "_");
        sb.Replace("/", "_");
        sb.Replace("\\", "_");

        return sb.ToString();
    }
}

internal readonly struct QName
{
    public QName(string ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public string Namespace { get; }

    public string Name { get; }
}

internal readonly struct Item
{
    public Item(QName qname, TagAttributes attributes)
    {
        QName = qname;
        Attributes = attributes;
    }

    public QName QName { get; }

    public TagAttributes Attributes { get; }
}

internal readonly struct Variable
{
    public Variable(string name, QName qname)
    {
        Name = name;
        Type = qname;
    }

    public string Name { get; }

    public QName Type { get; }
}
