// MIT License.

using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WebForms.Internal;

using static WebForms.Compiler.Dynamic.DynamicSystemWebCompilation;

using HttpContext = System.Web.HttpContext;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilation : SystemWebCompilation<DynamicCompiledPage>, IHttpHandlerCollection, IWebFormsCompiler
{
    private readonly ILoggerFactory _factory;
    private readonly ILogger _logger;
    private readonly ManualResetEventSlim _event = new(false);
    private readonly CancellationChangeTokenSource _changeTokenSource = new();

    public IEnumerable<NamedHttpHandlerRoute> NamedRoutes => [];

    public DynamicSystemWebCompilation(
        ILoggerFactory factory,
        IMetadataProvider metadataProvider,
        IOptions<WebFormsOptions> webFormsOptions,
        IOptions<PageCompilationOptions> pageCompilationOptions)
        : base(factory, metadataProvider, webFormsOptions, pageCompilationOptions)
    {
        _logger = factory.CreateLogger<DynamicSystemWebCompilation>();
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
            var errors = GetErrors(result.Diagnostics).ToList();

            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, route);

            return new DynamicCompiledPage(new(route), [])
            {
                Exception = new RoslynCompilationException(route, errors)
            };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(route, assemblies, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);

        if (assembly.GetType(typeName) is Type type)
        {
            return new DynamicCompiledPage(new(route), embedded.Select(t => t.FilePath).ToArray())
            {
                Type = type,
                MetadataReference = compilation.ToMetadataReference()
            };
        }

        throw new InvalidOperationException("No type found");
    }

    IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata()
    {
        var accessor = TypeAccessor;

        foreach (var page in GetPages())
        {
            if (page.Type is { } type)
            {
                yield return new WrappedMetadata(HandlerMetadata.Create(page.Path, type), accessor);
            }
            else if (page.Exception is { } ex)
            {
                yield return HandlerMetadata.Create(page.Path, new ErrorHandler(ex));
            }
        }
    }

    IChangeToken IHttpHandlerCollection.GetChangeToken() => _changeTokenSource.GetChangeToken();

    Task IWebFormsCompiler.CompilePagesAsync(CancellationToken token)
    {
        _event.Reset();

        using (MarkRecompile())
        {
            CompileAllPages(token);

            _event.Set();
            _changeTokenSource.OnChange();
        }

        return Task.CompletedTask;
    }

    protected override DynamicCompiledPage CreateErrorPage(string path, Exception e)
        => new(new(path), [])
        {
            Exception = e
        };

    internal sealed class DynamicCompiledPage(PagePath path, string[] dependencies) : CompiledPage(path, dependencies)
    {
        public override void Dispose()
        {
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

    private sealed class WrappedMetadata(IHttpHandlerMetadata metadata, ICompiledTypeAccessor compiledTypes) : IHttpHandlerMetadata, ICompiledTypeAccessor
    {
        SessionStateBehavior IHttpHandlerMetadata.Behavior => metadata.Behavior;

        string IHttpHandlerMetadata.Route => metadata.Route;

        IHttpHandler IHttpHandlerMetadata.Create(Microsoft.AspNetCore.Http.HttpContext context) => metadata.Create(context);

        Type? ICompiledTypeAccessor.GetForPath(string virtualPath) => compiledTypes.GetForPath(virtualPath);
    }
}
