// MIT License.

using System.Runtime.Versioning;
using System.Web.UI;

namespace System.Web;

internal class MultiTargetingUtil
{
    public const bool IsTargetFramework40OrAbove = true;

    public static Version TargetFrameworkVersion { get; } = new Version(4, 8);

    public static FrameworkName TargetFrameworkName { get; } = new FrameworkName(".NETFramework", TargetFrameworkVersion);

    internal static bool EnableReferenceAssemblyResolution {
        get {
            return BuildManagerHost.InClientBuildManager; // Enable only in CBM scenarios.
        }
    }
}
