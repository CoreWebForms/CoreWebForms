// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;

namespace System.Web;
internal sealed class VirtualPath : IComparable
{
    private string _appRelativeVirtualPath;
    private string _virtualPath;

    // const masks into the BitVector32
    private const int isWithinAppRootComputed = 0x00000001;
    private const int isWithinAppRoot = 0x00000002;
    private const int appRelativeAttempted = 0x00000004;

#pragma warning disable 0649
    private SimpleBitVector32 flags;
#pragma warning restore 0649


    internal static VirtualPath RootVirtualPath = VirtualPath.Create("/");

    private VirtualPath() { }

    int IComparable.CompareTo(object obj)
    {

        VirtualPath virtualPath = obj as VirtualPath;

        // Make sure we're compared to another VirtualPath
        if (virtualPath == null)
            throw new ArgumentException();

        // Check if it's the same object
        if (virtualPath == this)
            return 0;

        return StringComparer.InvariantCultureIgnoreCase.Compare(
            this.VirtualPathString, virtualPath.VirtualPathString);
    }

    public static VirtualPath Create(string virtualPath)
    {
        return Create(virtualPath, VirtualPathOptions.AllowAllPath);
    }

    public string VirtualPathString
    {
        get
        {
            if (_virtualPath == null)
            {
                Debug.Assert(_appRelativeVirtualPath != null);

                // todo app domain
                //// This is not valid if we don't know the app path
                //if (HttpRuntime.AppDomainAppVirtualPathObject == null)
                //{
                //    throw new HttpException(SR.GetString(SR.VirtualPath_CantMakeAppAbsolute,
                //        _appRelativeVirtualPath));
                //}

                if (_appRelativeVirtualPath.Length == 1)
                {
                    _virtualPath = HttpRuntime.AppDomainAppVirtualPath;
                }
                else
                {
                    // todo app domain
                    //_virtualPath = HttpRuntime.AppDomainAppVirtualPathString +
                    //    _appRelativeVirtualPath.Substring(2);
                }
            }

            return _virtualPath;
        }
    }
    public static VirtualPath Create(string virtualPath, VirtualPathOptions options)
    {

        // Trim it first, so that blank strings (e.g. "  ") get treated as empty
        if (virtualPath != null)
            virtualPath = virtualPath.Trim();

        // If it's empty, check whether we allow it
        if (String.IsNullOrEmpty(virtualPath))
        {
            if ((options & VirtualPathOptions.AllowNull) != 0)
                return null;

            throw new ArgumentNullException("virtualPath");
        }

        // Dev10 767308: optimize for normal paths, and scan once for
        //     i) invalid chars
        //    ii) slashes
        //   iii) '.'

        bool slashes = false;
        bool dot = false;
        int len = virtualPath.Length;

        // todo migrate unsafe code
        //unsafe
        //{
        //    fixed (char* p = virtualPath)
        //    {
        //        for (int i = 0; i < len; i++)
        //        {
        //            switch (p[i])
        //            {
        //                // need to fix slashes ?
        //                case '/':
        //                    if (i > 0 && p[i - 1] == '/')
        //                        slashes = true;
        //                    break;
        //                case '\\':
        //                    slashes = true;
        //                    break;
        //                // contains "." or ".."
        //                case '.':
        //                    dot = true;
        //                    break;
        //                // invalid chars
        //                case '\0':
        //                    throw new HttpException(SR.GetString(SR.Invalid_vpath, virtualPath));
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //}

        if (slashes)
        {
            // If we're supposed to fail on malformed path, then throw
            if ((options & VirtualPathOptions.FailIfMalformed) != 0)
            {
                throw new HttpException(SR.GetString(SR.Invalid_vpath, virtualPath));
            }
            // Flip ----lashes, and remove duplicate slashes                
            virtualPath = UrlPath.FixVirtualPathSlashes(virtualPath);
        }

        // Make sure it ends with a trailing slash if requested
        if ((options & VirtualPathOptions.EnsureTrailingSlash) != 0)
            virtualPath = UrlPath.AppendSlashToPathIfNeeded(virtualPath);

        VirtualPath virtualPathObject = new VirtualPath();

        if (UrlPath.IsAppRelativePath(virtualPath))
        {

            if (dot)
                virtualPath = UrlPath.ReduceVirtualPath(virtualPath);

            if (virtualPath[0] == UrlPath.appRelativeCharacter)
            {
                if ((options & VirtualPathOptions.AllowAppRelativePath) == 0)
                {
                    throw new ArgumentException(SR.GetString(SR.VirtualPath_AllowAppRelativePath, virtualPath));
                }

                virtualPathObject._appRelativeVirtualPath = virtualPath;
            }
            else
            {
                // It's possible for the path to become absolute after calling Reduce,
                // even though it started with "~/".  e.g. if the app is "/app" and the path is
                // "~/../hello.aspx", it becomes "/hello.aspx", which is absolute

                if ((options & VirtualPathOptions.AllowAbsolutePath) == 0)
                {
                    throw new ArgumentException(SR.GetString(SR.VirtualPath_AllowAbsolutePath, virtualPath));
                }

                virtualPathObject._virtualPath = virtualPath;
            }
        }
        else
        {
            if (virtualPath[0] != '/')
            {
                if ((options & VirtualPathOptions.AllowRelativePath) == 0)
                {
                    throw new ArgumentException(SR.GetString(SR.VirtualPath_AllowRelativePath, virtualPath));
                }

                // Don't Reduce relative paths, since the Reduce method is broken (e.g. "../foo.aspx" --> "/foo.aspx!")
                // 
                virtualPathObject._virtualPath = virtualPath;
            }
            else
            {
                if ((options & VirtualPathOptions.AllowAbsolutePath) == 0)
                {
                    throw new ArgumentException(SR.GetString(SR.VirtualPath_AllowAbsolutePath, virtualPath));
                }

                if (dot)
                    virtualPath = UrlPath.ReduceVirtualPath(virtualPath);

                virtualPathObject._virtualPath = virtualPath;
            }
        }
#if DBG
            virtualPathObject.ValidateState();
#endif
        return virtualPathObject;
    }
}

[Flags]
internal enum VirtualPathOptions
{
    AllowNull = 0x00000001,
    EnsureTrailingSlash = 0x00000002,
    AllowAbsolutePath = 0x00000004,
    AllowAppRelativePath = 0x00000008,
    AllowRelativePath = 0x00000010,
    FailIfMalformed = 0x00000020,

    AllowAllPath = AllowAbsolutePath | AllowAppRelativePath | AllowRelativePath,
}
