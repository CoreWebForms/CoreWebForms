// MIT License.

using System.Collections.Generic;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Generator;

internal class SourceGeneratorLookup : IControlLookup
{
    private readonly Dictionary<(string, string), ControlInfo> _controls = new();

    public void Add(ControlInfo info)
        => _controls.Add((info.Namespace, info.Name), info);

    bool IControlLookup.TryGetControl(string prefix, string name, out ControlInfo info)
    {
        // Todo: support more
        if (prefix == "asp")
        {
            return _controls.TryGetValue(("System.Web.UI", name), out info);
        }

        info = null!;
        return false;
    }
}

