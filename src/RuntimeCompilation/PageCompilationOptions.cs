// MIT License.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

public class PageCompilationOptions
{
    private readonly ControlCollection _controls;

    public PageCompilationOptions()
    {
        _controls = new();

        // Ensure this assembly is loaded
        _ = typeof(HttpUtility).Assembly;

        AddTypeNamespace(typeof(Page), "asp");
        AddTypeNamespace(typeof(TextBox), "asp");
    }

    public IReadOnlyCollection<Assembly> Assemblies => _controls.Assemblies;

    public void AddTypeNamespace(Type type, string prefix)
        => AddAssembly(type.Assembly, type.Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
        => _controls.Add(assembly, ns, prefix);

    internal IControlLookup Info => _controls;

    private sealed class ControlCollection : IControlLookup
    {
        private readonly Dictionary<string, Dictionary<string, ControlInfo>> _info = new();
        private readonly HashSet<Assembly> _assemblies = new();

        public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

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
            _assemblies.Add(assembly);

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
                        info.ChildProperty = parseChildren.DefaultProperty;
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

