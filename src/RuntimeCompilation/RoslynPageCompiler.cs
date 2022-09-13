// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Runtime.Loader;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynPageCompiler : IPageCompiler
{
    private readonly ILogger<RoslynPageCompiler> _logger;

    public RoslynPageCompiler(ILogger<RoslynPageCompiler> logger)
    {
        _logger = logger;
    }

    public async Task<Type?> CompilePageAsync(PageFile file, CancellationToken token)
    {
        try
        {
            var (contents, className) = await GetSourceAsync(file.Directory, file.File).ConfigureAwait(false);

            var normalized = NormalizeName(file.Directory, file.File.Name);

            var tree = CSharpSyntaxTree.ParseText(contents, cancellationToken: token);

            var compilation = CSharpCompilation.Create(normalized,
                options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: GetMetadataReferences());

            using (var ms = new MemoryStream())
            {
                compilation.Emit(ms, cancellationToken: token);
                ms.Position = 0;

                var context = new PageAssemblyLoadContext(normalized);
                var assembly = context.LoadFromStream(ms);

                return assembly.GetType(className) ?? throw new InvalidOperationException("Could not find class in generated assembly");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error compiling file {Path}", file.File.Name);
            return null;
        }
    }

    public void RemovePage(Type type)
    {
        var alc = AssemblyLoadContext.GetLoadContext(type.Assembly);

        if (alc is not PageAssemblyLoadContext)
        {
            throw new InvalidOperationException("Tried to unload something that is not a page");
        }

        alc.Unload();
    }

    private sealed class PageAssemblyLoadContext : AssemblyLoadContext
    {
        public PageAssemblyLoadContext(string name)
            : base(name, isCollectible: true)
        {
        }
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            if (!assembly.IsDynamic)
            {
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }
    }

    private static string NormalizeName(string directory, string name)
        => Path.Combine(directory, name)
            .Replace(".", "_")
            .Replace("/", "_")
            .Replace("\\", "_");

    private static async Task<(string Contents, string name)> GetSourceAsync(string directory, IFileInfo file)
    {
        using var stringWriter = new StringWriter();
        using var writer = new IndentedTextWriter(stringWriter);

        var contents = await GetContentsAsync(file).ConfigureAwait(false);
        var generator = new CSharpPageBuilder(Path.Combine(directory, file.Name), writer, contents);

        generator.WriteSource();

        return (stringWriter.ToString(), generator.Name);
    }

    private static async Task<string> GetContentsAsync(IFileInfo file)
    {
        using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
