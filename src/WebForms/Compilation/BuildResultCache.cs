//------------------------------------------------------------------------------
// <copyright file="BuildResultCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



/*********************************

BuildResultCache
    MemoryBuildResultCache
    DiskBuildResultCache
        StandardDiskBuildResultCache
        PrecompBaseDiskBuildResultCache
            PrecompilerDiskBuildResultCache
            PrecompiledSiteDiskBuildResultCache

**********************************/

namespace System.Web.Compilation;

using System;
using System.IO;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.Caching;
using System.Web.UI;

internal abstract class BuildResultCache
{
    internal BuildResult GetBuildResult(string cacheKey)
    {
        return GetBuildResult(cacheKey, null /*virtualPath*/, 0 /*hashCode*/);
    }

    internal abstract BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode,
        bool ensureIsUpToDate = true);

    internal void CacheBuildResult(string cacheKey, BuildResult result, DateTime utcStart)
    {
        CacheBuildResult(cacheKey, result, 0 /*hashCode*/, utcStart);
    }

    internal abstract void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart);

    internal static string GetAssemblyCacheKey(string assemblyPath)
    {
        string assemblyName = Util.GetAssemblyNameFromFileName(Path.GetFileName(assemblyPath));
        return GetAssemblyCacheKeyFromName(assemblyName);
    }

    internal static string GetAssemblyCacheKey(Assembly assembly)
    {
        Debug.Assert(!assembly.GlobalAssemblyCache);
        return GetAssemblyCacheKeyFromName(assembly.GetName().Name);
    }

    internal static string GetAssemblyCacheKeyFromName(string assemblyName)
    {
        Debug.Assert(StringUtil.StringStartsWith(assemblyName, BuildManager.AssemblyNamePrefix));
        // TODO: Migration
        // return CacheInternal.PrefixAssemblyPath + assemblyName.ToLowerInvariant();
        return assemblyName.ToLowerInvariant();
    }

}

internal class MemoryBuildResultCache : BuildResultCache
{

    private CacheItemRemovedCallback _onRemoveCallback;

    // The keys are simple assembly names
    // The values are ArrayLists containing the simple names of assemblies that depend on it
    private Hashtable _dependentAssemblies = new Hashtable();

    internal MemoryBuildResultCache()
    {

        // Register an AssemblyLoad event
        AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(OnAssemblyLoad);
    }

    private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        Assembly a = args.LoadedAssembly;

        // Ignore GAC assemblies
        if (a.GlobalAssemblyCache)
            return;

        // Ignore assemblies that don't start with our prefix
        string name = a.GetName().Name;
        if (!StringUtil.StringStartsWith(name, BuildManager.AssemblyNamePrefix))
            return;

        // Go through all the assemblies it references
        foreach (AssemblyName assemblyName in a.GetReferencedAssemblies())
        {

            // Ignore references that don't start with our prefix
            if (!StringUtil.StringStartsWith(assemblyName.Name, BuildManager.AssemblyNamePrefix))
                continue;

            lock (_dependentAssemblies)
            {
                // Check whether we already have an ArrayList for this reference
                ArrayList dependentList = _dependentAssemblies[assemblyName.Name] as ArrayList;
                if (dependentList == null)
                {
                    // If not, create one and add it to the hashtable
                    dependentList = new ArrayList();
                    _dependentAssemblies[assemblyName.Name] = dependentList;
                }

                // Add the assembly that just got loaded as a dependent
                Debug.Assert(!dependentList.Contains(name));
                dependentList.Add(name);
            }
        }
    }

    internal override BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode,
        bool ensureIsUpToDate)
    {
        Debug.WriteLine("BuildResultCache", "Looking for '" + cacheKey + "' in the memory cache");

        string key = GetMemoryCacheKey(cacheKey);
        BuildResult result = (BuildResult)HttpRuntime.Cache.Get(key);

        // Not found in the cache
        if (result == null)
        {
            Debug.WriteLine("BuildResultCache", "'" + cacheKey + "' was not found in the memory cache");
            return null;
        }

        // We found it in the cache, but is it up to date.  First, if it uses a CacheDependency,
        // it must be up to date (this is the default case when using MapPathBasedVirtualPathProvider).
        // If not, then we need to explicitely check that it's up to date (more expensive)
        if (!result.UsesCacheDependency && !result.IsUpToDate(virtualPath, ensureIsUpToDate))
        {

            Debug.WriteLine("BuildResultCache", "'" + cacheKey + "' was found but is out of date");

            // Remove it from the cache
            HttpRuntime.Cache.Remove(key);

            // Debug.Assert(HttpRuntime.Cache.Get(key) == null);

            return null;
        }

        Debug.WriteLine("BuildResultCache", "'" + cacheKey + "' was found in the memory cache");

        // It's up to date: return it
        return result;
    }

    internal override void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart)
    {

        ICollection virtualDependencies = result.VirtualPathDependencies;

        Debug.WriteLine("BuildResultCache", "Adding cache " + cacheKey + " in the memory cache");

        CacheDependency cacheDependency = null;

        if (virtualDependencies != null)
        {
            // TODO: Migration
            // cacheDependency = result.VirtualPath.GetCacheDependency(virtualDependencies, utcStart);

            // If we got a cache dependency, remember that in the BuildResult
            if (cacheDependency != null)
                result.UsesCacheDependency = true;
        }

        // If it should not be cached to memory, leave it alone
        if (!result.CacheToMemory)
        {
            return;
        }

        if (BuildResultCompiledType.UsesDelayLoadType(result))
        {
            // If the result is delaying loading of assembly, then don't cache
            // to avoid having to load the assembly.
            return;
        }

        BuildResultCompiledAssemblyBase compiledResult = result as BuildResultCompiledAssemblyBase;
        if (compiledResult != null && compiledResult.ResultAssembly != null && !compiledResult.UsesExistingAssembly)
        {

            // Insert a new cache entry using the assembly path as the key
            string assemblyKey = GetAssemblyCacheKey(compiledResult.ResultAssembly);
            Assembly a = (Assembly)HttpRuntime.Cache.Get(assemblyKey);
            // TODO: Migration
            // if (a == null) {
            //     Debug.WriteLine("BuildResultCache", "Adding marker cache entry " + compiledResult.ResultAssembly);
            //     // VSWhidbey 500049 - add as NotRemovable to prevent the assembly from being prematurely deleted
            //     HttpRuntime.Cache.Insert(assemblyKey, compiledResult.ResultAssembly,
            //         new CacheInsertOptions() { Priority = CacheItemPriority.NotRemovable });
            // }
            // else {
            //     Debug.Assert(a == compiledResult.ResultAssembly);
            // }

            // Now create a dependency based on that key. This way, by removing that key, we are able to
            // remove all the pages that live in that assembly from the cache.
            CacheDependency assemblyCacheDependency = new CacheDependency(null, new string[] { assemblyKey });

            // TODO: Migration
            // if (cacheDependency != null) {
            //     // We can't share the same CacheDependency, since we don't want the UtcStart
            //     // behavior for the assembly.  Use an Aggregate to put the two together.
            //     AggregateCacheDependency tmpDependency = new AggregateCacheDependency();
            //     tmpDependency.Add(new CacheDependency[] { cacheDependency, assemblyCacheDependency });
            //     cacheDependency = tmpDependency;
            // }
            // else {
            cacheDependency = assemblyCacheDependency;
            // }
        }

        string key = GetMemoryCacheKey(cacheKey);

        // Only allow the cache item to expire if the result can be unloaded.  Otherwise,
        // we may as well cache it forever (e.g. for Assemblies and Types).
        CacheItemPriority cachePriority;
        if (result.IsUnloadable)
            cachePriority = CacheItemPriority.Default;
        else
            cachePriority = CacheItemPriority.NotRemovable;

        CacheItemRemovedCallback onRemoveCallback = null;

        // If the appdomain needs to be shut down when the item becomes invalid, register
        // a callback to do the shutdown.
        if (result.ShutdownAppDomainOnChange || result is BuildResultCompiledAssemblyBase)
        {

            // Create the delegate on demand
            if (_onRemoveCallback == null)
                _onRemoveCallback = new CacheItemRemovedCallback(OnCacheItemRemoved);

            onRemoveCallback = _onRemoveCallback;
        }

        HttpRuntime.Cache.Insert(key, result, cacheDependency, result.MemoryCacheExpiration,
            result.MemoryCacheSlidingExpiration, cachePriority, onRemoveCallback);
    }

    // OnCacheItemRemoved can be invoked with user code on the stack, for example if someone
    // implements VirtualPathProvider.GetCacheDependency to return a custom CacheDependency.
    // This callback needs PathDiscovery, Read, and Write permission.
    private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
    {

        // Only handle case when the dependency is removed.
        if (reason == CacheItemRemovedReason.DependencyChanged)
        {
            Debug.WriteLine("BuildResultCache", "OnCacheItemRemoved Key=" + key);

            // Remove the assembly if a buildresult becomes obsolete
            // TODO: Migration
            // if (HostingEnvironment.ShutdownInitiated) {
            //     // VSWhidbey 564168
            //     // We still need to mark the affected files and dependencies for later deletion so that we do not build up unused assemblies.
            //     RemoveAssemblyAndCleanupDependenciesShuttingDown(value as BuildResultCompiledAssembly);
            // }
            // else {

            RemoveAssemblyAndCleanupDependencies(value as BuildResultCompiledAssemblyBase);

            // Shutdown the appdomain if the buildresult requires it.
            if (((BuildResult)value).ShutdownAppDomainOnChange)
            {
                // Dev10 823114
                // At this point in code, it is possible that the current thread have acquired the CompilationMutex, and calling
                // InitiateShutdownWithoutDemand will result in an acquisition of the lock on LockableAppDomainContext.
                // A deadlock would happen if another thread were starting up, having acquired the lock on LockableAppDomainContext
                // and going on to perform some compilation thus waiting on the CompilationMutex.
                // In order to avoid the deadlock, we perform the call to InitiateShutdownWithoutDemand on a separate thread,
                // so that it is possible for the current thread to continue without blocking or waiting on any lock, and
                // to release the CompilationMutex later on.

                ThreadPool.QueueUserWorkItem(new WaitCallback(MemoryBuildResultCache.ShutdownCallBack),
                    "BuildResult change, cache key=" + key);
            }
            // }
        }
    }

    static private void ShutdownCallBack(Object state)
    {
        string message = state as string;
        // TODO: Migration
        // if (message != null) {
        //     HttpRuntime.SetShutdownReason(ApplicationShutdownReason.BuildManagerChange, message);
        // }
        // HostingEnvironment.InitiateShutdownWithoutDemand();
    }

    // Since we are shutting down, we will just create the .delete files to mark the files for deletion,
    // and not try to get the compilation lock.
    internal void RemoveAssemblyAndCleanupDependenciesShuttingDown(BuildResultCompiledAssemblyBase compiledResult)
    {
        if (compiledResult == null)
            return;

        if (compiledResult != null && compiledResult.ResultAssembly != null && !compiledResult.UsesExistingAssembly)
        {
            string assemblyName = compiledResult.ResultAssembly.GetName().Name;
            lock (_dependentAssemblies)
            {
                RemoveAssemblyAndCleanupDependenciesNoLock(assemblyName);
            }
        }
    }


    internal void RemoveAssemblyAndCleanupDependencies(BuildResultCompiledAssemblyBase compiledResult)
    {
        if (compiledResult == null)
            return;

        if (compiledResult != null && compiledResult.ResultAssembly != null && !compiledResult.UsesExistingAssembly)
        {
            RemoveAssemblyAndCleanupDependencies(compiledResult.ResultAssembly.GetName().Name);
        }
    }

    private void RemoveAssemblyAndCleanupDependencies(string assemblyName)
    {
        bool gotLock = false;

        try
        {
            // Grab the compilation mutex, since we will remove cached build result
            CompilationLock.GetLock(ref gotLock);

            // Protect the dependent assemblies table, as it's accessed/modified in the recursion
            lock (_dependentAssemblies)
            {
                RemoveAssemblyAndCleanupDependenciesNoLock(assemblyName);
            }
        }
        finally
        {
            // Always release the mutex if we had taken it
            if (gotLock)
            {
                CompilationLock.ReleaseLock();
            }

            DiskBuildResultCache.ShutDownAppDomainIfRequired();
        }
    }

    private void RemoveAssemblyAndCleanupDependenciesNoLock(string assemblyName)
    {

        // If we have no cache entry for this assembly, there is nothing to do
        string cacheKey = GetAssemblyCacheKeyFromName(assemblyName);
        Assembly assembly = (Assembly)HttpRuntime.Cache.Get(cacheKey);
        if (assembly == null)
            return;

        // Get the physical path to the assembly
        String assemblyPath = Util.GetAssemblyCodeBase(assembly);

        Debug.WriteLine("BuildResultCache",
            "removing cacheKey for assembly " + assemblyPath + " because of dependency change");

        // Remove the cache entry in order to kick out all the pages that are in that batch
        HttpRuntime.Cache.Remove(cacheKey);

        // Now call recursively on all the dependent assemblies (VSWhidbey 577593)
        ICollection dependentAssemblies = _dependentAssemblies[assemblyName] as ICollection;
        if (dependentAssemblies != null)
        {
            foreach (string dependentAssemblyName in dependentAssemblies)
            {
                RemoveAssemblyAndCleanupDependenciesNoLock(dependentAssemblyName);
            }

            // We can now remove this assembly from the hashtable
            _dependentAssemblies.Remove(cacheKey);
        }

        // Remove (or rename) the DLL
        RemoveAssembly(assemblyPath);
    }

    private static void RemoveAssembly(string path)
    {
        var f = new FileInfo(path);
        DiskBuildResultCache.RemoveAssembly(f);
        // Delete the associated pdb file as well, since it is possible to
        // run into a situation where the dependency has changed just
        // when the cache item is about to get inserted, resulting in
        // the callback deleting only the dll file and leaving behind the
        // pdb file. (Dev10 bug 846606)
        var pdbPath = Path.ChangeExtension(f.FullName, ".pdb");
        if (File.Exists(pdbPath))
        {
            DiskBuildResultCache.TryDeleteFile(new FileInfo(pdbPath));
        }
    }

    private static string GetMemoryCacheKey(string cacheKey)
    {

        // Prepend something to it to avoid conflicts with other cache users
        // todo: migration
        return cacheKey;
        // return CacheInternal.PrefixMemoryBuildResult + cacheKey;
    }
}

internal abstract class DiskBuildResultCache : BuildResultCache
{

    protected const string preservationFileExtension = ".compiled";

    protected string _cacheDir;

    private static int s_recompilations;
    private static int s_maxRecompilations = -1;

    private static bool s_inUseAssemblyWasDeleted;

    protected const string dotDelete = ".delete";

    private static int s_shutdownStatus;
    private const int SHUTDOWN_NEEDED = 1;
    private const int SHUTDOWN_STARTED = 2;

    internal DiskBuildResultCache(string cacheDir)
    {
        _cacheDir = cacheDir;

        // Find out how many recompilations we allow before restarting the appdomain
        // TODO: Migration
        // if (s_maxRecompilations < 0)
        //     s_maxRecompilations = CompilationUtil.GetRecompilationsBeforeAppRestarts();
    }

    protected void EnsureDiskCacheDirectoryCreated()
    {

        // Create the disk cache directory if it's not already there
        if (!FileUtil.DirectoryExists(_cacheDir))
        {
            try
            {
                Directory.CreateDirectory(_cacheDir);
            }
            catch (IOException e)
            {
                // TODO: Migration
                // throw new HttpException(SR.GetString(SR.Failed_to_create_temp_dir, HttpRuntime.GetSafePath(_cacheDir)), e);
                throw new HttpException(SR.GetString(SR.Failed_to_create_temp_dir, _cacheDir), e);
            }
        }
    }

    internal override BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode,
        bool ensureIsUpToDate)
    {

        Debug.WriteLine("BuildResultCache", "Looking for '" + cacheKey + "' in the disk cache");

        string preservationFile = GetPreservedDataFileName(cacheKey);

        PreservationFileReader pfr = new PreservationFileReader(this, PrecompilationMode);

        // Create the BuildResult from the preservation file
        BuildResult result = pfr.ReadBuildResultFromFile(virtualPath, preservationFile, hashCode, ensureIsUpToDate);

        if (result != null)
            Debug.WriteLine("BuildResultCache", "'" + cacheKey + "' was found in the disk cache");
        else
            Debug.WriteLine("BuildResultCache", "'" + cacheKey + "' was not found in the disk cache");

        return result;
    }

    internal override void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart)
    {

        // If it should not be cached to disk, leave it alone
        if (!result.CacheToDisk)
            return;

        // VSWhidbey 564168 don't save to disk if already shutting down, otherwise we might
        // be persisting assembly that was compiled with obsolete references.
        // Since we are shutting down and not creating any cache, delete the compiled result
        // as it will not be used in future.
        // TODO: Migration
        // if (HostingEnvironment.ShutdownInitiated) {
        //     BuildResultCompiledAssemblyBase compiledResult = result as BuildResultCompiledAssemblyBase;
        //
        //     // DevDiv2 880034: check if ResultAssembly is null before calling GetName().
        //     // UsesExistingAssembly could be true in updatable compilation scenarios.
        //     if (compiledResult != null && compiledResult.ResultAssembly != null && !compiledResult.UsesExistingAssembly)
        //         MarkAssemblyAndRelatedFilesForDeletion(compiledResult.ResultAssembly.GetName().Name);
        //     return;
        // }

        string preservationFile = GetPreservedDataFileName(cacheKey);
        PreservationFileWriter pfw = new PreservationFileWriter(PrecompilationMode);

        pfw.SaveBuildResultToFile(preservationFile, result, hashCode);
    }

    private void MarkAssemblyAndRelatedFilesForDeletion(string assemblyName)
    {
        DirectoryInfo directory = new DirectoryInfo(_cacheDir);
        // Get rid of the prefix "App_web", since related files don't have it
        string baseName = assemblyName.Substring(BuildManager.WebAssemblyNamePrefix.Length);
        FileInfo[] files = directory.GetFiles("*" + baseName + ".*");
        foreach (FileInfo f in files)
            CreateDotDeleteFile(f);
    }

    /*
     * Return the physical full path to the preservation data file
     */
    private string GetPreservedDataFileName(string cacheKey)
    {

        // Make sure the key doesn't contain any invalid file name chars (VSWhidbey 263142)
        cacheKey = Util.MakeValidFileName(cacheKey);

        cacheKey = Path.Combine(_cacheDir, cacheKey);

        cacheKey = FileUtil.TruncatePathIfNeeded(cacheKey, 9 /*length of ".compiled"*/);

        // Use a ".compiled" extension for the preservation file
        return cacheKey + preservationFileExtension;
    }

    protected virtual bool PrecompilationMode
    {
        get { return false; }
    }

    internal static bool InUseAssemblyWasDeleted
    {
        get { return s_inUseAssemblyWasDeleted; }
    }

    internal static void ResetAssemblyDeleted()
    {
        s_inUseAssemblyWasDeleted = false;
    }

    /*
     * Delete an assembly and all its related files.  The assembly is typically named
     * something like ASPNET.jnw_y10n.dll, while related files are simply jnw_y10n.*.
     */
    internal virtual void RemoveAssemblyAndRelatedFiles(string assemblyName)
    {

        Debug.WriteLine("DiskBuildResultCache", "RemoveAssemblyAndRelatedFiles(" + assemblyName + ")");

        // If the name doesn't start with the prefix, the cleanup code doesn't apply
        if (!assemblyName.StartsWith(BuildManager.WebAssemblyNamePrefix, StringComparison.Ordinal))
        {
            return;
        }

        // Get rid of the prefix, since related files don't have it
        string baseName = assemblyName.Substring(BuildManager.WebAssemblyNamePrefix.Length);

        bool gotLock = false;
        try
        {
            // Grab the compilation mutex, since we will remove generated assembly
            CompilationLock.GetLock(ref gotLock);

            DirectoryInfo directory = new DirectoryInfo(_cacheDir);

            // Find all the files that contain the base name
            FileInfo[] files = directory.GetFiles("*" + baseName + ".*");
            foreach (FileInfo f in files)
            {

                if (f.Extension == ".dll")
                {
                    // Notify existing buildresults that result assembly will be removed.
                    // This is required otherwise new components can be compiled
                    // with obsolete build results whose assembly has been removed.
                    string assemblyKey = GetAssemblyCacheKey(f.FullName);
                    HttpRuntime.Cache.Remove(assemblyKey);

                    // Remove the assembly
                    RemoveAssembly(f);

                    // Also, remove satellite assemblies that may be associated with it
                    StandardDiskBuildResultCache.RemoveSatelliteAssemblies(assemblyName);
                }
                else if (f.Extension == dotDelete)
                {
                    CheckAndRemoveDotDeleteFile(f);
                }
                else
                {
                    // Remove the file, or if not possible, rename it, so it'll get
                    // cleaned up next time by RemoveOldTempFiles()

                    TryDeleteFile(f);
                }
            }
        }
        finally
        {
            // Always release the mutex if we had taken it
            if (gotLock)
            {
                CompilationLock.ReleaseLock();
            }

            DiskBuildResultCache.ShutDownAppDomainIfRequired();
        }
    }

    internal static void RemoveAssembly(FileInfo f)
    {

        // If we are shutting down, just create the .delete file and exit quickly.
        // TODO: Migration
        // if (HostingEnvironment.ShutdownInitiated) {
        //     CreateDotDeleteFile(f);
        //     return;
        // }

        // VSWhidbey 564168 / Visual Studio QFE 4710
        // The assembly could still be referenced and needed for compilation in some cases.
        // Thus, if we cannot delete it, we create an empty .delete file,
        // so that both will be later removed by RemoveOldTempFiles.

        // If the file is already marked for deletion, we simply return, so that
        // we do not double count it in s_recompilations.
        if (HasDotDeleteFile(f.FullName))
            return;

        if (TryDeleteFile(f))
            return;

        // It had to be renamed, so increment the recompilations count,
        // and restart the appdomain if it reaches the limit

        Debug.WriteLine("DiskBuildResultCache", "RemoveAssembly: " + f.Name + " was renamed");

        if (++s_recompilations == s_maxRecompilations)
        {
            s_shutdownStatus = SHUTDOWN_NEEDED;
        }

        // Remember the fact that we just invalidated an assembly, which can cause
        // other BuildResults to become invalid as a side effect (VSWhidbey 269297)
        s_inUseAssemblyWasDeleted = true;
    }

    static internal void ShutDownAppDomainIfRequired()
    {
        // VSWhidbey 610631 Stress Failure: Worker process throws exceptions while unloading app domain and re-tries over and over
        // It is possible for a deadlock to happen when locks on ApplicationManager and the CompilationMutex
        // are acquired in different orders in multiple threads.
        // Thus, since ShutdownAppDomain acquires a lock on ApplicationManager, we always release the CompilationMutex
        // before calling ShutdownAppDomain, in case another thread has acquired the lock on ApplicationManager and
        // is waiting on the CompilationMutex.


        if (s_shutdownStatus == SHUTDOWN_NEEDED &&
            (Interlocked.Exchange(ref s_shutdownStatus, SHUTDOWN_STARTED) == SHUTDOWN_NEEDED))
        {
            // Perform the actual shutdown on another thread, so that
            // this thread can proceed and release any compilation mutex it is
            // holding and not have to block if another thread has acquired a
            // lock on ApplicationManager.
            // (DevDiv 158814)
            ThreadPool.QueueUserWorkItem(new WaitCallback(DiskBuildResultCache.ShutdownCallBack));
        }
    }

    static private void ShutdownCallBack(Object state /*not used*/)
    {
        // TODO: Migration
        // HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.MaxRecompilationsReached,
        //     "Recompilation limit of " + s_maxRecompilations + " reached");
    }



    internal static bool TryDeleteFile(string s)
    {
        return TryDeleteFile(new FileInfo(s));
    }

    // Returns true if we are able to delete the file. Otherwise, creates a .delete file and returns false.
    internal static bool TryDeleteFile(FileInfo f)
    {
        if (f.Extension == dotDelete)
            return CheckAndRemoveDotDeleteFile(f);

        try
        {
            f.Delete();
            Debug.WriteLine("DiskBuildResultCache", "TryDeleteFile removed " + f.Name);
            return true;
        }
        catch
        {
        }

        CreateDotDeleteFile(f);
        return false;
    }

    // Checks if the file is .delete. If it is, check if the associated base file is still around.
    // If associated base file is around, try to delete it. If success, delete the .delete.
    // Returns true only if both base file and .delete are removed.
    internal static bool CheckAndRemoveDotDeleteFile(FileInfo f)
    {
        if (f.Extension != dotDelete)
            return false;

        string baseName = Path.GetDirectoryName(f.FullName) + Path.DirectorySeparatorChar +
                          Path.GetFileNameWithoutExtension(f.FullName);
        if (FileUtil.FileExists(baseName))
        {
            try
            {
                File.Delete(baseName);
                Debug.WriteLine("DiskBuildResultCache", "CheckAndRemoveDotDeleteFile deleted " + baseName);
            }
            catch
            {
                return false;
            }
        }

        try
        {
            f.Delete();
            Debug.WriteLine("DiskBuildResultCache", "CheckAndRemoveDotDeleteFile deleted " + f.Name);
        }
        catch
        {
        }

        return true;
    }

    internal static bool HasDotDeleteFile(string s)
    {
        return File.Exists(s + dotDelete);
    }

    private static void CreateDotDeleteFile(FileInfo f)
    {
        if (f.Extension == dotDelete)
            return;
        string newName = f.FullName + dotDelete;
        if (!File.Exists(newName))
        {
            try
            {
                (new StreamWriter(newName)).Close();
                Debug.WriteLine("DiskBuildResultCache", "CreateDotDeleteFile succeeded: " + newName);
            }
            catch
            {
                Debug.WriteLine("DiskBuildResultCache", "CreateDotDeleteFile failed: " + newName);
            } // If we fail the .delete probably just got created by another process.
        }
    }

}

internal class StandardDiskBuildResultCache : DiskBuildResultCache
{

    private const string fusionCacheDirectoryName = "assembly";
    private const string webHashDirectoryName = "hash";

    private static ArrayList _satelliteDirectories;

    internal StandardDiskBuildResultCache(string cacheDir)
        : base(cacheDir)
    {

        Debug.Assert(cacheDir == HttpRuntime2.CodegenDirInternal);

        EnsureDiskCacheDirectoryCreated();

        FindSatelliteDirectories();
    }

    private string GetSpecialFilesCombinedHashFileName()
    {
        return BuildManager.WebHashFilePath;
    }

    internal Tuple<long, long> GetPreservedSpecialFilesCombinedHash()
    {
        string fileName = GetSpecialFilesCombinedHashFileName();
        return GetPreservedSpecialFilesCombinedHash(fileName);
    }

    /*
     * Return the combined hash that was preserved to file.  Return 0 if not valid.
     */
    internal static Tuple<long, long> GetPreservedSpecialFilesCombinedHash(string fileName)
    {
        if (!FileUtil.FileExists(fileName))
        {
            return Tuple.Create<long, long>(0, 0);
        }

        try
        {
            string[] hashTokens = Util.StringFromFile(fileName)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            long value1, value2;
            if ((hashTokens.Length == 2) &&
                Int64.TryParse(hashTokens[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture,
                    out value1) &&
                Int64.TryParse(hashTokens[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture,
                    out value2))
            {
                return Tuple.Create(value1, value2);
            }

        }
        catch
        {
            // If anything went wrong (file not found, or bad format), return 0
        }

        return Tuple.Create<long, long>(0, 0);
    }

    internal void SavePreservedSpecialFilesCombinedHash(Tuple<long, long> hash)
    {
        string fileName = GetSpecialFilesCombinedHashFileName();
        SavePreservedSpecialFilesCombinedHash(fileName, hash);
    }

    /*
     * Preserve the combined hash of the special files to a file.
     */
    internal static void SavePreservedSpecialFilesCombinedHash(string hashFilePath, Tuple<long, long> hash)
    {

        Debug.Assert(hash != null && hash.Item1 != 0 && hash.Item2 != 0,
            "SavePreservedSpecialFilesCombinedHash: hash0 != 0, hash1 != 0");

        String hashDirPath = Path.GetDirectoryName(hashFilePath);

        // Create the hashweb directory if needed
        if (!FileUtil.DirectoryExists(hashDirPath))
        {
            Directory.CreateDirectory(hashDirPath);
        }

        using (var writer = new StreamWriter(hashFilePath, false, Encoding.UTF8))
        {
            writer.WriteLine(hash.Item1.ToString("x", CultureInfo.InvariantCulture));
            writer.WriteLine(';');
            writer.WriteLine(hash.Item2.ToString("x", CultureInfo.InvariantCulture));
        }
    }

    private void FindSatelliteDirectories()
    {

        Debug.Assert(_satelliteDirectories == null);

        //
        // Look for all the subdirectories of the codegen dir that look like
        // satellite assemblies dirs, and keep track of them
        //

        string[] subDirs = Directory.GetDirectories(_cacheDir);

        foreach (string subDir in subDirs)
        {
            string subDirName = Path.GetFileNameWithoutExtension(subDir);

            // Skip the fusion cache, since it's definitely not a culture (VSWhidbey 327716)
            if (subDirName == fusionCacheDirectoryName)
                continue;

            // Skip the "hash" folder
            if (subDirName == webHashDirectoryName)
                continue;

            if (Util.IsCultureName(subDirName))
            {
                if (_satelliteDirectories == null)
                    _satelliteDirectories = new ArrayList();

                _satelliteDirectories.Add(Path.Combine(_cacheDir, subDir));
            }
        }
    }

    internal static void RemoveSatelliteAssemblies(string baseAssemblyName)
    {

        if (_satelliteDirectories == null)
            return;

        //
        // If any satellite directory contains a satellite assembly that's
        // for the passed in assembly name, delete it.
        //

        string satelliteAssemblyName = baseAssemblyName + ".resources";

        foreach (string satelliteDir in _satelliteDirectories)
        {
            string fullAssemblyPath = Path.Combine(satelliteDir, satelliteAssemblyName);

            // Delete the DLL and PDB
            Util.DeleteFileIfExistsNoException(fullAssemblyPath + ".dll");
            Util.DeleteFileIfExistsNoException(fullAssemblyPath + ".pdb");
        }
    }


    private void RemoveCodegenResourceDir() {
        string path = BuildManager.CodegenResourceDir;
        Debug.WriteLine("BuildResultCache", "Deleting codegen temporary resource directory: " + path);
        if (Directory.Exists(path)){
            try {
                Directory.Delete(path, recursive:true);
            }
            catch { }
        }
    }

    /*
     * Delete all temporary files from the codegen directory (e.g. source files, ...)

     */
    internal void RemoveOldTempFiles() {
        Debug.WriteLine("BuildResultCache", "Deleting old temporary files from " + _cacheDir);

        RemoveCodegenResourceDir();

        string codegen = _cacheDir + "\\";

        // Go through all the files in the codegen dir
        foreach (FileData fileData in FileEnumerator.Create(codegen)) {

            // Skip directories
            if (fileData.IsDirectory) continue;

            // If it has a known extension, skip it
            string ext = Path.GetExtension(fileData.Name);
            if (ext == ".dll" || ext == ".pdb" || ext == ".web" || ext == ".ccu" || ext == ".prof" || ext == preservationFileExtension) {
                continue;
            }

            // .delete files need to be removed.
            if (ext != dotDelete) {
                // Don't delete the temp file if it's named after a dll that's still around
                // since it could still be useful for debugging.
                // Note that we can't use GetFileNameWithoutExtension here because
                // some of the files are named 5hvoxl6v.0.cs, and it would return
                // 5hvoxl6v.0 instead of just 5hvoxl6v
                int periodIndex = fileData.Name.LastIndexOf('.');
                if (periodIndex > 0) {
                    string baseName = fileData.Name.Substring(0, periodIndex);

                    int secondPeriodIndex = baseName.LastIndexOf('.');
                    if (secondPeriodIndex > 0) {
                        baseName = baseName.Substring(0, secondPeriodIndex);
                    }

                    // Generated source files uses assemblyname as prefix so we should keep them.
                    if (FileUtil.FileExists(codegen + baseName + ".dll"))
                        continue;

                    // other generated files, such as .cmdline, .err and .out need to add the
                    // WebAssemblyNamePrefix, since they do not use the assembly name as prefix.
                    if (FileUtil.FileExists(codegen + BuildManager.WebAssemblyNamePrefix + baseName + ".dll"))
                        continue;
                }
            }
            else {
                // Additional logic for VSWhidbey 564168 / Visual Studio QFE 4710.
                // Delete both original .dll and .delete if possible
                DiskBuildResultCache.CheckAndRemoveDotDeleteFile(new FileInfo(fileData.FullName));
                continue;
            }

            Debug.WriteLine("BuildResultCache", "Deleting old temporary files: " + fileData.FullName);
            try {
                File.Delete(fileData.FullName);
            } catch { }
        }
    }

    // private void RemoveCodegenResourceDir() {
    //     string path = BuildManager.CodegenResourceDir;
    //     Debug.WriteLine("BuildResultCache", "Deleting codegen temporary resource directory: " + path);
    //     if (Directory.Exists(path)){
    //         try {
    //             Directory.Delete(path, recursive:true);
    //         }
    //         catch { }
    //     }
    // }

    /*
     * Delete all the files in the codegen directory
     */
     [SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="System.Web.UnsafeNativeMethods.DeleteShadowCache(System.String,System.String)",
         Justification="Reviewed - we are just trying to clean up the codegen folder as much as possible, so it is ok to ignore any errors.")]
     internal void RemoveAllCodegenFiles() {
         Debug.WriteLine("BuildResultCache", "Deleting all files from " + _cacheDir);

         RemoveCodegenResourceDir();

         // Remove everything in the codegen directory, as well as all the subdirectories
         // used for culture assemblies

         // Go through all the files in the codegen dir.  Delete everything, except
         // for the fusion cache, which is in the "assembly" subdirectory
         foreach (FileData fileData in FileEnumerator.Create(_cacheDir)) {

             // If it's a directories
             if (fileData.IsDirectory) {

                 // Skip the fusion cache
                 if (fileData.Name == fusionCacheDirectoryName)
                     continue;

                 // Skip the "hash" folder
                 if (fileData.Name == webHashDirectoryName)
                     continue;

                 // Skip the source files generated for the designer (VSWhidbey 138194)
                 if (StringUtil.StringStartsWith(fileData.Name, CodeDirectoryCompiler.sourcesDirectoryPrefix))
                     continue;

                 try {
                     // If it is a directory, only remove the files inside and not the directory itself
                     // VSWhidbey 596757
                     DeleteFilesInDirectory(fileData.FullName);
                 }
                 catch { } // Ignore all exceptions

                 continue;
             }

             // VSWhidbey 564168 Do not delete files that cannot be deleted, these files are still
             // referenced by other appdomains that are in the process of shutting down.
             // We also do not rename as renaming can cause an assembly not to be found if another
             // appdomain tries to compile against it.
             DiskBuildResultCache.TryDeleteFile(fileData.FullName);
         }


         // Clean up the fusion shadow copy cache

         // Todo : Migration
         // AppDomainSetup appDomainSetup = Thread.GetDomain().SetupInformation;
         // UnsafeNativeMethods.DeleteShadowCache(appDomainSetup.CachePath,
         //     appDomainSetup.ApplicationName);
     }

    // Deletes all files in the directory, but leaves the directory there
    internal void DeleteFilesInDirectory(string path)
    {
        // Todo : Migration
        // Check if the directory exists
        if (!Directory.Exists(path))
        {
            return;
        }

        // Get all files in the directory and delete them
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            File.Delete(file);
        }

        // Get all directories and delete them
        var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        // Sort directories by their depth, ensuring we delete from the deepest child up to avoid access issues
        var sortedDirectories = directories.OrderByDescending(dir => dir.Length);
        foreach (var directory in sortedDirectories)
        {
            Directory.Delete(directory, false);
        }
    }
}

internal abstract class PrecompBaseDiskBuildResultCache: DiskBuildResultCache {

    // In precompilation, the preservation files go in the bin directory
    internal PrecompBaseDiskBuildResultCache(string cacheDir) : base(cacheDir) { }
}

// Used when precompiling a site
internal class PrecompilerDiskBuildResultCache: PrecompBaseDiskBuildResultCache {

    internal PrecompilerDiskBuildResultCache(string cacheDir) : base(cacheDir) {

        EnsureDiskCacheDirectoryCreated();
    }
}

// Used when precompiling a site using updatable precompilation
internal class UpdatablePrecompilerDiskBuildResultCache: PrecompilerDiskBuildResultCache {

    internal UpdatablePrecompilerDiskBuildResultCache(string cacheDir) : base(cacheDir) { }

    internal override void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart) {

        // Don't create preservation files in bin for pages in the updatable model,
        // because we turn them into a v1 style code behind, which works as a result of
        // having the aspx file point to the bin class via an inherits attribute.
        if (result is BuildResultCompiledTemplateType)
            return;

        base.CacheBuildResult(cacheKey, result, hashCode, utcStart);
    }

}

// Used when a site is already precompiled
internal class PrecompiledSiteDiskBuildResultCache: PrecompBaseDiskBuildResultCache {

    internal PrecompiledSiteDiskBuildResultCache(string cacheDir) : base(cacheDir) {}

    protected override bool PrecompilationMode { get { return true; } }

    internal override void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart) {

        // Nothing to cache to disk if the app is already precompiled
    }

    internal override void RemoveAssemblyAndRelatedFiles(string baseName) {
        // Never remove precompiled files (we couldn't anyways since they're
        // in the app dir)
    }
}
