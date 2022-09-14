// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynPageCompiler : IPageCompiler
{
    private readonly bool _isDebug;
    private readonly ILogger<RoslynPageCompiler> _logger;
    private readonly ILoggerFactory _factory;

    public RoslynPageCompiler(ILoggerFactory factory, IHostEnvironment env)
    {
        _isDebug = env.IsDevelopment();
        _logger = factory.CreateLogger<RoslynPageCompiler>();
        _factory = factory;
    }

    public async Task<Type?> CompilePageAsync(PageFile file, CancellationToken token)
    {
        try
        {
            using var sourceStream = new MemoryStream();
            var (className, endpointPath) = await GetSourceAsync(file.Directory, file.File, sourceStream).ConfigureAwait(false);

            sourceStream.Position = 0;

            var sourceText = SourceText.From(sourceStream, Encoding.UTF8, canBeEmbedded: true);
            var tree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: token)
                .WithFilePath($"{className}.cs");
            var optimization = _isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release;

            var compilation = CSharpCompilation.Create($"WebForms.{className}",
                options: new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: optimization),
                syntaxTrees: new[] { tree },
                references: GetMetadataReferences());

            using var peStream = new MemoryStream();
            using var pdbStream = new MemoryStream();

            var result = compilation.Emit(
                embeddedTexts: new[] { EmbeddedText.FromSource(tree.FilePath, sourceText) },
                peStream: peStream,
                pdbStream: pdbStream,
                cancellationToken: token);

            if (!result.Success)
            {
                _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, endpointPath);
                return null;
            }

            pdbStream.Position = 0;
            peStream.Position = 0;

            var context = new PageAssemblyLoadContext(endpointPath, _factory.CreateLogger<PageAssemblyLoadContext>());
            var assembly = context.LoadFromStream(peStream, pdbStream);

            return assembly.GetType(className) ?? throw new InvalidOperationException("Could not find class in generated assembly");
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
        private readonly ILogger<PageAssemblyLoadContext> _logger;

        private static long _count;

        private static string GetName(string name)
        {
            var count = Interlocked.Increment(ref _count);

            return $"WebForms:{name}:{count}";
        }

        public PageAssemblyLoadContext(string route, ILogger<PageAssemblyLoadContext> logger)
            : base(GetName(route), isCollectible: true)
        {
            _logger = logger;

            logger.LogInformation("Created assembly for {Path}", Name);

            Unloading += PageAssemblyLoadContext_Unloading;
        }

        private void PageAssemblyLoadContext_Unloading(AssemblyLoadContext obj)
        {
            Unloading -= PageAssemblyLoadContext_Unloading;

            _logger.LogInformation("Unloading assembly load context for {Path}", Name);
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

    private static async Task<(string ClassName, string EndpointPath)> GetSourceAsync(string directory, IFileInfo file, Stream stream)
    {
        using var streamWriter = new StreamWriter(stream, leaveOpen: true);
        using var writer = new IndentedTextWriter(streamWriter);

        var contents = await GetContentsAsync(file).ConfigureAwait(false);
        var generator = new CSharpPageBuilder(Path.Combine(directory, file.Name), writer, contents);

        generator.WriteSource();

        return (generator.ClassName, generator.Path);
    }

    private static async Task<string> GetContentsAsync(IFileInfo file)
    {
        using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
