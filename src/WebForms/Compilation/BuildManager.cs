// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.UI;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualBasic;

namespace System.Web.Compilation;

public sealed class BuildManager
{
    /// Contants relating to generated assembly names

    // All generated assemblies start with this prefix
    internal const string AssemblyNamePrefix = "App_";

    // Web assemblies are the assemblies generated from web files (aspx, ascx, ...)
    internal const string WebAssemblyNamePrefix = AssemblyNamePrefix + "Web_";

    internal const string AppThemeAssemblyNamePrefix = AssemblyNamePrefix + "Theme_";
    internal const string GlobalThemeAssemblyNamePrefix = AssemblyNamePrefix + "GlobalTheme_";
    internal const string AppBrowserCapAssemblyNamePrefix = AssemblyNamePrefix + "Browsers";

    private const string CodeDirectoryAssemblyName = AssemblyNamePrefix + "Code";
    internal const string SubCodeDirectoryAssemblyNamePrefix = AssemblyNamePrefix + "SubCode_";
    private const string ResourcesDirectoryAssemblyName = AssemblyNamePrefix + "GlobalResources";
    private const string LocalResourcesDirectoryAssemblyName = AssemblyNamePrefix + "LocalResources";
    private const string WebRefDirectoryAssemblyName = AssemblyNamePrefix + "WebReferences";

    private static bool _theBuildManagerInitialized;
    private static Exception _initializeException;
    private static BuildManager _theBuildManager = new BuildManager();  // single instance of the class
    private StringSet _excludedTopLevelDirectories;
    private IDictionary _assemblyResolveMapping;

    private BuildResultCache[] _caches;
    internal static BuildManager TheBuildManager { get { return _theBuildManager; } }

    private static HashSet<Assembly> s_dynamicallyAddedReferencedAssembly = new HashSet<Assembly>();

    // The assemblies produced from the code directories and global.asax, which
    // every other compilation will linked with.
    private List<Assembly> _topLevelReferencedAssemblies = new List<Assembly>() {
            typeof(HttpRuntime).Assembly,
            typeof(System.ComponentModel.Component).Assembly,
        };

    private List<Assembly> TopLevelReferencedAssemblies { get { return _topLevelReferencedAssemblies; } }

    /*
     * Look for a type by name in the top level and config assemblies
     */
    public static Type GetType(string typeName, bool throwOnError)
    {
        return GetType(typeName, throwOnError, false);
    }

    internal static bool InitializeBuildManager()
    {

        // If we already tried and got an exception, just rethrow it
        if (_initializeException != null)
        {
            // We need to wrap it in a new exception, otherwise we lose the original stack.
            throw new HttpException(_initializeException.Message, _initializeException);
        }

        if (!_theBuildManagerInitialized)
        {
            //// If Fusion was not yet initialized, skip the init.
            //// This can happen when there is a very early failure (e.g. see VSWhidbey 137366)
            //Debug.Write("BuildManager", "InitializeBuildManager " + HttpRuntime.FusionInited);
            //if (!HttpRuntime.FusionInited)
            //    return false;

            //// Likewise, if the trust level has not yet been determined, skip the init (VSWhidbey 422311)
            //if (HttpRuntime.TrustLevel == null)
            //    return false;

            _theBuildManagerInitialized = true;
            try
            {
                _theBuildManager.Initialize();
            }
            catch (Exception e)
            {
                _theBuildManagerInitialized = false;
                _initializeException = e;
                throw;
            }
        }
        return true;
    }

    private void Initialize()
    {

        Debug.Assert(_caches == null);

        // Register an AssemblyResolve event
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.ResolveAssembly);

        //_globalAsaxVirtualPath = HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine(
        //    HttpApplicationFactory.applicationFileName);

        //_webHashFilePath = Path.Combine(HttpRuntime.CodegenDirInternal, "hash\\hash.web");

        //// Indicate whether we should ignore the top level compilation exceptions.
        //// In CBM case, we want to continue processing the page and return partial info even
        //// if the code files fail to compile.
        //_skipTopLevelCompilationExceptions = BuildManagerHost.InClientBuildManager;

        //// Deal with precompilation if we're in that mode
        //SetPrecompilationInfo(HostingEnvironment.HostingParameters);

        //MultiTargetingUtil.EnsureFrameworkNamesInitialized();

        //// The init code depends on whether we're precompiling or running an app
        //if (_precompTargetPhysicalDir != null)
        //{

        //    // If the app is already precompiled, fail
        //    FailIfPrecompiledApp();

        //    PrecompilationModeInitialize();
        //}
        //else
        //{
        //    // Check if this application has been precompiled by aspnet_compiler.exe
        //    if (IsPrecompiledApp)
        //    {
        //        PrecompiledAppRuntimeModeInitialize();
        //    }
        //    else
        //    {
        //        RegularAppRuntimeModeInitialize();
        //    }
        //}

        //_scriptVirtualDir = Util.GetScriptLocation();

        //// Top level directories that have a special semantic
        //_excludedTopLevelDirectories = new CaseInsensitiveStringSet();
        //_excludedTopLevelDirectories.Add(HttpRuntime.BinDirectoryName);
        //_excludedTopLevelDirectories.Add(HttpRuntime.CodeDirectoryName);
        //_excludedTopLevelDirectories.Add(HttpRuntime.ResourcesDirectoryName);
        //_excludedTopLevelDirectories.Add(HttpRuntime.LocalResourcesDirectoryName);
        //_excludedTopLevelDirectories.Add(HttpRuntime.WebRefDirectoryName);
        //_excludedTopLevelDirectories.Add(HttpRuntime.ThemesDirectoryName);

        //// Top level directories that are not requestable
        //// It's the same as _excludedTopLevelDirectories, except that we allow
        //// the bin directory to avoid a v1 breaking change (VSWhidbey 465018)
        //_forbiddenTopLevelDirectories = new CaseInsensitiveStringSet();
        //_forbiddenTopLevelDirectories.Add(HttpRuntime.CodeDirectoryName);
        //_forbiddenTopLevelDirectories.Add(HttpRuntime.ResourcesDirectoryName);
        //_forbiddenTopLevelDirectories.Add(HttpRuntime.LocalResourcesDirectoryName);
        //_forbiddenTopLevelDirectories.Add(HttpRuntime.WebRefDirectoryName);
        //_forbiddenTopLevelDirectories.Add(HttpRuntime.ThemesDirectoryName);

        //LoadLicensesAssemblyIfExists();
    }

    private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
    {

        if (_assemblyResolveMapping == null)
            return null;

        string name = e.Name;
        Assembly assembly = (Assembly)_assemblyResolveMapping[name];

        // Return the assembly if we have it in our mapping (VSWhidbey 276776)
        if (assembly != null)
        {
            return assembly;
        }

        // Get the normalized assembly name from random name (VSWhidbey 380793)
        String normalizedName = GetNormalizedCodeAssemblyName(name);
        if (normalizedName != null)
        {
            return (Assembly)_assemblyResolveMapping[normalizedName];
        }

        return null;
    }

    internal static string GetNormalizedCodeAssemblyName(string assemblyName)
    {
        // Return the main code assembly.
        if (assemblyName.StartsWith(CodeDirectoryAssemblyName, StringComparison.Ordinal))
        {
            return CodeDirectoryAssemblyName;
        }

        // Check the sub code directories.
        CodeSubDirectoriesCollection codeSubDirectories = CompilationUtil.GetCodeSubDirectories();
        foreach (CodeSubDirectory directory in codeSubDirectories)
        {
            if (assemblyName.StartsWith(SubCodeDirectoryAssemblyNamePrefix + directory.AssemblyName + ".", StringComparison.Ordinal))
            {
                return directory.AssemblyName;
            }
        }

        return null;
    }

    /*
     * Look for a type by name in the top level and config assemblies
     */
    public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
    {
        // If it contains an assembly name, just call Type.GetType().  Do this before even trying
        // to initialize the BuildManager, so that if InitializeBuildManager has errors, it doesn't
        // affect us when the type string can be resolved via Type.GetType().
        Type type = null;
        if (UI.Util.TypeNameContainsAssembly(typeName))
        {
            type = Type.GetType(typeName, throwOnError, ignoreCase);

            if (type != null)
            {
                return type;
            }
        }

        // Make sure the build manager is initialized.  If it fails to initialize for any reason,
        // don't attempt to use the fancy GetType logic.  Just call Type.GetType instead (VSWhidbey 284498)
        if (!InitializeBuildManager())
        {
            return Type.GetType(typeName, throwOnError, ignoreCase);
        }

        // First, always try System.Web.dll
        try
        {
            type = typeof(BuildManager).Assembly.GetType(typeName,
                false /*throwOnError*/, ignoreCase);
        }
        catch (ArgumentException e)
        {
            // Even though we pass false to throwOnError, GetType can throw if the
            // assembly name is malformed.  In that case, throw our own error instead
            // of the cryptic ArgumentException (VSWhidbey 275586)
            throw new HttpException(
                SR.GetString(SR.Invalid_type, typeName), e);
        }

        if (type != null) return type;

        _theBuildManager.EnsureTopLevelFilesCompiled();

        // Otherwise, look for the type in the top level assemblies
        type = UI.Util.GetTypeFromAssemblies(TheBuildManager.TopLevelReferencedAssemblies,
            typeName, ignoreCase);
        if (type != null) return type;

        // Otherwise, look for the type in the config assemblies
        IEnumerable<Assembly> configAssemblies = GetAssembliesForAppLevel();
        type = UI.Util.GetTypeFromAssemblies(configAssemblies, typeName, ignoreCase);

        if (type == null && throwOnError)
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_type, typeName));
        }

        return type;
    }

    /*
    * Simple wrapper to get the Assemblies
    */
    private static IEnumerable<Assembly> GetAssembliesForAppLevel()
    {
        CompilationSection compilationConfiguration = MTConfigUtil.GetCompilationAppConfig();
        AssemblyCollection assemblyInfoCollection = null;// compilationConfiguration.Assemblies;

        Debug.Assert(s_dynamicallyAddedReferencedAssembly != null);

        //if (assemblyInfoCollection == null)
        //{
            return s_dynamicallyAddedReferencedAssembly.OfType<Assembly>();
        //}

        // todo - implement CompilationSection & AssemblyCollection
        //return assemblyInfoCollection.Cast<AssemblyInfo>()
        //    .SelectMany(ai => ai.AssemblyInternal)
        //    .Union(s_dynamicallyAddedReferencedAssembly)
        //    .Distinct();
    }

    internal void EnsureTopLevelFilesCompiled()
    {
        // todo
        //if (PreStartInitStage != Compilation.PreStartInitStage.AfterPreStartInit)
        //{
        //    throw new InvalidOperationException(SR.GetString(SR.Method_cannot_be_called_during_pre_start_init));
        //}

        //// This should never get executed in non-hosted appdomains
        //Debug.Assert(HostingEnvironment.IsHosted);

        //// If we already tried and got an exception, just rethrow it
        //if (_topLevelFileCompilationException != null && !SkipTopLevelCompilationExceptions)
        //{
        //    ReportTopLevelCompilationException();
        //}

        //if (_topLevelFilesCompiledStarted)
        //    return;

        //// Set impersonation to hosting identity (process or UNC)
        //using (new ApplicationImpersonationContext())
        //{
        //    bool gotLock = false;
        //    _parseErrorReported = false;

        //    try
        //    {
        //        // Grab the compilation mutex, since this method accesses the codegen files
        //        CompilationLock.GetLock(ref gotLock);

        //        // Check again if there is an exception
        //        if (_topLevelFileCompilationException != null && !SkipTopLevelCompilationExceptions)
        //        {
        //            ReportTopLevelCompilationException();
        //        }

        //        // Check again if we're done
        //        if (_topLevelFilesCompiledStarted)
        //            return;

        //        _topLevelFilesCompiledStarted = true;
        //        _topLevelAssembliesIndexTable =
        //            new Dictionary<String, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase);

        //        _compilationStage = CompilationStage.TopLevelFiles;

        //        CompileResourcesDirectory();
        //        CompileWebRefDirectory();
        //        CompileCodeDirectories();

        //        _compilationStage = CompilationStage.GlobalAsax;

        //        CompileGlobalAsax();

        //        _compilationStage = CompilationStage.BrowserCapabilities;

        //        // Call GetBrowserCapabilitiesType() to make sure browserCap directory is compiled
        //        // early on.  This avoids getting into potential deadlock situations later (VSWhidbey 530732).
        //        // For the same reason, get the EmptyHttpCapabilitiesBase.
        //        BrowserCapabilitiesCompiler.GetBrowserCapabilitiesType();
        //        IFilterResolutionService dummy = HttpCapabilitiesBase.EmptyHttpCapabilitiesBase;

        //        _compilationStage = CompilationStage.AfterTopLevelFiles;
        //    }
        //    catch (Exception e)
        //    {
        //        // Remember the exception, and rethrow it
        //        _topLevelFileCompilationException = e;

        //        // Do not rethrow the exception since so CBM can still provide partial support
        //        if (!SkipTopLevelCompilationExceptions)
        //        {

        //            if (!_parseErrorReported)
        //            {
        //                // Report the error if this is not a CompileException. CompileExceptions are handled
        //                // directly by the AssemblyBuilder already.
        //                if (!(e is HttpCompileException))
        //                {
        //                    ReportTopLevelCompilationException();
        //                }
        //            }

        //            throw;
        //        }
        //    }
        //    finally
        //    {
        //        _topLevelFilesCompiledCompleted = true;

        //        // Always release the mutex if we had taken it
        //        if (gotLock)
        //        {
        //            CompilationLock.ReleaseLock();
        //        }
        //    }
        //}
    }
}
