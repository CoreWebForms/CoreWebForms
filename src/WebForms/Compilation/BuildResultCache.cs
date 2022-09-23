// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Util;

namespace System.Web.Compilation;
internal abstract class BuildResultCache
{
    internal BuildResult GetBuildResult(string cacheKey)
    {
        return GetBuildResult(cacheKey, null /*virtualPath*/, 0 /*hashCode*/);
    }

    internal abstract BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate = true);

    internal void CacheBuildResult(string cacheKey, BuildResult result, DateTime utcStart)
    {
        CacheBuildResult(cacheKey, result, 0 /*hashCode*/, utcStart);
    }

    internal abstract void CacheBuildResult(string cacheKey, BuildResult result,
        long hashCode, DateTime utcStart);

    internal static string GetAssemblyCacheKey(string assemblyPath)
    {
        string assemblyName = UI.Util.GetAssemblyNameFromFileName(Path.GetFileName(assemblyPath));
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
        return CacheInternal.PrefixAssemblyPath + assemblyName.ToLowerInvariant();
    }
}
