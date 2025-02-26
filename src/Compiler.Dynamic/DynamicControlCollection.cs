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
    private readonly Lazy<LoadedAssemblies> _loaded;

    public DynamicControlCollection(ILogger<DynamicControlCollection> logger)
    {
        _logger = logger;
        _context = AssemblyLoadContext.Default;

        _loaded = new(() =>
        {
            var loader = new LoadedAssemblies(_logger);

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            foreach (var assembly in _context.Assemblies)
            {
                loader.Load(assembly);
            }

            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
            {
                loader.Load(file);
            }

            return loader;
        });
    }

    private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        => _loaded.Value.Load(args.LoadedAssembly);

    IEnumerable<MetadataReference> IMetadataProvider.References => _loaded.Value.References;

    IEnumerable<Assembly> IMetadataProvider.ControlAssemblies => _loaded.Value.Controls;

    Assembly? ITypeResolutionService.GetAssembly(AssemblyName assemblyName)
    {
        return _context.LoadFromAssemblyName(assemblyName);
    }

    Type? ITypeResolutionService.GetType(string type)
    {
        foreach (var control in _loaded.Value.Controls)
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

    private sealed class LoadedAssemblies(ILogger logger)
    {
        private ImmutableHashSet<Assembly> _controls = ImmutableHashSet<Assembly>.Empty;
        private ImmutableDictionary<AssemblyName, MetadataReference> _map = ImmutableDictionary<AssemblyName, MetadataReference>.Empty;

        public IEnumerable<Assembly> Controls => _controls;

        public IEnumerable<MetadataReference> References => _map.Values;

        public void Load(Assembly assembly)
        {
            logger.LogTrace("Searching {Assembly} for tag prefixes", assembly.FullName);

            if (assembly.GetCustomAttributes<TagPrefixAttribute>().Any())
            {
                logger.LogInformation("Found tag prefixes in {Assembly}", assembly.FullName);
                ImmutableInterlocked.Update(ref _controls, c => c.Add(assembly));
            }

            var assemblyName = assembly.GetName();

            if (assembly is { Location: { Length: > 0 } location } && !_map.ContainsKey(assemblyName))
            {
                logger.LogTrace("Loading loaded {AssemblyName} for dynamic compilations", assemblyName);
                ImmutableInterlocked.TryAdd(ref _map, assemblyName, MetadataReference.CreateFromFile(location));
            }
        }

        public void Load(string file)
        {
            try
            {
                if (AssemblyName.GetAssemblyName(file) is { } assemblyName && !_map.ContainsKey(assemblyName))
                {
                    logger.LogTrace("Loading unloaded {AssemblyName} for dynamic compilations", assemblyName);
                    ImmutableInterlocked.TryAdd(ref _map, assemblyName, MetadataReference.CreateFromFile(file));
                }
            }
            catch (BadImageFormatException)
            {
                // Possibly a native assembly, so we'll ignore it
            }
        }
    }
}
