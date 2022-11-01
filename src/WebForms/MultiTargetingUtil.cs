// MIT License.

namespace System.Web;

internal class MultiTargetingUtil
{
    public const bool IsTargetFramework40OrAbove = true;

    public static Version TargetFrameworkVersion { get; } = new Version(4, 8);
}
