// MIT License.

using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.Loader;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicControlCollection : ITypeResolutionService, IMetadataProvider, IDisposable
{
    private readonly AssemblyLoadContext _context;
    private readonly ILogger<DynamicControlCollection> _logger;
    private ImmutableHashSet<Assembly> _controls;
    private ImmutableDictionary<AssemblyName, MetadataReference> _map;

    public DynamicControlCollection(ILogger<DynamicControlCollection> logger)
    {
        _logger = logger;
        _controls = ImmutableHashSet<Assembly>.Empty;
        _context = AssemblyLoadContext.Default;
        _map = ImmutableDictionary<AssemblyName, MetadataReference>.Empty;

        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

        ProcessLoadedAssemblies();
    }

    private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        => ProcessAssembly(args.LoadedAssembly);

    private void ProcessLoadedAssemblies()
    {
        foreach (var assembly in _context.Assemblies)
        {
            ProcessAssembly(assembly);
        }
    }

    private void ProcessAssembly(Assembly assembly)
    {
        _logger.LogTrace("Searching {Assembly} for tag prefixes", assembly.FullName);

        if (assembly.GetCustomAttributes<TagPrefixAttribute>().Any())
        {
            _logger.LogInformation("Found tag prefixes in {Assembly}", assembly.FullName);
            ImmutableInterlocked.Update(ref _controls, c => c.Add(assembly));
        }

        if (assembly is { Location: { Length: > 0 } location })
        {
            ImmutableInterlocked.TryAdd(ref _map, assembly.GetName(), MetadataReference.CreateFromFile(location));
        }
    }

    IEnumerable<MetadataReference> IMetadataProvider.References => _map.Values;

    IEnumerable<Assembly> IMetadataProvider.ControlAssemblies => _controls;

    Assembly? ITypeResolutionService.GetAssembly(AssemblyName assemblyName)
    {
        return _context.LoadFromAssemblyName(assemblyName);
    }

    Type? ITypeResolutionService.GetType(string type)
    {
        foreach (var control in _controls)
        {
            if (control.GetType(type, throwOnError: false) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    void IDisposable.Dispose()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= CurrentDomain_AssemblyLoad;
    }

    // TODO: unused
    Assembly? ITypeResolutionService.GetAssembly(AssemblyName name, bool throwOnError)
    {
        throw new NotImplementedException();
    }

    string? ITypeResolutionService.GetPathOfAssembly(AssemblyName name)
    {
        throw new NotImplementedException();
    }

    Type? ITypeResolutionService.GetType(string name, bool throwOnError)
    {
        throw new NotImplementedException();
    }

    Type? ITypeResolutionService.GetType(string name, bool throwOnError, bool ignoreCase)
    {
        throw new NotImplementedException();
    }

    void ITypeResolutionService.ReferenceAssembly(AssemblyName name)
    {
        throw new NotImplementedException();
    }
}
