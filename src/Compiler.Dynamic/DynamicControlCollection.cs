// MIT License.

using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Web;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicControlCollection : ITypeResolutionService, IMetadataProvider, IDisposable
{
    private readonly AssemblyLoadContext _context;
    private readonly ILogger<DynamicControlCollection> _logger;

    private ImmutableDictionary<AssemblyName, Assembly> _controls = ImmutableDictionary<AssemblyName, Assembly>.Empty.WithComparers(AssemblyNameComparer.Instance);
    private ImmutableDictionary<AssemblyName, MetadataReference> _metadataReferences = ImmutableDictionary<AssemblyName, MetadataReference>.Empty.WithComparers(AssemblyNameComparer.Instance);

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
            SearchForControls(assembly);
        }

        foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            LoadMetadataReference(file);
        }
    }

    private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        => SearchForControls(args.LoadedAssembly);

    IEnumerable<MetadataReference> IMetadataProvider.References => _metadataReferences.Values;

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
        return GetType(name, throwOnError, ignoreCase: false);
    }

    private static Type? GetType(string typeName, bool throwOnError, bool ignoreCase)
    {
        Type? type = null;
        if (Util.TypeNameContainsAssembly(typeName))
        {
            type = Type.GetType(typeName, throwOnError, ignoreCase);

            if (type != null)
            {
                return type;
            }
        }

        if(type == null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) //start with Web related assemblies
            {
                var tt = assembly.GetType(typeName, false);
                if (tt != null)
                {
                    return tt;
                }
            }
        }
        if (type == null && throwOnError)
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_type, typeName));
        }
        return null;
    }

    Type? ITypeResolutionService.GetType(string name, bool throwOnError, bool ignoreCase)
    {
        return GetType(name, throwOnError, ignoreCase);
    }

    void ITypeResolutionService.ReferenceAssembly(AssemblyName name)
    {
        throw new NotImplementedException();
    }

    private void SearchForControls(Assembly assembly)
    {
        var assemblyName = assembly.GetName();

        try
        {
            _logger.LogTrace("Searching {Assembly} for tag prefixes", assembly.FullName);

            if (assembly.GetCustomAttributes<TagPrefixAttribute>().Any())
            {
                _logger.LogInformation("Found tag prefixes in {Assembly}", assembly.FullName);
                ImmutableInterlocked.TryAdd(ref _controls, assemblyName, assembly);
            }

            if (assembly is { Location: { Length: > 0 } location } && !_metadataReferences.ContainsKey(assemblyName))
            {
                _logger.LogTrace("Loading loaded {AssemblyName} for dynamic compilations", assemblyName);
                ImmutableInterlocked.TryAdd(ref _metadataReferences, assemblyName, MetadataReference.CreateFromFile(location));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "There was an unexpected error trying to search for controls for {AssemblyName}", assemblyName);
        }
    }

    private void LoadMetadataReference(string file)
    {
        try
        {
            if (MetadataReader.GetAssemblyName(file) is { } assemblyName)
            {
                // Controls must be loaded into the runtime due to WebForms compilation usage
                if (!_controls.ContainsKey(assemblyName) && HasControls(file))
                {
                    _logger.LogTrace("Detected WebForms controls in {AssemblyName}. It will be loaded so it's available for use in WebForms compilations.", assemblyName);

                    // If it has a control, we need to eagerly load it so it'll be available for WebForms compilation
                    // Once loaded, it will trigger the event that will search for controls
                    _context.LoadFromAssemblyPath(file);
                }

                // Other libraries don't need to be loaded, but we need to have the metadata reference in order to compile pages/controls/etc
                else if (!_metadataReferences.ContainsKey(assemblyName))
                {
                    _logger.LogTrace("Registering {AssemblyName} for WebForms compilations", assemblyName);

                    ImmutableInterlocked.TryAdd(ref _metadataReferences, assemblyName, MetadataReference.CreateFromFile(file));
                }
            }
        }

        // Possibly a native assembly, so we'll ignore it
        catch (BadImageFormatException)
        {
            _logger.LogTrace("Attempted to load {Path} for WebForms compilation reference assemblies but it was an invalid image", file);
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
