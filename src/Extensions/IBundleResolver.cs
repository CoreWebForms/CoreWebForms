// MIT License.

namespace WebForms.Extensions;

// An internal implementation similar to what is in the optimization
internal interface IBundleResolver
{
    /// <summary>
    /// Determines whether a virutal path addresses a bundle.
    /// </summary>
    /// <param name="virtualPath">The requested virtual path.</param>
    /// <returns>True if the virtual path addresses a bundle.</returns>
    bool IsBundleVirtualPath(string virtualPath);

    /// <summary>
    /// Gets the paths to files included in a bundle. 
    /// </summary>
    /// <param name="virtualPath">The requested virtual path.</param>
    /// <returns>A list of all the virtual paths for the files contained in the bundle.</returns>
    IEnumerable<string> GetBundleContents(string virtualPath);

    /// <summary>
    /// Gets a versioned bundle url.
    /// </summary>
    /// <param name="virtualPath">The requested virtual path.</param>
    /// <returns>The versioned url for a bundle.</returns>
    /// <remarks>
    /// As an exmple, GetBundleUrl("~/bundle") will, assuming that ~/bundle addresses a valid bundle, yield the path string "~/bundle?v=149872359237"
    /// </remarks>
    string GetBundleUrl(string virtualPath);
}
