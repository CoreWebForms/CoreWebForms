// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Generator;

[Generator]
public class PageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var aspxFiles = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase));

        context.RegisterSourceOutput(aspxFiles, GenerateSource);
    }

    private void GenerateSource(SourceProductionContext context, AdditionalText text)
    {
        // TODO: maybe want to use CopyTo to load into a pooled buffer instead of getting a string each time
        var contents = text.GetText(context.CancellationToken)?.ToString();

        if (contents is null)
        {
            return;
        }

        var generated = GenerateFile(context, text, contents);

        if (generated is null)
        {
            return;
        }

        var path = text.Path.TrimStart('/') + ".g.cs";

        context.AddSource(path, generated);
    }

    private string? GenerateFile(SourceProductionContext context, AdditionalText text, string contents)
    {
        using var stringWriter = new StringWriter();
        using var writer = new IndentedTextWriter(stringWriter);

        var parser = new AspxParser();
        var source = new AspxSource(text.Path, contents);
        var tree = parser.Parse(source);
        var builder = new PageBuilder(text, writer, context);
        tree.RootNode.Accept(builder);

        return builder.HasErrors ? null : stringWriter.ToString();
    }
}

class PageBuilder : DepthFirstAspxVisitor<object>
{
    private readonly object _obj = new();
    private readonly AdditionalText _text;

    public PageBuilder(AdditionalText text, TextWriter writer, SourceProductionContext context)
    {
        _text = text;
        Writer = writer;
        Context = context;
    }

    public TextWriter Writer { get; }

    public SourceProductionContext Context { get; }

    public void RaiseError()
    {
        HasErrors = true;
    }

    public bool HasErrors { get; private set; }

    public override object Visit(AspxNode.AspxDirective node)
    {
        var info = new DirectiveDetails(node);

        Writer.Write("[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(\"");
        Writer.Write(_text.Path);
        Writer.WriteLine("\")]");
        Writer.Write("internal partial class ");
        Writer.Write(ConvertPathToClassName(info.CodeBehind));
        Writer.Write(" : ");
        Writer.WriteLine(info.Inherits);
        Writer.WriteLine("{");
        Writer.WriteLine("}");

        return _obj;
    }

    private string ConvertPathToClassName(string input)
    {
        var sb = new StringBuilder(input);

        sb.Replace(".", "_");
        sb.Replace("/", "_");
        sb.Replace("\\", "_");

        return sb.ToString();
    }

    private readonly struct DirectiveDetails
    {
        private readonly AspxNode.AspxDirective _directive;

        public DirectiveDetails(AspxNode.AspxDirective directive)
        {
            _directive = directive;
        }

        public string MasterPageFile => GetAttribute();

        public string Language => GetAttribute();

        public string Inherits => GetAttribute() ?? "global::System.Web.UI.Page";

        public bool AutoEventWireup => bool.TryParse(GetAttribute(), out var result) ? result : false;

        public string Title => GetAttribute();

        public string CodeBehind => GetAttribute();

        private string GetAttribute([CallerMemberName] string name = null!)
            => _directive.Attributes[name];
    }
}
