// MIT License.

using System.ComponentModel.Design;
using System.Reflection;
using System.Web.Script;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.UI;

/// <summary>
/// This was cached before, but we're essentially delegating to the <see cref="ITypeResolutionService"/> so we can let that cache it if it wants
/// </summary>
internal static class AssemblyCache
{
    public static Assembly SystemWebExtensions { get; } = typeof(ScriptManager).Assembly;

    public static Assembly SystemWeb { get; } = typeof(Page).Assembly;

    public static Version GetVersion(Assembly assembly) => assembly.GetName().Version;

    public static Assembly Load(string assemblyName) => HttpRuntime.WebObjectActivator.GetRequiredService<ITypeResolutionService>().GetAssembly(new(assemblyName));

    public static bool IsAjaxFrameworkAssembly(Assembly assembly) => assembly.GetCustomAttributes<AjaxFrameworkAssemblyAttribute>().Any();
}
