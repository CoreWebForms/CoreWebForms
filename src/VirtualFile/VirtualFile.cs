// MIT License.

namespace System.Web;

public abstract class VirtualFile : VirtualFileBase
{

    /*
     * Contructs a VirtualFile, passing it the virtual path to the
     * file it represents
     */

    protected VirtualFile(string virtualPath)
    {
        _virtualPath = System.Web.VirtualPath.Create(virtualPath);
    }


    public override bool IsDirectory
    {
        get { return false; }
    }

    /*
     * Returns a readonly stream to the file
     */

    public abstract Stream Open();
}
