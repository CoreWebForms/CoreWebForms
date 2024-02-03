// MIT License.

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Compiler.Generator;

[Generator]
public class MapPathReplacementGenerator : IIncrementalGenerator
{
    private sealed record WebFormsDetails(string Path, string Type);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var isEnabled = context.AnalyzerConfigOptionsProvider
            .Select((p, _) =>
            {
                var v = p.GlobalOptions.TryGetValue("build_property.UseWebFormsInterceptors", out var value) && bool.TryParse(value, out var result) && result;
                return v;
            });

        var pages = context.AdditionalTextsProvider
            .Where(p => p.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((args, token) =>
            {
                try
                {
                    if (args.Right.GetOptions(args.Left).TryGetValue("build_metadata.AdditionalFiles.LinkBase", out var result))
                    {
                        var linkBase = new Uri(result);
                        var filepath = new Uri(args.Left.Path);
                        var relative = "/" + linkBase.MakeRelativeUri(filepath).ToString();

                        return new WebFormsDetails(relative, "ASP._" + relative.Replace('/', '_').Replace('.', '_'));
                    }
                }
                catch // TODO: handle exception
                {
                }

                return null;
            })
            .Where(path => path is { })
            .Collect();

        var pagesWithDiagnostics = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node.TryGetMapMethodName(out var method) && method == "MapWebFormsPages",
            transform: static (context, token) =>
            {
                var operation = context.SemanticModel.GetOperation(context.Node, token);

                if (operation is IInvocationOperation invocation)
                {
                    return invocation.GetLocation();
                }

                return default;
            })
            .Where(static invocation => invocation is { });

        var final = pagesWithDiagnostics.Combine(pages).Combine(isEnabled);

        context.RegisterSourceOutput(final, (context, source) =>
            {
                if (!source.Right)
                {
                    return;
                }

                if (source.Left.Left is not { } location)
                {
                    return;
                }

                var pages = source.Left.Right;

                if (pages.IsDefaultOrEmpty)
                {
                    return;
                }

                using var str = new StringWriter();
                using var indented = new IndentedTextWriter(str);

                indented.WriteLine("﻿// <auto-generated />");
                indented.WriteLine();
                indented.WriteLine("using Microsoft.AspNetCore.Routing;");
                indented.WriteLine("using System.Web;");
                indented.WriteLine();
                indented.WriteLine("namespace WebForms.Generated");
                indented.WriteLine("{");
                indented.Indent++;
                indented.WriteLine("internal static class InterceptedCompiledWebForms");
                indented.WriteLine("{");
                indented.Indent++;

                indented.Write("[System.Runtime.CompilerServices.InterceptsLocation(\"");
                indented.Write(location.FilePath.Replace("\\", "\\\\"));
                indented.Write("\", ");
                indented.Write(location.Line);
                indented.Write(", ");
                indented.Write(location.Character);
                indented.WriteLine(")]");

                indented.WriteLine("internal static IEndpointConventionBuilder MapWebFormsPages(this IEndpointRouteBuilder endpoints)");
                indented.WriteLine("{");
                indented.Indent++;

                var first = true;
                foreach (var page in pages)
                {
                    if (first)
                    {
                        indented.Write("var builder = ");
                        first = false;
                    }

                    indented.WriteLine($"endpoints.MapHttpHandler(\"{page!.Path}\", typeof({page.Type}));");
                }

                indented.WriteLine();
                indented.WriteLine("return builder;");
                indented.Indent--;

                indented.WriteLine("}");

                indented.Indent--;
                indented.WriteLine("}");
                indented.Indent--;
                indented.WriteLine("}");
                indented.Write("""
                    #pragma warning disable

                    namespace System.Runtime.CompilerServices
                    {
                        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                        internal sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
                        {
                        }
                    }
                    """);

                context.AddSource("InterceptedCompiledWebForms", str.ToString());
            });
    }
}
