// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace System.Web.Compilation;

internal class CompilationUtil
{
    internal static CodeSubDirectoriesCollection GetCodeSubDirectories()
    {
        // Get the <compilation> config object
        CompilationSection config = MTConfigUtil.GetCompilationAppConfig();

        CodeSubDirectoriesCollection codeSubDirectories = null;// config.CodeSubDirectories;

        // Make sure the config data is valid
        if (codeSubDirectories != null)
        {
            codeSubDirectories.EnsureRuntimeValidation();
        }

        return codeSubDirectories;
    }
}
