// MIT License.

using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicControlCollection : ITypeResolutionService, IMetadataProvider, IDisposable
{
    private readonly AssemblyLoadContext _context;
    private readonly ILogger<DynamicControlCollection> _logger;
    private readonly LoadedAssemblies _loader;

    public DynamicControlCollection(ILogger<DynamicControlCollection> logger)
    {
        _logger = logger;
        _context = AssemblyLoadContext.Default;
        _loader = new LoadedAssemblies(_logger);

        Initialize();
    }

    private void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

        foreach (var assembly in _context.Assemblies)
        {
            _loader.Load(assembly);
        }

        foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            if (_loader.Load(file, out var assemblyName))
            {
                _context.LoadFromAssemblyName(assemblyName);
            }
        }
    }

    private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        => _loader.Load(args.LoadedAssembly);

    IEnumerable<MetadataReference> IMetadataProvider.References => _loader.References;

    IEnumerable<Assembly> IMetadataProvider.ControlAssemblies => _loader.Controls;

    Assembly? ITypeResolutionService.GetAssembly(AssemblyName assemblyName)
    {
        return _context.LoadFromAssemblyName(assemblyName);
    }

    Type? ITypeResolutionService.GetType(string type)
    {
        foreach (var control in _loader.Controls)
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
        private ImmutableDictionary<AssemblyName, Assembly> _controls = ImmutableDictionary<AssemblyName, Assembly>.Empty.WithComparers(AssemblyNameComparer.Instance);
        private ImmutableDictionary<AssemblyName, MetadataReference> _map = ImmutableDictionary<AssemblyName, MetadataReference>.Empty.WithComparers(AssemblyNameComparer.Instance);

        public IEnumerable<Assembly> Controls => _controls.Values;

        public IEnumerable<MetadataReference> References => _map.Values;

        public void Load(Assembly assembly)
        {
            logger.LogTrace("Searching {Assembly} for tag prefixes", assembly.FullName);

            if (assembly.GetCustomAttributes<TagPrefixAttribute>().Any())
            {
                logger.LogInformation("Found tag prefixes in {Assembly}", assembly.FullName);
                ImmutableInterlocked.TryAdd(ref _controls, assembly.GetName(), assembly);
            }

            var assemblyName = assembly.GetName();

            if (assembly is { Location: { Length: > 0 } location } && !_map.ContainsKey(assemblyName))
            {
                logger.LogTrace("Loading loaded {AssemblyName} for dynamic compilations", assemblyName);
                ImmutableInterlocked.TryAdd(ref _map, assemblyName, MetadataReference.CreateFromFile(location));
            }
        }

        public bool Load(string file, [MaybeNullWhen(false)] out AssemblyName result)
        {
            try
            {
                if (MetadataReader.GetAssemblyName(file) is { } assemblyName)
                {
                    if (!_map.ContainsKey(assemblyName))
                    {
                        logger.LogTrace("Loading unloaded {AssemblyName} for dynamic compilations", assemblyName);
                        var metadataReference = MetadataReference.CreateFromFile(file);
                        ImmutableInterlocked.TryAdd(ref _map, assemblyName, metadataReference);
                    }

                    if (!_controls.ContainsKey(assemblyName) && HasControls(file))
                    {
                        result = assemblyName;
                        return true;
                    }
                }
            }
            catch (BadImageFormatException)
            {
                // Possibly a native assembly, so we'll ignore it
            }

            result = null;
            return false;
        }

        private static bool HasControls(string file)
        {
            using var stream = File.OpenRead(file);
            using var peReader = new PEReader(stream);

            var reader = peReader.GetMetadataReader();
            return reader.HasAttribute<TagPrefixAttribute>();
        }

        private sealed class AssemblyNameComparer : IEqualityComparer<AssemblyName>
        {
            public static AssemblyNameComparer Instance { get; } = new();

            public bool Equals(AssemblyName? x, AssemblyName? y)
                => StringComparer.OrdinalIgnoreCase.Equals(x?.FullName, y?.FullName);

            public int GetHashCode([DisallowNull] AssemblyName obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
        }
    }
}
