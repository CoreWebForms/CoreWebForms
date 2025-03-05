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

    private ImmutableDictionary<AssemblyName, Assembly> _controls = ImmutableDictionary<AssemblyName, Assembly>.Empty.WithComparers(AssemblyNameComparer.Instance);
    private ImmutableDictionary<AssemblyName, MetadataReference> _map = ImmutableDictionary<AssemblyName, MetadataReference>.Empty.WithComparers(AssemblyNameComparer.Instance);

    public DynamicControlCollection(ILogger<DynamicControlCollection> logger)
    {
        _logger = logger;
        _context = AssemblyLoadContext.Default;

        Initialize();
    }

    private void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

        foreach (var assembly in _context.Assemblies)
        {
            LoadAssembly(assembly);
        }

        foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            LoadMetadataReference(file);
        }
    }

    private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        => LoadAssembly(args.LoadedAssembly);

    IEnumerable<MetadataReference> IMetadataProvider.References => _map.Values;

    IEnumerable<Assembly> IMetadataProvider.ControlAssemblies => _controls.Values;

    Assembly? ITypeResolutionService.GetAssembly(AssemblyName assemblyName)
    {
        return _context.LoadFromAssemblyName(assemblyName);
    }

    Type? ITypeResolutionService.GetType(string type)
    {
        foreach (var control in _controls.Values)
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

    private void LoadAssembly(Assembly assembly)
    {
        _logger.LogTrace("Searching {Assembly} for tag prefixes", assembly.FullName);

        if (assembly.GetCustomAttributes<TagPrefixAttribute>().Any())
        {
            _logger.LogInformation("Found tag prefixes in {Assembly}", assembly.FullName);
            ImmutableInterlocked.TryAdd(ref _controls, assembly.GetName(), assembly);
        }

        var assemblyName = assembly.GetName();

        if (assembly is { Location: { Length: > 0 } location } && !_map.ContainsKey(assemblyName))
        {
            _logger.LogTrace("Loading loaded {AssemblyName} for dynamic compilations", assemblyName);
            ImmutableInterlocked.TryAdd(ref _map, assemblyName, MetadataReference.CreateFromFile(location));
        }
    }

    private void LoadMetadataReference(string file)
    {
        try
        {
            if (MetadataReader.GetAssemblyName(file) is { } assemblyName)
            {
                if (!_map.ContainsKey(assemblyName))
                {
                    _logger.LogTrace("Loading unloaded {AssemblyName} for dynamic compilations", assemblyName);
                    var metadataReference = MetadataReference.CreateFromFile(file);
                    ImmutableInterlocked.TryAdd(ref _map, assemblyName, metadataReference);
                }

                if (!_controls.ContainsKey(assemblyName) && HasControls(file))
                {
                    // If it has a control, we need to eagerly load it so it'll be available for WebForms compilation
                    _context.LoadFromAssemblyPath(file);
                }
            }
        }
        catch (BadImageFormatException)
        {
            // Possibly a native assembly, so we'll ignore it
        }
    }

    private static bool HasControls(string file)
    {
        using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var peReader = new PEReader(stream);

        var reader = peReader.GetMetadataReader();
        return reader.HasAttribute<TagPrefixAttribute>();
    }

    /// <summary>
    /// The default comparer for <see cref="AssemblyName"/> is if the reference equals. This provides a comparer that compares
    /// the <see cref="AssemblyName.FullName"/> property.
    /// </summary>
    private sealed class AssemblyNameComparer : IEqualityComparer<AssemblyName>
    {
        public static AssemblyNameComparer Instance { get; } = new();

        public bool Equals(AssemblyName? x, AssemblyName? y)
            => StringComparer.OrdinalIgnoreCase.Equals(x?.FullName, y?.FullName);

        public int GetHashCode([DisallowNull] AssemblyName obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
    }
}
