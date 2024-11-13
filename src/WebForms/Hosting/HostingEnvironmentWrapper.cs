// MIT License.

using System.Web.Util;

namespace System.Web.Hosting;

public static class HostingEnvironmentWrapper
{
    private static string MapPathActual(VirtualPath virtualPath, bool permitNull)
        {
            string result = null;

            Debug.Assert(virtualPath != null);

            virtualPath.FailIfRelativePath();

            VirtualPath reqpath = virtualPath;

            // if (String.CompareOrdinal(reqpath.VirtualPathString, _appVirtualPath.VirtualPathString) == 0) {
            //     // for application path don't need to call app host
            //     Debug.Trace("MapPath", reqpath  +" is the app path");
            //     result = _appPhysicalPath;
            // }
            // else {
            //     // using (new ProcessImpersonationContext()) {
            //         // If there is a mapping for this virtual path in the call context, use it
            //         result = GetVirtualPathToFileMapping(reqpath);
            //
            //         if (result == null) {
            //             // call host's mappath
            //             if (_configMapPath == null) {
            //                 Debug.Trace("MapPath", "Missing _configMapPath");
            //                 throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, reqpath));
            //             }
            //             Debug.Trace("MapPath", "call ConfigMapPath (" + reqpath + ")");
            //
            //             // see if the IConfigMapPath provider implements the interface
            //             // with VirtualPath
            //             try {
            //                 if (null != _configMapPath2) {
            //                     result = _configMapPath2.MapPath(GetSiteID(), reqpath);
            //                 }
            //                 else {
            //                     result = _configMapPath.MapPath(GetSiteID(), reqpath.VirtualPathString);
            //                 }
            //                 if (HttpRuntime.IsMapPathRelaxed)
            //                     result = HttpRuntime.GetRelaxedMapPathResult(result);
            //             } catch {
            //                 if (HttpRuntime.IsMapPathRelaxed)
            //                     result = HttpRuntime.GetRelaxedMapPathResult(null);
            //                 else
            //                     throw;
            //             }
            //         }
            //     // }
            // }
            //
            // if (String.IsNullOrEmpty(result)) {
            //     Debug.Trace("MapPath", "null Result");
            //     if (!permitNull) {
            //         if (HttpRuntime.IsMapPathRelaxed)
            //             result = HttpRuntime.GetRelaxedMapPathResult(null);
            //         else
            //             throw new InvalidOperationException(SR.GetString(SR.Cannot_map_path, reqpath));
            //     }
            // }
            // else {
            //     // ensure extra '\\' in the physical path if the virtual path had extra '/'
            //     // and the other way -- no extra '\\' in physical if virtual didn't have it.
            //     if (virtualPath.HasTrailingSlash) {
            //         if (!UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
            //             result = result + "\\";
            //     }
            //     else {
            //         if (UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
            //             result = result.Substring(0, result.Length - 1);
            //     }
            //
            //     Debug.Write("MapPath", "    result=" + result);
            // }

            return result;
        }

    internal static string MapPath(VirtualPath virtualPath) {

        String path = MapPathInternal(virtualPath);

        // if (path != null)
        //     InternalSecurityPermissions.PathDiscovery(path).Demand();

        return path;
    }

    internal static String MapPathInternal(string virtualPath) {
        return MapPathInternal(VirtualPath.Create(virtualPath));
    }

    internal static String MapPathInternal(VirtualPath virtualPath) {
        return HostingEnvironmentWrapper.MapPathActual(virtualPath, false);
    }

    internal static String MapPathInternal(string virtualPath, bool permitNull) {
        return MapPathInternal(VirtualPath.Create(virtualPath), permitNull);
    }

    internal static String MapPathInternal(VirtualPath virtualPath, bool permitNull) {
        return HostingEnvironmentWrapper.MapPathActual(virtualPath, permitNull);
    }

    internal static string MapPathInternal(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
        return MapPathInternal(VirtualPath.Create(virtualPath),
            VirtualPath.CreateNonRelative(baseVirtualDir), allowCrossAppMapping);
    }

    internal static string MapPathInternal(VirtualPath virtualPath, VirtualPath baseVirtualDir, bool allowCrossAppMapping) {
        Debug.Assert(baseVirtualDir != null, "baseVirtualDir != null");

        // Combine it with the base and reduce
        virtualPath = baseVirtualDir.Combine(virtualPath);

        // TODO: Migration
        // if (!allowCrossAppMapping && !virtualPath.IsWithinAppRoot)
        //     throw new ArgumentException(SR.GetString(SR.Cross_app_not_allowed, virtualPath));

        return MapPathInternal(virtualPath);
    }
}
