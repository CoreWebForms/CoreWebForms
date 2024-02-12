// MIT License.

namespace System.Web;

public abstract class VirtualFileBase
{
    internal VirtualPath _virtualPath;

    /*
     * Returns the name of the file or directory, without any path info.
     * e.g. if the virtual path is /app/sub/foo.aspx, this returns foo.aspx.
     * Note that this is expected to return the name in the correct casing,
     * which may be different from the casing in the original virtual path.
     */

    public virtual string Name
    {
        get
        {
            // By default, return the last chunk of the virtual path
            return _virtualPath.FileName;
        }
    }

    /*
     * Returns the virtual path to the file or directory that this object
     * represents.  This is typically the path passed in to the constructor.
     */

    public string VirtualPath
    {
        get { return _virtualPath.VirtualPathString; }
    }

    /*
     * Returns true if this is a directory, and false if its a file
     */

    public abstract bool IsDirectory { get; }
}
