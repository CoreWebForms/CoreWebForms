// MIT License.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebForms.Compiler.Dynamic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WebForms.Compiler;

internal sealed class StaticCompilationService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<StaticCompilationService> _logger;
    private readonly IOptions<StaticCompilationOptions> _options;
    private readonly IWebFormsCompiler _compiler;

    public StaticCompilationService(
        IOptions<StaticCompilationOptions> options,
        IWebFormsCompiler compiler,
        IHostApplicationLifetime lifetime,
        ILogger<StaticCompilationService> logger)
    {
        _options = options;
        _compiler = compiler;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Starting static compilation");

        try
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var compilation = new PersistedCompilation(_options.Value);
            using var result = _compiler.CompilePages(compilation, stoppingToken);

            var pagesPath = Path.Combine(_options.Value.TargetDirectory, "webforms.pages.json");
            File.WriteAllText(pagesPath, JsonSerializer.Serialize(compilation.Pages, jsonOptions));

            var errorsPath = Path.Combine(_options.Value.TargetDirectory, "webforms.errors.json");
            File.WriteAllText(errorsPath, JsonSerializer.Serialize(compilation.Errors, jsonOptions));

            _logger.LogInformation("Completed compilation");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception while compiling occurred");
        }
        finally
        {
            _lifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    private sealed record PageDetails(string Path, string Type, string Assembly);

    private sealed record ErrorDetails(string Path, List<RoslynError> Diagnostics);

    private sealed class PersistedCompilation(StaticCompilationOptions options) : ICompilationStrategy
    {
        public List<PageDetails> Pages { get; } = [];

        public List<ErrorDetails> Errors { get; } = [];

        public bool HandleExceptions => false;

        Stream ICompilationStrategy.CreatePdbStream(string route, string typeName, string assemblyName)
            => CreateStream(GetAssemblyPath(assemblyName, isPdb: true));

        Stream ICompilationStrategy.CreatePeStream(string route, string typeName, string assemblyName)
        {
            Pages.Add(new(route, typeName, assemblyName));
            return CreateStream(GetAssemblyPath(assemblyName));
        }

        private static Stream CreateStream(string path) => File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        private string GetAssemblyPath(string assemblyName, bool isPdb = false)
        {
            var ext = isPdb ? "pdb" : "dll";
            return Path.Combine(options.TargetDirectory, $"{assemblyName}.{ext}");
        }

        public bool HandleErrors(string route, ImmutableArray<Diagnostic> errors)
        {
            Errors.Add(new(route, errors.ConvertToErrors().ToList()));

            return true;
        }
    }
}
