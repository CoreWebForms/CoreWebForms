// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI;

internal class BuildManagerHost
{
    public static bool InClientBuildManager { get; internal set; }
}

internal class ResourceExpressionBuilder
{
    internal static IResourceProvider GetLocalResourceProvider(VirtualPath virtualPath)
    {
        throw new NotImplementedException("GetLocalResourceProvider");
    }

    internal static object ParseExpression(string fullResourceKey)
    {
        throw new NotImplementedException("ParseExpression");
    }

    internal static object GetGlobalResourceObject(string classKey, string resourceKey)
    {
        return GetGlobalResourceObject(classKey, resourceKey, null /*objType*/, null /*propName*/, null /*culture*/);
    }

    internal static object GetGlobalResourceObject(string classKey,
        string resourceKey, Type objType, string propName, CultureInfo culture)
    {

        IResourceProvider resourceProvider = GetGlobalResourceProvider(classKey);
        return GetResourceObject(resourceProvider, resourceKey, culture,
            objType, propName);
    }

    private static IResourceProvider GetGlobalResourceProvider(string classKey)
    {
        throw new NotImplementedException("GetGlobalResourceProvider");
    }

    internal static object GetResourceObject(IResourceProvider resourceProvider,
        string resourceKey, CultureInfo culture)
    {
        return GetResourceObject(resourceProvider, resourceKey, culture,
            null /*objType*/, null /*propName*/);
    }

    internal static object GetResourceObject(IResourceProvider resourceProvider,
        string resourceKey, CultureInfo culture, Type objType, string propName)
    {

        if (resourceProvider == null)
            return null;

        object o = resourceProvider.GetObject(resourceKey, culture);

        // If no objType/propName was provided, return the object as is
        if (objType == null)
            return o;

        // Also, if the object from the resource is not a string, return it as is
        string s = o as String;
        if (s == null)
            return o;

        // If they were provided, perform the appropriate conversion
        return ObjectFromString(s, objType, propName);
    }

    private static object ObjectFromString(string value, Type objType, string propName)
    {

        // Get the PropertyDescriptor for the property
        PropertyDescriptor pd = TypeDescriptor.GetProperties(objType)[propName];
        Debug.Assert(pd != null);
        if (pd == null) return null;

        // Get its type descriptor
        TypeConverter converter = pd.Converter;
        Debug.Assert(converter != null);
        if (converter == null) return null;

        // Perform the conversion
        return converter.ConvertFromInvariantString(value);
    }
}

internal class BaseResourcesBuildProvider : BuildProvider
{
    public static string DefaultResourcesNamespace { get; internal set; }

    public void DontGenerateStronglyTypedClass()
    {
        throw new NotImplementedException();
    }
}

internal static class BuildManager
{
    // All generated assemblies start with this prefix
    internal const string AssemblyNamePrefix = "App_";

    // Web assemblies are the assemblies generated from web files (aspx, ascx, ...)
    internal const string WebAssemblyNamePrefix = AssemblyNamePrefix + "Web_";

    internal const string AppThemeAssemblyNamePrefix = AssemblyNamePrefix + "Theme_";
    internal const string GlobalThemeAssemblyNamePrefix = AssemblyNamePrefix + "GlobalTheme_";
    internal const string AppBrowserCapAssemblyNamePrefix = AssemblyNamePrefix + "Browsers";


    private static BuildResultCache[] _caches = new BuildResultCache[] {
        _memoryCache,
        _codeGenCache,
    };
    private static StandardDiskBuildResultCache _codeGenCache = new StandardDiskBuildResultCache(HttpRuntimeConsts.CodegenDirInternal);
    private static MemoryBuildResultCache _memoryCache = new MemoryBuildResultCache();

    private static CompilationStage _compilationStage = CompilationStage.PreTopLevelFiles;

    private static object _lock = new();


    internal static bool OptimizeCompilations { get; set; }

    public static bool PrecompilingForDeployment { get; internal set; }
    public static string UpdatableInheritReplacementToken { get; internal set; }
    public static Assembly AppResourcesAssembly { get; internal set; }
    public static bool PrecompilingForUpdatableDeployment { get; internal set; }
    public static bool ThrowOnFirstParseError { get; set; } = true;

    public static VirtualPath ScriptVirtualDir = Util.GetScriptLocation();

    public static bool IsPrecompiledApp { get; set;  }

    // TODO: Migration
    public static string WebHashFilePath { get; set; } = "Web_Hash.webinfo";

    internal static BuildResult GetBuildResultFromCache(string cacheKey) {
        return GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, null /*virtualPath*/,
            0 /*hashCode*/);
    }

    internal static BuildResult GetBuildResultFromCache(string cacheKey, VirtualPath virtualPath) {
        return GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, virtualPath,
            0 /*hashCode*/);
    }

    public static void CacheBuildResult(string cacheKey, BuildResult result, DateTime utcStart)
    {
        throw new NotImplementedException();
    }

    private static BuildResult GetBuildResultFromCacheInternal(string cacheKey, bool keyFromVPP,
        VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate = true)
    {

        BuildResult result = null;

        // Allow the possibility that BuildManager was not initialized due to
        // a very early failure (e.g. see VSWhidbey 137366)
        //Debug.Trace("BuildManager", "GetBuildResultFromCacheInternal " + _theBuildManagerInitialized);
        // if (!_theBuildManagerInitialized)
        //     return null;

        // The first cache should always be memory
        Debug.Assert(_caches[0] == _memoryCache);

        // Try to get it from the memeory cache before taking any locks (for perf reasons)
        result = _memoryCache.GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);
        if (result != null)
        {
            return PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
        }

        Debug.Write("BuildManager", "Didn't find '" + virtualPath + "' in memory cache before lock");

        lock (_lock)
        {
            // Try to get the BuildResult from the cheapest to most expensive cache
            int i;
            for (i = 0; i < _caches.Length; i++)
            {
                result = _caches[i].GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);

                // There might be changes in local resources for dependencies,
                // so we need to make sure EnsureFirstTimeDirectoryInit gets called
                // for them even when we already have a cache result.
                // VSWhidbey Bug 560521

                if (result != null)
                {
                    // We should only process the local resources folder after the top level files have been compiled,
                    // so that any custom VPP can be registered first. (Dev10 bug 890796)
                    if (_compilationStage == CompilationStage.AfterTopLevelFiles &&
                        result.VirtualPathDependencies != null)
                    {
                        EnsureFirstTimeDirectoryInitForDependencies(result.VirtualPathDependencies);
                    }

                    break;
                }

                // If we didn't find it in the memory cache, perform the per directory
                // initialization.  This is a good place to do this, because we don't
                // affect the memory cache code path, but we do the init as soon as
                // something is not found in the memory cache.
                if (i == 0 && virtualPath != null)
                {
                    VirtualPath virtualDir = virtualPath.Parent;
                    EnsureFirstTimeDirectoryInit(virtualDir);
                }
            }


            if (result == null)
                return null;

            result = PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
            if (result == null)
                return null;

            Debug.Assert(_memoryCache != null);

            // If we found it in a cache, cache it in all the caches that come before
            // the one where we found it.  If we found it in the memory cache, this is a no op.
            for (int j = 0; j < i; j++)
                _caches[j].CacheBuildResult(cacheKey, result, DateTime.UtcNow);

            Debug.Write("BuildManager", "Found '" + virtualPath + "' in " + _caches[i]);

            return result;
        }
    }

    private static Hashtable _localResourcesAssemblies = new Hashtable();

    private static void EnsureFirstTimeDirectoryInit(VirtualPath virtualDir) {

            // Don't process local resources when precompiling for updatable deployment.
            // Instead, we deploy the App_LocalResources folder as is.
            if (PrecompilingForUpdatableDeployment)
                return;

            if (virtualDir == null)
                return;

            // Only do this once per directory
            if (_localResourcesAssemblies.Contains(virtualDir))
                return;

            // Don't do anything if it's outside the app root
            // TODO: Migration
            // if (!virtualDir.IsWithinAppRoot)
            //     return;

            Debug.Write("BuildManager", "EnsureFirstTimeDirectoryInit(" + virtualDir + ")");

            // Get the virtual path to the LocalResources subdirectory for this directory
            VirtualPath localResDir = virtualDir.Combine(HttpRuntimeConsts.LocalResourcesDirectoryName);

            bool dirExists;
            try {
                dirExists = localResDir.DirectoryExists();
            }
            catch {
                // If an exception happens, the directory may be outside the application,
                // in which case we should skip this logic, and act is if there are no
                // local resources (VSWhidbey 258776);

                _localResourcesAssemblies[virtualDir] = null;
                return;
            }

            Debug.Write("BuildManager", "EnsureFirstTimeDirectoryInit: dirExists=" + dirExists);

            // TODO: Migration
            // try {
            //     // Monitor changes to it so the appdomain can shut down when it changes
            //     HttpRuntime.StartListeningToLocalResourcesDirectory(localResDir);
            // }
            // catch {
            //     // could fail for long directory names
            //     if (dirExists) {
            //         throw;
            //     }
            // }

            Assembly resourceAssembly = null;

            // If it exists, build it
            if (dirExists) {

                string localResAssemblyName = GetLocalResourcesAssemblyName(virtualDir);

                bool gotLock = false;

                try {
                    // Grab the compilation mutex, since this method accesses the codegen files
                    // TODO: Migration
                    // CompilationLock.GetLock(ref gotLock);

                    resourceAssembly = CompileCodeDirectory(localResDir, CodeDirectoryType.LocalResources,
                        localResAssemblyName, null /*excludedSubdirectories*/);
                }
                finally {
                    // Always release the mutex if we had taken it
                    // TODO: Migration
                    // if (gotLock) {
                    //     CompilationLock.ReleaseLock();
                    // }
                }
            }

            // Cache it whether it's null or not
            _localResourcesAssemblies[virtualDir] = resourceAssembly;
        }

    internal static void ReportDirectoryCompilationProgress(VirtualPath virtualDir) {
        // TODO: Migration
        // Nothing to do if there is no CBM callback
        // ClientBuildManagerCallback callback = CBMCallback;
        // if (callback == null)
        //     return;
        //
        // // Don't report anything if the directory doesn't exist
        // if (!virtualDir.DirectoryExists())
        //     return;
        //
        // string message = SR.GetString(SR.Directory_progress, virtualDir.VirtualPathString);
        // callback.ReportProgress(message);
    }

    private static List<Assembly> _topLevelReferencedAssemblies = new List<Assembly>() {
        typeof(HttpRuntime).Assembly,
        typeof(System.ComponentModel.Component).Assembly,
    };


    private static ArrayList _codeAssemblies;
    // TODO: Migration
    // public static IList CodeAssemblies {
    //     get {
    //         EnsureTopLevelFilesCompiled();
    //         return _codeAssemblies;
    //     }
    // }
    private static IDictionary _assemblyResolveMapping;

    private static Dictionary<String, AssemblyReferenceInfo> _topLevelAssembliesIndexTable;
    private static IDictionary<String, AssemblyReferenceInfo> TopLevelAssembliesIndexTable { get { return _topLevelAssembliesIndexTable; } }

    private static Assembly CompileCodeDirectory(VirtualPath virtualDir, CodeDirectoryType dirType,
            string assemblyName, StringSet excludedSubdirectories) {

            Debug.Write("BuildManager", "CompileCodeDirectory(" + virtualDir.VirtualPathString + ")");

            bool isDirectoryAllowed = true;
            if (IsPrecompiledApp) {
                // Most special dirs are not allowed in precompiled apps.  App_LocalResources is
                // an exception, as it is allowed in updatable precompiled apps.
                // TODO: Migration
                // if (IsUpdatablePrecompiledAppInternal && dirType == CodeDirectoryType.LocalResources)
                if (dirType == CodeDirectoryType.LocalResources)
                    isDirectoryAllowed = true;
                else
                    isDirectoryAllowed = false;
            }

            // Remember the referenced assemblies based on the current count.
            AssemblyReferenceInfo info = new AssemblyReferenceInfo(_topLevelReferencedAssemblies.Count);
            _topLevelAssembliesIndexTable[virtualDir.VirtualPathString] = info;

            Assembly codeAssembly = CodeDirectoryCompiler.GetCodeDirectoryAssembly(
                    virtualDir, dirType, assemblyName, excludedSubdirectories,
                    isDirectoryAllowed);

            if (codeAssembly != null) {

                // Remember the generated assembly
                info.Assembly = codeAssembly;

                // Page resource assemblies are not added to the top level list
                if (dirType != CodeDirectoryType.LocalResources) {
                    _topLevelReferencedAssemblies.Add(codeAssembly);

                    if (dirType == CodeDirectoryType.MainCode || dirType == CodeDirectoryType.SubCode) {
                        if (_codeAssemblies == null) {
                            _codeAssemblies = new ArrayList();
                        }

                        _codeAssemblies.Add(codeAssembly);
                    }

                    // Add it to the list of assembly name that we resolve, so that users can
                    // refer to the assemblies by their fixed name (even though they
                    // random names).  (VSWhidbey 276776)
                    if (_assemblyResolveMapping == null) {
                        _assemblyResolveMapping = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    }
                    _assemblyResolveMapping[assemblyName] = codeAssembly;

                    if (dirType == CodeDirectoryType.MainCode) {
                        // Profile gets built in the same assembly as the main code dir, so
                        // see whether we can get its type from the assembly.
                        // TODO: Migration
                        // _profileType = ProfileBuildProvider.GetProfileTypeFromAssembly(
                        //     codeAssembly, IsPrecompiledApp);

                        // To avoid breaking earlier Whidbey apps, allows the name "__code"
                        // to be used for the main code assembly.
                        //
                        _assemblyResolveMapping["__code"] = codeAssembly;
                    }
                }
            }

            Debug.Write("BuildManager", "CompileCodeDirectory generated assembly: " +
                (codeAssembly == null ? "None" : codeAssembly.ToString()));

            return codeAssembly;
        }

    internal static void AddFolderLevelBuildProviders(BuildProviderSet buildProviders, VirtualPath virtualPath,
        FolderLevelBuildProviderAppliesTo appliesTo, CompilationSection compConfig, ICollection referencedAssemblies) {

        if (buildProviders == null) {
            return;
        }

        List<Type> buildProviderTypes = CompilationUtil.GetFolderLevelBuildProviderTypes(compConfig, appliesTo);
        if (buildProviderTypes != null) {
            foreach (Type buildProviderType in buildProviderTypes) {
                // TODO: Migration
                // object o = HttpRuntime.CreatePublicInstanceByWebObjectActivator(buildProviderType);
                object o = Activator.CreateInstance(buildProviderType);
                BuildProvider buildProvider = (BuildProvider)o;

                buildProvider.SetVirtualPath(virtualPath);
                buildProvider.SetReferencedAssemblies(referencedAssemblies);

                buildProviders.Add(buildProvider);

            }
        }
    }

    private static void EnsureFirstTimeDirectoryInitForDependencies(ICollection dependencies) {
        foreach (String dependency in dependencies) {
            VirtualPath dependencyPath = VirtualPath.Create(dependency);
            VirtualPath dir = dependencyPath.Parent;
            EnsureFirstTimeDirectoryInit(dir);
        }
    }

    private static BuildResult PostProcessFoundBuildResult(BuildResult result, bool keyFromVPP, VirtualPath virtualPath) {

    // Check that the virtual path in the result matches the passed in
    // virtualPath (VSWhidbey 516641).  But skip this check in case the key came from
    // calling VirtualPathProvider.GetCacheKey, as it may legitimately not match.
    if (!keyFromVPP) {
        if (virtualPath != null && virtualPath != result.VirtualPath) {
            Debug.Assert(false);
            return null;
        }
    }

    // If what we found in the cache is a CompileError, rethrow the exception
    if (result is BuildResultCompileError) {
        // Report the cached error from Callback interface.
        HttpCompileException compileException = ((BuildResultCompileError)result).CompileException;

        // But don't report it if we're doing precompilation, as that would cause it to be
        // reported twice because we always try to compile everything that has failed
        // before (VSWhidbey 525414)
        // TODO: Migration
        // if (!PerformingPrecompilation) {
        //     ReportErrorsFromException(compileException);
        // }

        throw compileException;
    }

    return result;
}

    internal static string GetLocalResourcesAssemblyName(VirtualPath virtualDir)
    {
        throw new NotImplementedException("GetLocalResourcesAssemblyName");
    }

    internal static TextWriter GetUpdatableDeploymentTargetWriter(VirtualPath currentVirtualPath, Encoding fileEncoding)
        => new StringWriter();

    internal static object GetVPathBuildResult(VirtualPath virtualPath)
    {
        throw new NotImplementedException("GetVPathBuildResult");
    }

    internal static void ReportParseError(ParserError parseError)
    {
        throw new NotImplementedException("ReportParseError");
    }

    internal static void ThrowIfPreAppStartNotRunning()
    {
        throw new NotImplementedException("ThrowIfPreAppStartNotRunning");
    }

    internal static void ValidateCodeFileVirtualPath(VirtualPath codeFileVirtualPath)
    {
    }

    public static ICollection GetReferencedAssemblies(CompilationSection compConfig, int? index = null)
    {
        throw new NotImplementedException();
    }

    public static Compilation.BuildResult GetVPathBuildResultFromCache(object virtualPathObject)
    {
        throw new NotImplementedException();
    }

    internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
        CompilationSection compConfig, ICollection referencedAssemblies,
        bool failIfUnknown) {

        return CreateBuildProvider(virtualPath, BuildProviderAppliesTo.Web,
            compConfig, referencedAssemblies, failIfUnknown);
    }

    internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
        BuildProviderAppliesTo neededFor,
        CompilationSection compConfig, ICollection referencedAssemblies,
        bool failIfUnknown) {

        string extension = virtualPath.Extension;

        Type buildProviderType = CompilationUtil.GetBuildProviderTypeFromExtension(compConfig,
            extension, neededFor, failIfUnknown);
        if (buildProviderType == null)
            return null;

        // TODO: Migration
        // object o = HttpRuntime.CreatePublicInstanceByWebObjectActivator(buildProviderType);
        object o = Activator.CreateInstance(buildProviderType);

        BuildProvider buildProvider = (BuildProvider)o;

        buildProvider.SetVirtualPath(virtualPath);
        buildProvider.SetReferencedAssemblies(referencedAssemblies);

        return buildProvider;
    }

    public static string GenerateRandomAssemblyName(string themeName)
    {
        // TODO: Migration
        // Add random string to prefix to avoid name collision
        return AssemblyNamePrefix ;
    }

    internal enum CompilationStage {
        PreTopLevelFiles = 0,       // Before EnsureTopLevelFilesCompiled() is called
        TopLevelFiles = 1,          // In EnsureTopLevelFilesCompiled() but before building global.asax
        GlobalAsax = 2,             // While building global.asax
        BrowserCapabilities = 3,    // While building browserCap
        AfterTopLevelFiles = 4      // After EnsureTopLevelFilesCompiled() is called
    }

    internal enum PreStartInitStage {
        BeforePreStartInit,
        DuringPreStartInit,
        AfterPreStartInit,
    }

    internal class AssemblyReferenceInfo {
        internal Assembly Assembly;
        internal int ReferenceIndex;

        internal AssemblyReferenceInfo(int referenceIndex) {
            ReferenceIndex = referenceIndex;
        }
    }
}

internal static class HttpRuntime2
{
    internal static RootBuilder CreateNonPublicInstance(Type fileLevelBuilderType)
    {
        throw new NotImplementedException();
    }
}

internal class BuildResultCompiledAssembly : BuildResult
{
    public Assembly ResultAssembly { get; internal set; }
}

internal static class FastPropertyAccessor
{
    internal static object GetProperty(object obj, string name, bool inDesigner)
    {
        // TODO: Make "fast"
        return obj.GetType().GetProperty(name).GetValue(obj);
    }

    internal static void SetProperty(object obj, string name, object objectValue, bool inDesigner)
    {
        // TODO: Make "fast"
        obj.GetType().GetProperty(name).SetValue(obj, objectValue);
    }
}
