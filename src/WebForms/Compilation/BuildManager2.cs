// // MIT License.
//
// using System.Collections;
// using System.Globalization;
// using System.Reflection;
// using System.Runtime.Versioning;
// using System.Text;
// using System.Web.Compilation;
// using System.Web.Configuration;
// using System.Web.Hosting;
// using System.Web.Util;
// using BuildProvider = System.Web.Compilation.BuildProvider;
//
// namespace System.Web.UI;
//
// internal class BuildManager2
// {
//     public BuildManager2()
//     {
//         CompileResourcesDirectory();
//     }
//
//     // All generated assemblies start with this prefix
//     internal const string AssemblyNamePrefix = "App_";
//
//     // Web assemblies are the assemblies generated from web files (aspx, ascx, ...)
//     internal const string WebAssemblyNamePrefix = AssemblyNamePrefix + "Web_";
//
//     internal const string AppThemeAssemblyNamePrefix = AssemblyNamePrefix + "Theme_";
//     internal const string GlobalThemeAssemblyNamePrefix = AssemblyNamePrefix + "GlobalTheme_";
//     internal const string AppBrowserCapAssemblyNamePrefix = AssemblyNamePrefix + "Browsers";
//     private const string CodegenResourceDirectoryName = "ResX";
//
//     private static string _codegenResourceDir;
//
//
//     // TODO: Migration
//     internal static bool CompileWithAllowPartiallyTrustedCallersAttribute => false;
//     internal static bool CompileWithDelaySignAttribute => false;
//     internal static bool PrecompilingWithCodeAnalysisSymbol => false;
//     public static FrameworkName TargetFramework {
//         get {
//             return MultiTargetingUtil.TargetFrameworkName;
//         }
//     }
//
//
//     /// <summary>
//     /// Temporary subdirectory under the codegen folder for buildproviders to generate embedded resource files.
//     /// </summary>
//     internal static string CodegenResourceDir {
//         get {
//             string resxDir = _codegenResourceDir;
//             if (resxDir == null) {
//                 resxDir = Path.Combine(HttpRuntime2.CodegenDirInternal, CodegenResourceDirectoryName);
//                 _codegenResourceDir = resxDir;
//             }
//             return resxDir;
//         }
//     }
//
//
//     private static BuildResultCache[] _caches => [_memoryCache, _codeGenCache];
//     private static StandardDiskBuildResultCache _codeGenCache = new(HttpRuntime2.CodegenDirInternal);
//     private static MemoryBuildResultCache _memoryCache = new();
//
//     private static CompilationStage _compilationStage = CompilationStage.PreTopLevelFiles;
//
//     private static object _lock = new();
//
//     private static bool _topLevelFilesCompiledStarted;
//     private static bool _topLevelFilesCompiledCompleted;
//     private Exception _topLevelFileCompilationException;
//
//
//     internal static bool OptimizeCompilations { get; set; }
//
//     public static bool PrecompilingForDeployment { get; internal set; }
//     public static string UpdatableInheritReplacementToken { get; internal set; }
//     public static bool PrecompilingForUpdatableDeployment { get; internal set; }
//     public static bool ThrowOnFirstParseError { get; set; } = true;
//
//     public static VirtualPath ScriptVirtualDir = Util.GetScriptLocation();
//
//     private static List<Assembly> TopLevelReferencedAssemblies = new List<Assembly>() {
//         typeof(HttpRuntime).Assembly,
//         typeof(System.ComponentModel.Component).Assembly,
//     };
//
//     private static HashSet<Assembly> s_dynamicallyAddedReferencedAssembly = new HashSet<Assembly>();
//
//
//     public static bool IsPrecompiledApp { get; set;  }
//
//     // TODO: Migration
//     public static Assembly AppResourcesAssembly { get; internal set; }
//     public static string WebHashFilePath { get; set; } = "Web_Hash.webinfo";
//
//     private const string ResourcesDirectoryAssemblyName = AssemblyNamePrefix + "GlobalResources";
//
//     internal static BuildResult GetBuildResultFromCache(string cacheKey) {
//         return GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, null /*virtualPath*/,
//             0 /*hashCode*/);
//     }
//
//     internal static BuildResult GetBuildResultFromCache(string cacheKey, VirtualPath virtualPath) {
//         return GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, virtualPath,
//             0 /*hashCode*/);
//     }
//
//     public static void CacheBuildResult(string cacheKey, BuildResult result, DateTime utcStart)
//     {
//         throw new NotImplementedException();
//     }
//
//     private static BuildResult GetBuildResultFromCacheInternal(string cacheKey, bool keyFromVPP,
//         VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate = true)
//     {
//         BuildResult result = null;
//
//         // Allow the possibility that BuildManager was not initialized due to
//         // a very early failure (e.g. see VSWhidbey 137366)
//         //Debug.Trace("BuildManager", "GetBuildResultFromCacheInternal " + _theBuildManagerInitialized);
//         // if (!_theBuildManagerInitialized)
//         //     return null;
//
//         // The first cache should always be memory
//         Debug.Assert(_caches[0] == _memoryCache);
//
//         // Try to get it from the memeory cache before taking any locks (for perf reasons)
//         result = _memoryCache.GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);
//         if (result != null)
//         {
//             return PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
//         }
//
//         Debug.Write("BuildManager", "Didn't find '" + virtualPath + "' in memory cache before lock");
//
//         lock (_lock)
//         {
//             // Try to get the BuildResult from the cheapest to most expensive cache
//             int i;
//             for (i = 0; i < _caches.Length; i++)
//             {
//                 result = _caches[i].GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);
//
//                 // There might be changes in local resources for dependencies,
//                 // so we need to make sure EnsureFirstTimeDirectoryInit gets called
//                 // for them even when we already have a cache result.
//                 // VSWhidbey Bug 560521
//
//                 if (result != null)
//                 {
//                     // We should only process the local resources folder after the top level files have been compiled,
//                     // so that any custom VPP can be registered first. (Dev10 bug 890796)
//                     if (_compilationStage == CompilationStage.AfterTopLevelFiles &&
//                         result.VirtualPathDependencies != null)
//                     {
//                         EnsureFirstTimeDirectoryInitForDependencies(result.VirtualPathDependencies);
//                     }
//
//                     break;
//                 }
//
//                 // If we didn't find it in the memory cache, perform the per directory
//                 // initialization.  This is a good place to do this, because we don't
//                 // affect the memory cache code path, but we do the init as soon as
//                 // something is not found in the memory cache.
//                 if (i == 0 && virtualPath != null)
//                 {
//                     VirtualPath virtualDir = virtualPath.Parent;
//                     EnsureFirstTimeDirectoryInit(virtualDir);
//                 }
//             }
//
//
//             if (result == null)
//                 return null;
//
//             result = PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
//             if (result == null)
//                 return null;
//
//             Debug.Assert(_memoryCache != null);
//
//             // If we found it in a cache, cache it in all the caches that come before
//             // the one where we found it.  If we found it in the memory cache, this is a no op.
//             for (int j = 0; j < i; j++)
//                 _caches[j].CacheBuildResult(cacheKey, result, DateTime.UtcNow);
//
//             Debug.Write("BuildManager", "Found '" + virtualPath + "' in " + _caches[i]);
//
//             return result;
//         }
//     }
//
//     private static Dictionary<String, String> _generatedFileTable;
//     internal static Dictionary<String, String> GenerateFileTable {
//         get {
//             if (_generatedFileTable == null) {
//                 _generatedFileTable = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
//             }
//
//             return _generatedFileTable;
//         }
//     }
//
//     internal static string GetCacheKeyFromVirtualPath(VirtualPath virtualPath) {
//         bool keyFromVPP;
//         return GetCacheKeyFromVirtualPath(virtualPath, out keyFromVPP);
//     }
//
//     static SimpleRecyclingCache _keyCache = new SimpleRecyclingCache();
//     private static string GetCacheKeyFromVirtualPath(VirtualPath virtualPath, out bool keyFromVPP) {
//
//         // Check if the VirtualPathProvider needs to use a non-default cache key
//         string key = virtualPath.GetCacheKey();
//
//         // If so, just return it
//         if (key != null) {
//             keyFromVPP = true;
//             return key.ToLowerInvariant();
//         }
//
//         // The VPP didn't return a key, so use our standard key algorithm
//         keyFromVPP = false;
//
//         // Check if the key for this virtual path is already cached
//         key = _keyCache[virtualPath.VirtualPathString] as string;
//         if (key != null) return key;
//
//         // Compute the key
//         key = GetCacheKeyFromVirtualPathInternal(virtualPath);
//
//         // The key should always be lower case
//         Debug.Assert(key == key.ToLowerInvariant());
//
//         // Cache it for next time
//         _keyCache[virtualPath.VirtualPathString] = key;
//
//         return key;
//     }
//
//     private static string GetCacheKeyFromVirtualPathInternal(VirtualPath virtualPath) {
//
//         // We want the key to be app independent (for precompilation), so we
//         // change the virtual path to be app relative
//
//         /* Disable assertion since global theme needs to compile theme files in different vroot.
//         Debug.Assert(StringUtil.VirtualPathStartsWithAppPath(virtualPath),
//             String.Format("VPath {0} is outside the application: {1}", virtualPath, HttpRuntime.AppDomainAppVirtualPath));
//         */
//         string virtualPathString = virtualPath.AppRelativeVirtualPathString.ToLowerInvariant();
//         virtualPathString = UrlPath.RemoveSlashFromPathIfNeeded(virtualPathString);
//
//         // Split the path into the directory and the name
//         int slashIndex = virtualPathString.LastIndexOf('/');
//         Debug.Assert(slashIndex >= 0 || virtualPathString == "~");
//
//         if (virtualPathString == "~")
//             return "root";
//
//         Debug.Assert(slashIndex != virtualPathString.Length - 1);
//         string name = virtualPathString.Substring(slashIndex + 1);
//         string dir;
//         if (slashIndex <= 0)
//             dir = "/";
//         else {
//             dir = virtualPathString.Substring(0, slashIndex);
//         }
//
//         return name + "." + StringUtil.GetStringHashCode(dir).ToString("x", CultureInfo.InvariantCulture);
//     }
//
//     private static Hashtable _localResourcesAssemblies = new Hashtable();
//
//     private static void EnsureFirstTimeDirectoryInit(VirtualPath virtualDir) {
//
//         // Don't process local resources when precompiling for updatable deployment.
//         // Instead, we deploy the App_LocalResources folder as is.
//         if (PrecompilingForUpdatableDeployment)
//             return;
//
//         if (virtualDir == null)
//             return;
//
//         // Only do this once per directory
//         if (_localResourcesAssemblies.Contains(virtualDir))
//             return;
//
//         // Don't do anything if it's outside the app root
//         // TODO: Migration
//         // if (!virtualDir.IsWithinAppRoot)
//         //     return;
//
//         Debug.Write("BuildManager", "EnsureFirstTimeDirectoryInit(" + virtualDir + ")");
//
//         // Get the virtual path to the LocalResources subdirectory for this directory
//         VirtualPath localResDir = virtualDir.Combine(HttpRuntime2.LocalResourcesDirectoryName);
//
//         bool dirExists;
//         try {
//             dirExists = localResDir.DirectoryExists();
//         }
//         catch {
//             // If an exception happens, the directory may be outside the application,
//             // in which case we should skip this logic, and act is if there are no
//             // local resources (VSWhidbey 258776);
//
//             _localResourcesAssemblies[virtualDir] = null;
//             return;
//         }
//
//         Debug.Write("BuildManager", "EnsureFirstTimeDirectoryInit: dirExists=" + dirExists);
//
//         // TODO: Migration
//         // try {
//         //     // Monitor changes to it so the appdomain can shut down when it changes
//         //     HttpRuntime.StartListeningToLocalResourcesDirectory(localResDir);
//         // }
//         // catch {
//         //     // could fail for long directory names
//         //     if (dirExists) {
//         //         throw;
//         //     }
//         // }
//
//         Assembly resourceAssembly = null;
//
//         // If it exists, build it
//         if (dirExists) {
//
//             string localResAssemblyName = GetLocalResourcesAssemblyName(virtualDir);
//
//             bool gotLock = false;
//
//             try {
//                 // Grab the compilation mutex, since this method accesses the codegen files
//                 // TODO: Migration
//                 // CompilationLock.GetLock(ref gotLock);
//
//                 resourceAssembly = CompileCodeDirectory(localResDir, CodeDirectoryType.LocalResources,
//                     localResAssemblyName, null /*excludedSubdirectories*/);
//             }
//             finally {
//                 // Always release the mutex if we had taken it
//                 // TODO: Migration
//                 // if (gotLock) {
//                 //     CompilationLock.ReleaseLock();
//                 // }
//             }
//         }
//
//         // Cache it whether it's null or not
//         _localResourcesAssemblies[virtualDir] = resourceAssembly;
//     }
//
//     internal static void ReportDirectoryCompilationProgress(VirtualPath virtualDir) {
//         // TODO: Migration
//         // Nothing to do if there is no CBM callback
//         // ClientBuildManagerCallback callback = CBMCallback;
//         // if (callback == null)
//         //     return;
//         //
//         // // Don't report anything if the directory doesn't exist
//         // if (!virtualDir.DirectoryExists())
//         //     return;
//         //
//         // string message = SR.GetString(SR.Directory_progress, virtualDir.VirtualPathString);
//         // callback.ReportProgress(message);
//     }
//
//     private static List<Assembly> _topLevelReferencedAssemblies = new List<Assembly>() {
//         typeof(HttpRuntime).Assembly,
//         typeof(System.ComponentModel.Component).Assembly,
//     };
//
//
//     private static ArrayList _codeAssemblies = new();
//     // TODO: Migration
//     // public static IList CodeAssemblies {
//     //     get {
//     //         EnsureTopLevelFilesCompiled();
//     //         return _codeAssemblies;
//     //     }
//     // }
//     private static IDictionary _assemblyResolveMapping = new Hashtable(StringComparer.OrdinalIgnoreCase);
//
//     private static Dictionary<String, AssemblyReferenceInfo> _topLevelAssembliesIndexTable =
//         new Dictionary<String, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase);
//     private static IDictionary<String, AssemblyReferenceInfo> TopLevelAssembliesIndexTable { get { return _topLevelAssembliesIndexTable; } }
//
//     private static Assembly CompileCodeDirectory(VirtualPath virtualDir, CodeDirectoryType dirType,
//         string assemblyName, StringSet excludedSubdirectories) {
//
//         Debug.Write("BuildManager", "CompileCodeDirectory(" + virtualDir.VirtualPathString + ")");
//
//         bool isDirectoryAllowed = true;
//         if (IsPrecompiledApp) {
//             // Most special dirs are not allowed in precompiled apps.  App_LocalResources is
//             // an exception, as it is allowed in updatable precompiled apps.
//             // TODO: Migration
//             // if (IsUpdatablePrecompiledAppInternal && dirType == CodeDirectoryType.LocalResources)
//             if (dirType == CodeDirectoryType.LocalResources)
//                 isDirectoryAllowed = true;
//             else
//                 isDirectoryAllowed = false;
//         }
//
//         // Remember the referenced assemblies based on the current count.
//         AssemblyReferenceInfo info = new AssemblyReferenceInfo(_topLevelReferencedAssemblies.Count);
//         _topLevelAssembliesIndexTable[virtualDir.VirtualPathString] = info;
//
//         Assembly codeAssembly = CodeDirectoryCompiler.GetCodeDirectoryAssembly(
//             virtualDir, dirType, assemblyName, excludedSubdirectories,
//             isDirectoryAllowed);
//
//         if (codeAssembly != null) {
//
//             // Remember the generated assembly
//             info.Assembly = codeAssembly;
//
//             // Page resource assemblies are not added to the top level list
//             if (dirType != CodeDirectoryType.LocalResources) {
//                 _topLevelReferencedAssemblies.Add(codeAssembly);
//
//                 if (dirType == CodeDirectoryType.MainCode || dirType == CodeDirectoryType.SubCode) {
//                     if (_codeAssemblies == null) {
//                         _codeAssemblies = new ArrayList();
//                     }
//
//                     _codeAssemblies.Add(codeAssembly);
//                 }
//
//                 // Add it to the list of assembly name that we resolve, so that users can
//                 // refer to the assemblies by their fixed name (even though they
//                 // random names).  (VSWhidbey 276776)
//                 if (_assemblyResolveMapping == null) {
//                     _assemblyResolveMapping = new Hashtable(StringComparer.OrdinalIgnoreCase);
//                 }
//                 _assemblyResolveMapping[assemblyName] = codeAssembly;
//
//                 if (dirType == CodeDirectoryType.MainCode) {
//                     // Profile gets built in the same assembly as the main code dir, so
//                     // see whether we can get its type from the assembly.
//                     // TODO: Migration
//                     // _profileType = ProfileBuildProvider.GetProfileTypeFromAssembly(
//                     //     codeAssembly, IsPrecompiledApp);
//
//                     // To avoid breaking earlier Whidbey apps, allows the name "__code"
//                     // to be used for the main code assembly.
//                     //
//                     _assemblyResolveMapping["__code"] = codeAssembly;
//                 }
//             }
//         }
//
//         Debug.Write("BuildManager", "CompileCodeDirectory generated assembly: " +
//                                     (codeAssembly == null ? "None" : codeAssembly.ToString()));
//
//         return codeAssembly;
//     }
//
//     private static void CompileResourcesDirectory() {
//
//         VirtualPath virtualDir = HttpRuntime2.ResourcesDirectoryVirtualPath;
//
//         Debug.Assert(AppResourcesAssembly == null);
//         AppResourcesAssembly = CompileCodeDirectory(virtualDir, CodeDirectoryType.AppResources,
//             ResourcesDirectoryAssemblyName, null /*excludedSubdirectories*/);
//     }
//
//     internal static void AddFolderLevelBuildProviders(BuildProviderSet buildProviders, VirtualPath virtualPath,
//         FolderLevelBuildProviderAppliesTo appliesTo, CompilationSection compConfig, ICollection referencedAssemblies) {
//
//         if (buildProviders == null) {
//             return;
//         }
//
//         List<Type> buildProviderTypes = CompilationUtil.GetFolderLevelBuildProviderTypes(compConfig, appliesTo);
//         if (buildProviderTypes != null) {
//             foreach (Type buildProviderType in buildProviderTypes) {
//                 // TODO: Migration
//                 // object o = HttpRuntime.CreatePublicInstanceByWebObjectActivator(buildProviderType);
//                 object o = Activator.CreateInstance(buildProviderType);
//                 BuildProvider buildProvider = (BuildProvider)o;
//
//                 buildProvider.SetVirtualPath(virtualPath);
//                 buildProvider.SetReferencedAssemblies(referencedAssemblies);
//
//                 buildProviders.Add(buildProvider);
//
//             }
//         }
//     }
//
//     private static void EnsureFirstTimeDirectoryInitForDependencies(ICollection dependencies) {
//         foreach (String dependency in dependencies) {
//             VirtualPath dependencyPath = VirtualPath.Create(dependency);
//             VirtualPath dir = dependencyPath.Parent;
//             EnsureFirstTimeDirectoryInit(dir);
//         }
//     }
//
//     private static BuildResult PostProcessFoundBuildResult(BuildResult result, bool keyFromVPP, VirtualPath virtualPath) {
//
//         // Check that the virtual path in the result matches the passed in
//         // virtualPath (VSWhidbey 516641).  But skip this check in case the key came from
//         // calling VirtualPathProvider.GetCacheKey, as it may legitimately not match.
//         if (!keyFromVPP) {
//             if (virtualPath != null && virtualPath != result.VirtualPath) {
//                 Debug.Assert(false);
//                 return null;
//             }
//         }
//
//         // If what we found in the cache is a CompileError, rethrow the exception
//         if (result is BuildResultCompileError) {
//             // Report the cached error from Callback interface.
//             HttpCompileException compileException = ((BuildResultCompileError)result).CompileException;
//
//             // But don't report it if we're doing precompilation, as that would cause it to be
//             // reported twice because we always try to compile everything that has failed
//             // before (VSWhidbey 525414)
//             // TODO: Migration
//             // if (!PerformingPrecompilation) {
//             //     ReportErrorsFromException(compileException);
//             // }
//
//             throw compileException;
//         }
//
//         return result;
//     }
//
//     internal static string GetLocalResourcesAssemblyName(VirtualPath virtualDir)
//     {
//         throw new NotImplementedException("GetLocalResourcesAssemblyName");
//     }
//
//     internal static TextWriter GetUpdatableDeploymentTargetWriter(VirtualPath currentVirtualPath, Encoding fileEncoding)
//         => new StringWriter();
//
//     internal static object GetVPathBuildResult(VirtualPath virtualPath)
//     {
//         throw new NotImplementedException("GetVPathBuildResult");
//     }
//
//     internal static void ReportParseError(ParserError parseError)
//     {
//         throw new NotImplementedException("ReportParseError");
//     }
//
//     internal static void ThrowIfPreAppStartNotRunning()
//     {
//         throw new NotImplementedException("ThrowIfPreAppStartNotRunning");
//     }
//
//     internal static void ValidateCodeFileVirtualPath(VirtualPath codeFileVirtualPath)
//     {
//     }
//
//     internal static ICollection GetReferencedAssemblies(CompilationSection compConfig, int removeIndex) {
//         AssemblySet referencedAssemblies = new AssemblySet();
//
//         // Add all the config assemblies to the list
//         foreach (AssemblyInfo a in compConfig.Assemblies) {
//             Assembly[] assemblies = a.AssemblyInternal;
//             if (assemblies == null) {
//                 lock (compConfig) {
//                     assemblies = a.AssemblyInternal;
//                     if (assemblies == null)
//                         //
//                         assemblies = a.AssemblyInternal = compConfig.LoadAssembly(a);
//                 }
//             }
//
//             for (int i = 0; i < assemblies.Length; i++) {
//                 if (assemblies[i] != null) {
//                     referencedAssemblies.Add(assemblies[i]);
//                 }
//             }
//         }
//
//         // Clone the top level referenced assemblies (code + global.asax + etc...), up to the removeIndex
//         for (int i = 0; i < removeIndex; i++) {
//             referencedAssemblies.Add(TopLevelReferencedAssemblies[i]);
//         }
//
//         //
//
//         foreach (Assembly assembly in s_dynamicallyAddedReferencedAssembly) {
//             referencedAssemblies.Add(assembly);
//         }
//
//         return referencedAssemblies;
//     }
//
//     internal static ICollection GetReferencedAssemblies(CompilationSection compConfig) {
//
//         // Start by cloning the top level referenced assemblies (code + global.asax + etc...)
//         AssemblySet referencedAssemblies = AssemblySet.Create(TopLevelReferencedAssemblies);
//
//         // Add all the config assemblies to the list
//         foreach (AssemblyInfo a in compConfig.Assemblies) {
//             Assembly[] assemblies = a.AssemblyInternal;
//             if (assemblies == null) {
//                 lock (compConfig) {
//                     assemblies = a.AssemblyInternal;
//                     if (assemblies == null)
//                         //
//                         assemblies = a.AssemblyInternal = compConfig.LoadAssembly(a);
//                 }
//             }
//
//             for (int i = 0; i < assemblies.Length; i++) {
//                 if (assemblies[i] != null) {
//                     referencedAssemblies.Add(assemblies[i]);
//                 }
//             }
//         }
//
//         //
//
//         foreach (Assembly assembly in s_dynamicallyAddedReferencedAssembly) {
//             referencedAssemblies.Add(assembly);
//         }
//
//         return referencedAssemblies;
//     }
//
//
//     /*
//          * Return the list of assemblies that all page compilations need to reference. This includes
//          * config assemblies (<assemblies> section), bin assemblies and assemblies built from the
//          * app App_Code and other top level folders.
//          */
//
//
//
//     public static Compilation.BuildResult GetVPathBuildResultFromCache(object virtualPathObject)
//     {
//         throw new NotImplementedException();
//     }
//
//     internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
//         CompilationSection compConfig, ICollection referencedAssemblies,
//         bool failIfUnknown) {
//
//         return CreateBuildProvider(virtualPath, BuildProviderAppliesTo.Web,
//             compConfig, referencedAssemblies, failIfUnknown);
//     }
//
//     internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
//         BuildProviderAppliesTo neededFor,
//         CompilationSection compConfig, ICollection referencedAssemblies,
//         bool failIfUnknown) {
//
//         string extension = virtualPath.Extension;
//
//         Type buildProviderType = CompilationUtil.GetBuildProviderTypeFromExtension(compConfig,
//             extension, neededFor, failIfUnknown);
//         if (buildProviderType == null)
//             return null;
//
//         // TODO: Migration
//         // object o = HttpRuntime.CreatePublicInstanceByWebObjectActivator(buildProviderType);
//         object o = Activator.CreateInstance(buildProviderType);
//
//         BuildProvider buildProvider = (BuildProvider)o;
//
//         buildProvider.SetVirtualPath(virtualPath);
//         buildProvider.SetReferencedAssemblies(referencedAssemblies);
//
//         return buildProvider;
//     }
//
//     public static string GenerateRandomAssemblyName(string themeName)
//     {
//         // TODO: Migration
//         // Add random string to prefix to avoid name collision
//         return AssemblyNamePrefix ;
//     }
//
//     internal enum CompilationStage {
//         PreTopLevelFiles = 0,       // Before EnsureTopLevelFilesCompiled() is called
//         TopLevelFiles = 1,          // In EnsureTopLevelFilesCompiled() but before building global.asax
//         GlobalAsax = 2,             // While building global.asax
//         BrowserCapabilities = 3,    // While building browserCap
//         AfterTopLevelFiles = 4      // After EnsureTopLevelFilesCompiled() is called
//     }
//
//     internal enum PreStartInitStage {
//         BeforePreStartInit,
//         DuringPreStartInit,
//         AfterPreStartInit,
//     }
//
//     internal class AssemblyReferenceInfo {
//         internal Assembly Assembly;
//         internal int ReferenceIndex;
//
//         internal AssemblyReferenceInfo(int referenceIndex) {
//             ReferenceIndex = referenceIndex;
//         }
//     }
// }
