// MIT License.

using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class PageAssemblyLoadContext : AssemblyLoadContext
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
