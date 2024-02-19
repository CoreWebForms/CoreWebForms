// MIT License.

using System.Security;

namespace System.Web;

public class HttpRuntimeConsts
{
    internal const string codegenDirName = "Temporary ASP.NET Files";
    internal const string profileFileName = "profileoptimization.prof";

    private static HttpRuntimeConsts _theRuntime;   // single instance of the class
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

    private NamedPermissionSet _namedPermissionSet;
    internal static NamedPermissionSet NamedPermissionSet {
        get {
            // Make sure we have already initialized the trust level
            //


            return _theRuntime._namedPermissionSet;
        }
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
}
