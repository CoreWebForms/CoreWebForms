// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace System.Web;
internal class MTConfigUtil
{
    // We only need to use the root config of 2.0 if we are building (and
    // not during runtime) and targeting 2.0 or 3.5.
    static private bool? s_useMTConfig;
    static private bool UseMTConfig
    {
        get
        {
            //if (s_useMTConfig == null)
            //{
            //    s_useMTConfig = BuildManagerHost.InClientBuildManager &&
            //        (MultiTargetingUtil.IsTargetFramework20 || MultiTargetingUtil.IsTargetFramework35);
            //}
            s_useMTConfig = false;

            return s_useMTConfig.Value;
        }
    }

    internal static CompilationSection GetCompilationAppConfig()
    {
        return null;
    }
}
