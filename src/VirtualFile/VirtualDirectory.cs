// MIT License.

using System.Collections;

namespace System.Web;

public abstract class VirtualDirectory : VirtualFileBase
{

    /*
     * Contructs a VirtualDirectory, passing it the virtual path to the
     * directory it represents
     */

    protected VirtualDirectory(string virtualPath)
    {
        // Make sure it always has a trailing slash
        _virtualPath = System.Web.VirtualPath.CreateTrailingSlash(virtualPath);
    }

    public override bool IsDirectory
    {
        get { return true; }
    }

    /*
     * Returns an object that enumerates all the children VirtualDirectory's
     * of this directory.
     */

    public abstract IEnumerable Directories { get; }

    /*
     * Returns an object that enumerates all the children VirtualFile's
     * of this directory.
     */

    public abstract IEnumerable Files { get; }

    /*
     * Returns an object that enumerates all the children VirtualDirectory's
     * and VirtualFiles of this directory.
     */

    public abstract IEnumerable Children { get; }
}
