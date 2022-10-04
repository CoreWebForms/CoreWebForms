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

    public void AddTypeNamespace(Type type, string prefix)
        => AddAssembly(type.Assembly, type.Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
        => _controls.Add(assembly, ns, prefix);

    internal IControlLookup Info => _controls;

    private sealed class ControlCollection : IControlLookup
    {
        private readonly Dictionary<string, Dictionary<string, ControlInfo>> _assemblies = new();

        public bool TryGetControl(string prefix, string name, [MaybeNullWhen(false)] out ControlInfo info)
        {
            if (_assemblies.TryGetValue(prefix, out var lookup) && lookup.TryGetValue(name, out info))
            {
                return true;
            }

            info = null;
            return false;
        }

        public void Add(Assembly assembly, string ns, string prefix)
        {
            if (_assemblies.TryGetValue(prefix, out var existing))
            {
                GatherComponents(assembly, ns, existing);
            }
            else
            {
                _assemblies.TryAdd(prefix, GatherComponents(assembly, ns, new()));
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
                    }
                }

                foreach (var property in type.GetProperties())
                {
                    if (property.SetMethod is { IsPublic: true } && property.GetCustomAttribute<DefaultValueAttribute>() is { })
                    {
                        if (property.PropertyType.IsAssignableTo(typeof(Delegate)))
                        {
                            info.Events.Add(property.Name);
                        }
                        else if (property.PropertyType.IsAssignableTo(typeof(string)))
                        {
                            info.Strings.Add(property.Name);
                        }
                        else
                        {
                            info.Other.Add(property.Name);
                        }
                    }
                }

                result.TryAdd(info.Name, info);
            }

            return result;
        }
    }
}

