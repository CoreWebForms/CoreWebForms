// MIT License.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    public IWebFormsCompilationFeature Current => _result?.Types ?? EmptyWebFormsCompilation.Instance;

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
                    yield return HandlerMetadata.Create(route, type);
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
            context.Response.StatusCode = 500;

            return context.AsAspNetCore().Response.WriteAsJsonAsync(GetDetails(context.Request.Path));
        }

        private ProblemDetails GetDetails(string path)
        {
            if (e is RoslynCompilationException r)
            {
                var errors = r.Error.Select(e => new
                {
                    e.Severity,
                    e.Message,
                });

                return new ProblemDetails
                {
                    Title = "There was an error compiling the page",
                    Instance = path,
                    Detail = e.Message,
                    Extensions =
                    {
                        { "diagnostics", errors }
                    }
                };
            }
            else
            {
                return new ProblemDetails
                {
                    Title = "Unknown error while compiling the page",
                    Detail = e.Message,
                };
            }
        }
    }

    private sealed class EmptyWebFormsCompilation : IWebFormsCompilationFeature
    {
        public static EmptyWebFormsCompilation Instance { get; } = new();

        public IReadOnlyCollection<string> Paths => [];

        public Type? GetForPath(string virtualPath) => null;

        public bool TryGetException(string path, [MaybeNullWhen(false)] out Exception exception)
        {
            exception = null;
            return false;
        }
    }
}
