// MIT License.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

public class PageCompilationOptions
{
    private readonly ControlCollection _controls;

    public IFileProvider? Files { get; set; }

    internal ICollection<TagNamespaceRegisterEntry> KnownTags { get; }

    public PageCompilationOptions()
    {
        _controls = new();
        KnownTags = new List<TagNamespaceRegisterEntry>();

        // Ensure this assembly is loaded
        _ = typeof(HttpUtility).Assembly;

        AddTypeNamespace<Page>("asp");
        AddTypeNamespace<TextBox>("asp");
    }

    public void AddAssembly(Assembly assembly) => _controls.Assemblies.Add(assembly);

    public void AddAssemblyFrom<T>() => _controls.Assemblies.Add(typeof(T).Assembly);

    public IEnumerable<Assembly> Assemblies => _controls.Assemblies;

    public void AddTypeNamespace<T>(string prefix)
        where T : Control
        => AddAssembly(typeof(T).Assembly, typeof(T).Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
    {
        _controls.Add(assembly, ns, prefix);
        KnownTags.Add(new(prefix, ns, assembly.FullName));
    }

    internal IControlLookup Info => _controls;

    public bool UseFrameworkParser { get; set; }

    private sealed class ControlCollection : IControlLookup
    {
        private readonly Dictionary<string, Dictionary<string, ControlInfo>> _info = new();

        public HashSet<Assembly> Assemblies { get; } = new();

        public bool TryGetControl(string prefix, string name, [MaybeNullWhen(false)] out ControlInfo info)
        {
            if (_info.TryGetValue(prefix, out var lookup) && lookup.TryGetValue(name, out info))
            {
                return true;
            }

            info = null;
            return false;
        }

        public void Add(Assembly assembly, string ns, string prefix)
        {
            Assemblies.Add(assembly);

            if (_info.TryGetValue(prefix, out var existing))
            {
                GatherComponents(assembly, ns, existing);
            }
            else
            {
                _info.TryAdd(prefix, GatherComponents(assembly, ns, new()));
            }
        }

        private static Dictionary<string, ControlInfo> GatherComponents(Assembly assembly, string ns, Dictionary<string, ControlInfo> result)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!string.Equals(ns, type.Namespace, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var info = new ControlInfo(type.Namespace, type.Name);

                foreach (var attribute in type.GetCustomAttributes())
                {
                    if (attribute is DefaultPropertyAttribute defaultProperty)
                    {
                        info.DefaultProperty = defaultProperty.Name;
                    }

                    if (attribute is ValidationPropertyAttribute validationProperty)
                    {
                        info.ValidationProperty = validationProperty.Name;
                    }

                    if (attribute is DefaultEventAttribute defaultEvent)
                    {
                        info.DefaultEvent = defaultEvent.Name;
                    }

                    if (attribute is SupportsEventValidationAttribute)
                    {
                        info.SupportsEventValidation = true;
                    }

                    if (attribute is ParseChildrenAttribute parseChildren)
                    {
                        info.ChildrenAsProperties = parseChildren.ChildrenAsProperties;

                        if (parseChildren.DefaultProperty is { } name && type.GetProperty(name) is { } property && property.CanWrite)
                        {
                            info.ChildProperty = parseChildren.DefaultProperty;
                        }
                    }
                }

                foreach (var property in type.GetProperties())
                {
                    if (property.PropertyType.IsAssignableTo(typeof(string)))
                    {
                        info.AddProperty(property.Name, DataType.String);
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(ITemplate)))
                    {
                        info.AddProperty(property.Name, DataType.Template);
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(System.Collections.ICollection)))
                    {
                        info.AddProperty(property.Name, DataType.Collection);
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        info.AddEnum(property.Name, property.PropertyType);
                    }
                    else
                    {
                        info.AddProperty(property.Name, DataType.NoQuotes);
                    }
                }

                foreach (var @event in type.GetEvents())
                {
                    info.AddProperty(@event.Name, DataType.Delegate);
                }

                result.TryAdd(info.Name, info);
            }

            return result;
        }
    }
}

