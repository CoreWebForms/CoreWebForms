// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
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

        var generated = GenerateFile(text, contents);

        if (generated is null)
        {
            return;
        }

        var path = text.Path.TrimStart('/') + ".g.cs";

        context.AddSource(path, generated);
    }

    private string? GenerateFile(AdditionalText text, string contents)
    {
        using var stringWriter = new StringWriter();
        using var writer = new IndentedTextWriter(stringWriter);

        var builder = new CSharpPageBuilder(text.Path, writer, contents);

        builder.WriteSource();

        return stringWriter.ToString();
    }
}

