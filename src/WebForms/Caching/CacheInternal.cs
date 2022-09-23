// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// src - https://github.com/microsoft/referencesource/blob/master/System.Web/Cache/CacheInternal.cs

namespace System.Web.Caching;

class CacheCommon
{
    private int _disposed = 0;

    internal void Dispose(bool disposing)
    {
        if (disposing)
        {
            // This method must be tolerant to multiple calls to Dispose on the same instance
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                // todo
                //EnableCacheMemoryTimer(false);
                //_cacheSizeMonitor.Dispose();
            }
        }
    }
}

abstract class CacheInternal : IDisposable
{
    // cache key prefixes - they keep cache keys short and prevent conflicts

    // NOTE: Since we already used up all the lowercase letters from 'a' to 'z',
    // we are now using uppercase letters from 'A' to 'Z'
    internal const string PrefixFIRST = "A";
    internal const string PrefixResourceProvider = "A";
    internal const string PrefixMapPathVPPFile = "Bf";
    internal const string PrefixMapPathVPPDir = "Bd";

    // Next prefix goes here, until we get to 'Z'

    internal const string PrefixOutputCache = "a";
    internal const string PrefixSqlCacheDependency = "b";
    internal const string PrefixMemoryBuildResult = "c";
    internal const string PrefixPathData = "d";
    internal const string PrefixHttpCapabilities = "e";
    internal const string PrefixMapPath = "f";
    internal const string PrefixHttpSys = "g";
    internal const string PrefixFileSecurity = "h";
    internal const string PrefixInProcSessionState = "j";
    internal const string PrefixStateApplication = "k";
    internal const string PrefixPartialCachingControl = "l";
    internal const string UNUSED = "m";
    internal const string PrefixAdRotator = "n";
    internal const string PrefixWebServiceDataSource = "o";
    internal const string PrefixLoadXPath = "p";
    internal const string PrefixLoadXml = "q";
    internal const string PrefixLoadTransform = "r";
    internal const string PrefixAspCompatThreading = "s";
    internal const string PrefixDataSourceControl = "u";
    internal const string PrefixValidationSentinel = "w";
    internal const string PrefixWebEventResource = "x";
    internal const string PrefixAssemblyPath = "y";
    internal const string PrefixBrowserCapsHash = "z";
    internal const string PrefixLAST = "z";

    protected CacheCommon _cacheCommon;
    internal int _refCount = 0;
    private int _disposed;

    protected virtual void Dispose(bool disposing)
    {
        _disposed = 1;
        _cacheCommon.Dispose(disposing);
    }

    public void Dispose()
    {
        Debug.Assert(_refCount >= 0);
        if (_refCount <= 0)
        {
            Dispose(true);
            // no destructor, don't need it.
            // System.GC.SuppressFinalize(this);
        }
    }

    internal bool IsDisposed { get { return _disposed == 1; } }
}
