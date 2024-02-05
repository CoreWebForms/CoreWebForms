// MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class PageAssemblyLoadContext : AssemblyLoadContext
{
    private readonly Dictionary<string, Assembly> _map;
    private readonly ILogger<PageAssemblyLoadContext> _logger;

    private static readonly ConcurrentDictionary<string, int> _count = new();

    private static string GetName(string name)
    {
        var count = _count.AddOrUpdate(name, 1, static (key, value) => value + 1);

        return $"WebForms:{name}:{count}";
    }

    public PageAssemblyLoadContext(string route, IEnumerable<Assembly> assemblies, ILogger<PageAssemblyLoadContext> logger)
        : base(GetName(route), isCollectible: true)
    {
        _map = assemblies.ToDictionary(a => a.FullName!);
        _logger = logger;

        logger.LogInformation("Created assembly for {Path}", Name);

        Unloading += PageAssemblyLoadContext_Unloading;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_map.TryGetValue(assemblyName.FullName, out var existing))
        {
            return existing;
        }

        return base.Load(assemblyName);
    }

    private void PageAssemblyLoadContext_Unloading(AssemblyLoadContext obj)
    {
        Unloading -= PageAssemblyLoadContext_Unloading;

        _logger.LogInformation("Unloading assembly load context for {Path}", Name);
    }
}
