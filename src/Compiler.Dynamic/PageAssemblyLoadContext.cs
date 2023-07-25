// MIT License.

using System.Collections.Concurrent;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class PageAssemblyLoadContext : AssemblyLoadContext
{
    private readonly ILogger<PageAssemblyLoadContext> _logger;

    private static readonly ConcurrentDictionary<string, int> _count = new();

    private static string GetName(string name)
    {
        var count = _count.AddOrUpdate(name, 1, static (key, value) => value + 1);

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
