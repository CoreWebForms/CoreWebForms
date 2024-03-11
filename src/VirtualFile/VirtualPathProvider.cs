// MIT License.

using System.Collections;
using System.Web.Caching;
using System.Web.Util;

namespace System.Web;

public abstract class VirtualPathProvider
{
    private VirtualPathProvider _previous;

    internal virtual void Initialize(VirtualPathProvider previous)
    {
        _previous = previous;
        Initialize();
    }

    /*
     * Initialize is called on the provider after it is registered.
     */

    protected virtual void Initialize()
    {
    }

    /*
     * Gives the provider access to the Previous provider.  It can be used to delegate some of the calls 
     * (e.g. as a way of having some files comes from the file system, and others from the database)
     */

    protected internal VirtualPathProvider Previous { get { return _previous; } }

    /*
     * Asks the provider for a hash string based on the state of a set of virtual paths.
     * The primary virtualPath is also passed in by itself.
     * If they match, the cached data held by the user of the provider is still
     * valid.  Otherwise, it should be discarded, and a new version needs to be
     * obtained via GetFile/GetDirectory.
     */

    public virtual string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
    {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
        {
            return null;
        }

        return _previous.GetFileHash(virtualPath, virtualPathDependencies);
    }

    internal string GetFileHash(VirtualPath virtualPath, IEnumerable virtualPathDependencies)
    {
        return GetFileHash(virtualPath.VirtualPathString, virtualPathDependencies);
    }

    /*
     * Asks the provider for a CacheDependency that will be invalidated if any of the
     * input files become invalid.
     * utcStart contains the time (UTC) at which the files were read.  Any change to the file
     * made after that time (even if the change is in the past) should invalidate the
     * CacheDependency.
     * If the provider doesn't support using a CacheDependency, it should return null,
     * or simply not override GetCacheDependency (the base implementation returns null).
     */

    public virtual CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
    {
        // Delegate to the previous VirtualPathProvider, if any
        if (_previous == null)
        {
            return null;
        }

        return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
    }

    internal CacheDependency GetCacheDependency(VirtualPath virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
    {
        return GetCacheDependency(virtualPath.VirtualPathString, virtualPathDependencies, utcStart);
    }

    /*
     * Returns whether the file described by the virtual path exists from
     * the point of view of this provider.
     */

    public virtual bool FileExists(string virtualPath)
    {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
        {
            return false;
        }

        return _previous.FileExists(virtualPath);
    }

    internal bool FileExists(VirtualPath virtualPath)
    {
        return FileExists(virtualPath.VirtualPathString);
    }

    /*
     * Returns whether the directory described by the virtual path exists from
     * the point of view of this provider.
     */

    public virtual bool DirectoryExists(string virtualDir)
    {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
        {
            return false;
        }

        return _previous.DirectoryExists(virtualDir);
    }

    internal bool DirectoryExists(VirtualPath virtualDir)
    {
        return DirectoryExists(virtualDir.VirtualPathString);
    }

    /*
     * Returns a VirtualFile object for the passed in virtual path
     */

    public virtual VirtualFile GetFile(string virtualPath)
    {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
        {
            return null;
        }

        return _previous.GetFile(virtualPath);
    }

    internal VirtualFile GetFile(VirtualPath virtualPath)
    {
        return GetFileWithCheck(virtualPath.VirtualPathString);
    }

    internal VirtualFile GetFileWithCheck(string virtualPath)
    {

        VirtualFile virtualFile = GetFile(virtualPath);

        if (virtualFile == null)
        {
            return null;
        }

        // Make sure the VirtualFile's path is the same as what was passed to GetFile
        if (!string.Equals(virtualPath, virtualFile.VirtualPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new HttpException($"Bad virtual path {virtualFile} in VirtuaPathBase");
        }

        return virtualFile;
    }

    /*
     * Returns a VirtualDirectory object for the passed in virtual path
     */

    public virtual VirtualDirectory GetDirectory(string virtualDir)
    {

        // Delegate to the previous VirtualPathProvider, if any

        if (_previous == null)
        {
            return null;
        }

        return _previous.GetDirectory(virtualDir);
    }

    /*
     * Allows the VirtualPathProvider to use custom logic to combine virtual path.
     * This can be used to give a special meaning to app relative paths (DevDiv 31438).
     * basePath is the path to the file in which the relative reference was found.
     */
    public virtual string CombineVirtualPaths(string basePath, string relativePath)
    {

        string baseDir = null;
        if (!String.IsNullOrEmpty(basePath))
        {
            baseDir = UrlPath2.GetDirectory(basePath);
        }

        // By default, just combine them normally
        return Path.Combine(baseDir, relativePath).Replace("\\", "/");
    }

    internal VirtualPath CombineVirtualPaths(VirtualPath basePath, VirtualPath relativePath)
    {
        string virtualPath = CombineVirtualPaths(basePath.VirtualPathString,
            relativePath.VirtualPathString);
        return VirtualPath.Create(virtualPath);
    }

    /*
     * Helper method to call CombineVirtualPaths if there is a VirtualPathProvider
     */
    internal static VirtualPath CombineVirtualPathsInternal(VirtualPath basePath, VirtualPath relativePath)
    {
        // If there is no provider, just combine them normally
        return basePath.Parent.Combine(relativePath);
    }
}
