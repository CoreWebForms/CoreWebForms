// MIT License.

using System.Reflection;
using System.Runtime.Loader;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using static WebForms.Compiler.Dynamic.DynamicSystemWebCompilation;

using HttpContext = System.Web.HttpContext;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilation : SystemWebCompilation<DynamicCompiledPage>, IHttpHandlerCollection, IWebFormsCompiler
{
    private readonly Dictionary<Assembly, MetadataReference> _references = new();
    private readonly IOptions<PageCompilationOptions> _options;
    private readonly ILoggerFactory _factory;
    private readonly ILogger _logger;
    private readonly ManualResetEventSlim _event = new(false);
    private readonly CancellationChangeTokenSource _changeTokenSource = new();

    public IEnumerable<NamedHttpHandlerRoute> NamedRoutes => [];

    public DynamicSystemWebCompilation(
        ILoggerFactory factory,
        IHostEnvironment env,
        IOptions<PageCompilationOptions> options,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection)
        : base(env, factory, options, pagesSection, compilationSection)
    {
        _logger = factory.CreateLogger<DynamicSystemWebCompilation>();
        _options = options;
        _factory = factory;
    }

    protected override DynamicCompiledPage CreateCompiledPage(
      Compilation compilation,
      string route,
      string typeName,
      IEnumerable<SyntaxTree> trees,
      IEnumerable<MetadataReference> references,
      IEnumerable<EmbeddedText> embedded,
      IEnumerable<Assembly> assemblies,
      CancellationToken token)
    {
        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var result = compilation.Emit(
            embeddedTexts: embedded,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, route);

            return new DynamicCompiledPage(this, new(route), [])
            {
                Exception = new RoslynCompilationException(route, GetErrors(result.Diagnostics))
            };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(route, assemblies, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);

        if (assembly.GetType(typeName) is Type type)
        {
            return new DynamicCompiledPage(this, new(route), embedded.Select(t => t.FilePath).ToArray())
            {
                Type = type,
                MetadataReference = compilation.ToMetadataReference()
            };
        }

        throw new InvalidOperationException("No type found");
    }

    protected override IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        foreach (var assembly in AssemblyLoadContext.Default.Assemblies.Concat(_options.Value.Assemblies))
        {
            if (!assembly.IsDynamic)
            {
                if (!_references.TryGetValue(assembly, out var metadata))
                {
                    metadata = MetadataReference.CreateFromFile(assembly.Location);
                    _references.Add(assembly, metadata);
                }

                references.Add(metadata);
            }
        }

        return references;
    }

    IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata()
    {
        foreach (var page in GetPages())
        {
            if (page.Type is { } type)
            {
                yield return HandlerMetadata.Create(page.Path, type);
            }
            else if (page.Exception is { } ex)
            {
                yield return HandlerMetadata.Create(page.Path, new ErrorHandler(ex));
            }
        }
    }

    IChangeToken IHttpHandlerCollection.GetChangeToken() => _changeTokenSource.GetChangeToken();

    async Task IWebFormsCompiler.CompilePagesAsync(CancellationToken token)
    {
        _event.Reset();

        using (MarkRecompile())
        {
            foreach (var file in Files.GetFiles())
            {
                if (file.FullPath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    await CompilePageAsync(file.FullPath, token).ConfigureAwait(false);
                }
            }

            _event.Set();
            _changeTokenSource.OnChange();
        }
    }

    protected override DynamicCompiledPage CreateErrorPage(string path, Exception e)
        => new(this, new(path), [])
        {
            Exception = e
        };

    internal sealed class DynamicCompiledPage(DynamicSystemWebCompilation compilation, PagePath path, string[] dependencies) : CompiledPage(path, dependencies)
    {
        public override void Dispose()
        {
            compilation.RemovePage(Path);

            foreach (var d in PageDependencies)
            {
                d.Dispose();
            }

            base.Dispose();
        }
    }

    private sealed class ErrorHandler(Exception e) : HttpTaskAsyncHandler
    {
        public override bool IsReusable => true;

        public override Task ProcessRequestAsync(HttpContext context)
        {
            if (e is RoslynCompilationException r)
            {
                return context.AsAspNetCore().Response.WriteAsJsonAsync(r.Error.Select(e => new
                {
                    e.Severity,
                    e.Message
                }));
            }
            else
            {
                context.Response.Write(e.Message);
                return Task.CompletedTask;
            }
        }
    }
}
