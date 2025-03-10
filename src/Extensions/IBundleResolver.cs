// MIT License.

namespace WebForms.Extensions;

// An internal implementation similar to what is in the System.Web.Optimization
internal interface IBundleResolver
{
    bool IsBundleVirtualPath(string virtualPath);

    IEnumerable<string> GetBundleContents(string virtualPath);

    string GetBundleUrl(string virtualPath);
}
