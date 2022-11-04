// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public interface IControlLookup
{
    bool TryGetControl(string prefix, string name, out ControlInfo info);
}
