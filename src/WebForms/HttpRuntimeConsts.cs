// MIT License.

using System.Security;

namespace System.Web;

public static class HttpRuntimeConsts
{
    internal const string codegenDirName = "Temporary ASP.NET Files";
    internal const string profileFileName = "profileoptimization.prof";

    internal static byte[] s_autogenKeys = new byte[1024];

    //
    // Names of special ASP.NET directories
    //

    internal const string BinDirectoryName = "bin";
    internal const string CodeDirectoryName = "App_Code";
    internal const string WebRefDirectoryName = "App_WebReferences";
    internal const string ResourcesDirectoryName = "App_GlobalResources";
    internal const string LocalResourcesDirectoryName = "App_LocalResources";
    internal const string DataDirectoryName = "App_Data";
    internal const string ThemesDirectoryName = "App_Themes";
    internal const string GlobalThemesDirectoryName = "Themes";
    internal const string BrowsersDirectoryName = "App_Browsers";

    private static string DirectorySeparatorString = new string(Path.DirectorySeparatorChar, 1);
    private static string DoubleDirectorySeparatorString = new string(Path.DirectorySeparatorChar, 2);
    private static char[] s_InvalidPhysicalPathChars = { '/', '?', '*', '<', '>', '|', '"' };

    private static NamedPermissionSet _namedPermissionSet;
    internal static NamedPermissionSet NamedPermissionSet {
        get {
            // Make sure we have already initialized the trust level
            //


            return _namedPermissionSet;
        }
    }



    //
    // App domain related
    //

    private static String _tempDir;
    private static String _codegenDir;
    private static String _appDomainAppId;
    private static String _appDomainAppPath;
    private static VirtualPath _appDomainAppVPath;
    private static String _appDomainId;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static String CodegenDir {
        get {
            String path = CodegenDirInternal;
            // Todo : Migration
            //InternalSecurityPermissions.PathDiscovery(path).Demand();
            return path;
        }
    }

    internal static string CodegenDirInternal {
        get
        {
            if (_codegenDir == null)
            {
                _codegenDir = Thread.GetDomain().DynamicDirectory ?? throw new NullReferenceException("Thread.GetDomain().DynamicDirectory is null");
                Directory.CreateDirectory(_codegenDir);
            }
            return _codegenDir;
        }
    }

    internal static string TempDirInternal {
        get { return _tempDir; }
    }


    internal static bool HasAspNetHostingPermission(AspNetHostingPermissionLevel level) {

        // Make sure we have already initialized the trust level
        //



        // If we don't have a NamedPermissionSet, we're in full trust
        if (NamedPermissionSet == null)
            return true;

        AspNetHostingPermission permission = (AspNetHostingPermission)NamedPermissionSet.GetPermission(
            typeof(AspNetHostingPermission));
        if (permission == null)
            return false;

        return (permission.Level >= level);
    }


    internal static VirtualPath CodeDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(CodeDirectoryName); }
    }

    internal static VirtualPath ResourcesDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(ResourcesDirectoryName); }
    }

    internal static VirtualPath WebRefDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(WebRefDirectoryName); }
    }

}
