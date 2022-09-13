// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using AspxParser;
using Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser;

public sealed class AspxParser : IParserEventListener
{
    private readonly HashSet<string> currentFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly ImmutableArray<AspxParseError>.Builder errors = ImmutableArray.CreateBuilder<AspxParseError>();
    private readonly string rootDirectory;
    private readonly bool isFw40;
    private AspxNode currentNode;

    /// <param name="rootDirectory">A directory to a parsed document</param>
    /// <param name="isFw40">If true use .NET 4 regex, otherwise use .NET 3.5 regex</param>
    public AspxParser(string rootDirectory, bool isFw40 = true)
    {
        this.rootDirectory = rootDirectory;
        this.isFw40 = isFw40;
    }

    public Func<string, IAspxSource> LoadSourceHandler { get; set; }

    public AspxParseResult Parse(IAspxSource source)
    {
        currentFiles.Clear();
        errors.Clear();
        var rootNode = new AspxNode.Root();
        Parse(source, rootNode);
        return new AspxParseResult(rootNode, errors.ToImmutable());
    }

    public bool TryParseDirective(IAspxSource source, out AspxNode.AspxDirective directive)
    {
        currentFiles.Clear();
        errors.Clear();
        var parser = new Parser(this, isFw40, source);
        var result = parser.TryParseDirective();
        if (result != null)
        {
            directive = new AspxNode.AspxDirective(result.Item2, result.Item3, result.Item1);
            return true;
        }
        directive = null;
        return false;
    }

    private void Parse(IAspxSource source, AspxNode root)
    {
        if (currentFiles.Add(source.Name))
        {
            var previousCurrentNode = currentNode;
            currentNode = root;

            var parser = new Parser(this, isFw40, source);
            parser.Parse();

            currentNode = previousCurrentNode;
            currentFiles.Remove(source.Name);
        }
        else
        {
            AddError(new Location(source, 0, 0), "Circular file include.");
        }
    }

    private void AddError(Location location, string message) =>
        errors.Add(new AspxParseError(location, message));

    void IParserEventListener.OnError(Location location, string message) =>
        AddError(location, message);

    void IParserEventListener.OnComment(Location location, string comment) { }

    void IParserEventListener.OnLiteral(Location location, string text) =>
        currentNode.AddChild(new AspxNode.Literal(text, location));

    void IParserEventListener.OnDirective(Location location, string name, TagAttributes attributes) =>
        currentNode.AddChild(new AspxNode.AspxDirective(name.ToUpperInvariant(), attributes, location));

    void IParserEventListener.OnInclude(Location location, IncludePathType pathType, string path)
    {
        var normalizedPath = path.TrimStart(Path.DirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);

        string resolvedPath;
        switch (pathType)
        {
            case IncludePathType.Absolute:
                var currentFileDirectory = Path.GetDirectoryName(location.Source.Name) ?? rootDirectory;
                resolvedPath = Path.GetFullPath(Path.Combine(currentFileDirectory, normalizedPath));
                break;

            case IncludePathType.Virtual:
                resolvedPath = Path.GetFullPath(Path.Combine(rootDirectory, normalizedPath));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null);
        }

        try
        {
            IAspxSource newSource;

            var handler = LoadSourceHandler;
            if (handler != null)
            {
                newSource = handler(resolvedPath);
            }
            else
            {
                var normResolvedPath = resolvedPath.NormalizeFilePath();
                newSource = File.Exists(normResolvedPath)
                    ? new AspxSource(resolvedPath, File.ReadAllText(normResolvedPath))
                    : null;
            }

            if (newSource != null)
            {
                Parse(newSource, currentNode);
            }
            else
            {
                AddError(location, $"File `{resolvedPath}` does not exist.");
            }
        }
        catch (Exception ex) when (!(ex is ThreadAbortException))
        {
            AddError(location, $"Exception occured while parsing included file `{resolvedPath}`: {ex}");
        }
    }

    void IParserEventListener.OnCodeBlock(Location location, CodeBlockType blockType, string code, bool isEncode)
    {
        switch (blockType)
        {
            case CodeBlockType.Code:
                currentNode.AddChild(new AspxNode.CodeRender(code, location));
                break;

            case CodeBlockType.Expression:
                currentNode.AddChild(new AspxNode.CodeRenderExpression(code, location));
                break;

            case CodeBlockType.DataBinding:
                currentNode.AddChild(new AspxNode.DataBinding(code, location));
                break;

            case CodeBlockType.EncodedExpression:
                currentNode.AddChild(new AspxNode.CodeRenderEncode(code, location));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null);
        }
    }

    void IParserEventListener.OnTag(Location location, TagType tagType, string name, TagAttributes attributes)
    {
        switch (tagType)
        {
            case TagType.Close:
                {
                    string prefix;
                    string controlName;
                    if (IsAspxTag(name, out prefix, out controlName))
                    {
                        // find opening pair tag until the root
                        for (var node = currentNode; !(node is AspxNode.Root); node = node.Parent)
                        {
                            var casted = node as AspxNode.OpenAspxTag;
                            if (casted != null && casted.Prefix == prefix && casted.ControlName == controlName)
                            {
                                currentNode = casted.Parent;
                                break;
                            }
                        }
                        currentNode.AddChild(new AspxNode.CloseAspxTag(prefix, controlName, location));
                    }
                    else
                    {
                        // find closing pair tag no further than first opening ASPX tag
                        for (var node = currentNode; !(node is AspxNode.OpenAspxTag || node is AspxNode.Root); node = node.Parent)
                        {
                            var casted = node as AspxNode.OpenHtmlTag;
                            if (casted != null && casted.Name == name)
                            {
                                currentNode = casted.Parent;
                                break;
                            }
                        }
                        currentNode.AddChild(new AspxNode.CloseHtmlTag(name, location));
                    }
                    break;
                }

            case TagType.SelfClosing:
                {
                    string prefix;
                    string controlName;
                    var newNode = IsAspxTag(name, out prefix, out controlName)
                        ? (AspxNode)new AspxNode.SelfClosingAspxTag(prefix, controlName, attributes, location)
                        : new AspxNode.SelfClosingHtmlTag(name, attributes, location);

                    currentNode.AddChild(newNode);
                    break;
                }

            case TagType.Open:
                {
                    string prefix;
                    string controlName;
                    var newNode = IsAspxTag(name, out prefix, out controlName)
                        ? (AspxNode)new AspxNode.OpenAspxTag(prefix, controlName, attributes, location)
                        : new AspxNode.OpenHtmlTag(name, attributes, location);

                    currentNode.AddChild(newNode);
                    currentNode = newNode;
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(tagType), tagType, null);
        }
    }

    private static bool IsAspxTag(string name, out string prefix, out string controlName)
    {
        var separatorIndex = name.IndexOf(':');
        if (separatorIndex > 0)
        {
            prefix = string.Intern(name.Substring(0, separatorIndex));
            controlName = string.Intern(name.Substring(separatorIndex + 1));
            return true;
        }
        prefix = null;
        controlName = null;
        return false;
    }
}
