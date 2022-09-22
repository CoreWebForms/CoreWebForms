// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.Configuration;

public sealed class CodeSubDirectory : ConfigurationElement
{
    private const string dirNameAttribName = "directoryName";

    private static ConfigurationPropertyCollection _properties;
    private static readonly ConfigurationProperty _propDirectoryName =
        new ConfigurationProperty(dirNameAttribName,
                                    typeof(string),
                                    null,
                                    StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                    StdValidatorsAndConverters.NonEmptyStringValidator,
                                    ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

    static CodeSubDirectory()
    {
        _properties = new ConfigurationPropertyCollection();
        _properties.Add(_propDirectoryName);
    }


    internal CodeSubDirectory()
    {
    }

    public CodeSubDirectory(string directoryName)
    {
        DirectoryName = directoryName;
    }

    protected override ConfigurationPropertyCollection Properties
    {
        get
        {
            return _properties;
        }
    }

    [ConfigurationProperty(dirNameAttribName, IsRequired = true, IsKey = true, DefaultValue = "")]
    [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
    public string DirectoryName
    {
        get
        {
            return (string)base[_propDirectoryName];
        }
        set
        {
            base[_propDirectoryName] = value;
        }
    }

    // The assembly is named after the directory
    internal string AssemblyName { get { return DirectoryName; } }

    // Validate the element for runtime use
    internal void DoRuntimeValidation()
    {
        // todo
        //string directoryName = DirectoryName;

        //// If the app is precompiled, don't attempt further validation, sine the directory
        //// will not actually exist (VSWhidbey 394333)
        //if (BuildManager.IsPrecompiledApp)
        //{
        //    return;
        //}

        //// Make sure it's just a valid simple directory name
        //if (!Util.IsValidFileName(directoryName))
        //{
        //    throw new ConfigurationErrorsException(
        //        SR.GetString(SR.Invalid_CodeSubDirectory, directoryName),
        //        ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
        //}

        //VirtualPath codeVirtualSubDir = HttpRuntime.CodeDirectoryVirtualPath.SimpleCombineWithDir(directoryName);

        //// Make sure the specified directory exists
        //if (!VirtualPathProvider.DirectoryExistsNoThrow(codeVirtualSubDir))
        //{
        //    throw new ConfigurationErrorsException(
        //        SR.GetString(SR.Invalid_CodeSubDirectory_Not_Exist, codeVirtualSubDir),
        //        ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
        //}

        //// Look at the actual physical dir to get its name canonicalized (VSWhidbey 288568)
        //string physicalDir = codeVirtualSubDir.MapPathInternal();
        //FindFileData ffd;
        //FindFileData.FindFile(physicalDir, out ffd);

        //// If the name was not canonical, reject it
        //if (!StringUtil.EqualsIgnoreCase(directoryName, ffd.FileNameLong))
        //{
        //    throw new ConfigurationErrorsException(
        //        SR.GetString(SR.Invalid_CodeSubDirectory, directoryName),
        //        ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
        //}

        //if (BuildManager.IsReservedAssemblyName(directoryName))
        //{
        //    throw new ConfigurationErrorsException(
        //        SR.GetString(SR.Reserved_AssemblyName, directoryName),
        //        ElementInformation.Properties[dirNameAttribName].Source, ElementInformation.Properties[dirNameAttribName].LineNumber);
        //}
    }
}
