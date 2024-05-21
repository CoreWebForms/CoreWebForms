// MIT License.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using WebForms.Features;

using HttpContext = System.Web.HttpContext;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilation : IHttpHandlerCollection
{
    private readonly IWebFormsCompiler _compiler;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<DynamicSystemWebCompilation> _logger;
    private readonly ManualResetEventSlim _event = new(false);
    private readonly CancellationChangeTokenSource _changeTokenSource = new();

    private ICompilationResult? _result;

    public IEnumerable<NamedHttpHandlerRoute> NamedRoutes => [];

    public DynamicSystemWebCompilation(IWebFormsCompiler compiler, ILoggerFactory factory)
    {
        _compiler = compiler;
        _factory = factory;
        _logger = factory.CreateLogger<DynamicSystemWebCompilation>();
    }

    public IFileProvider Files => _compiler.Files;

    IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata()
    {
        if (_result is not { } result)
        {
            yield break;
        }

        foreach (var route in result.Types.Paths)
        {
            if (route.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                if (result.Types.GetForPath(route) is { } type)
                {
                    yield return new WrappedMetadata(HandlerMetadata.Create(route, type), result.Types);
                }
                else if (result.Types.TryGetException(route, out var exception))
                {
                    yield return HandlerMetadata.Create(route, new ErrorHandler(exception));
                }
                else
                {
                    _logger.LogError("'{Route}' is registered but could not be found", route);
                }
            }
        }
    }

    IChangeToken IHttpHandlerCollection.GetChangeToken() => _changeTokenSource.GetChangeToken();

    public void CompilePages(CancellationToken token)
    {
        _event.Reset();

        var result = _compiler.CompilePages(new DynamicCompilationProvider(_factory.CreateLogger<DynamicCompilationProvider>()), token);

        var previous = _result;
        _result = result;

        _changeTokenSource.OnChange();

        previous?.Dispose();

        _event.Set();
    }

    internal sealed class DynamicCompilationProvider(ILogger<DynamicCompilationProvider> logger) : ICompilationStrategy
    {
        public bool HandleExceptions => true;

        Stream ICompilationStrategy.CreatePdbStream(string route, string typeName, string assemblyName) => new MemoryStream();

        Stream ICompilationStrategy.CreatePeStream(string route, string typeName, string assemblyName) => new MemoryStream();

        bool ICompilationStrategy.HandleErrors(string route, ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (var d in diagnostics)
            {
                var logLevel = d.Severity switch
                {
                    DiagnosticSeverity.Hidden => LogLevel.Trace,
                    DiagnosticSeverity.Info => LogLevel.Debug,
                    DiagnosticSeverity.Warning => LogLevel.Warning,
                    DiagnosticSeverity.Error => LogLevel.Error,
                    _ => LogLevel.Critical,
                };

                logger.Log(logLevel, "[{Id}] {Message} @{Location}", d.Id, d.GetMessage(CultureInfo.CurrentCulture), d.Location);
            }

            return true;
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

    private sealed class WrappedMetadata(IHttpHandlerMetadata metadata, IWebFormsCompilationFeature compiledTypes) : IHttpHandlerMetadata, IWebFormsCompilationFeature
    {
        SessionStateBehavior IHttpHandlerMetadata.Behavior => metadata.Behavior;

        string IHttpHandlerMetadata.Route => metadata.Route;

        IHttpHandler IHttpHandlerMetadata.Create(Microsoft.AspNetCore.Http.HttpContext context) => metadata.Create(context);

        Type? IWebFormsCompilationFeature.GetForPath(string virtualPath) => compiledTypes.GetForPath(virtualPath);

        bool IWebFormsCompilationFeature.TryGetException(string path, [MaybeNullWhen(false)] out Exception exception) => compiledTypes.TryGetException(path, out exception);

        IReadOnlyCollection<string> IWebFormsCompilationFeature.Paths => compiledTypes.Paths;
    }
}
