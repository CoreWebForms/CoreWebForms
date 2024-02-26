//------------------------------------------------------------------------------
// <copyright file="WebReferencesBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Net;
using System.Xml.Serialization;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;
using Util=System.Web.UI.Util;

internal class WebReferencesBuildProvider: BuildProvider {

    private VirtualDirectory _vdir;

    private const string IndigoWebRefProviderTypeName = "System.Web.Compilation.WCFBuildProvider";
    private static Type s_indigoWebRefProviderType;
    private static bool s_triedToGetWebRefType;

    internal WebReferencesBuildProvider(VirtualDirectory vdir) {
        _vdir = vdir;
    }

    public override void GenerateCode(AssemblyBuilder assemblyBuilder)  {

        // Only attempt to get the Indigo provider once
        if (!s_triedToGetWebRefType) {
            // TODO: Migration
            // s_indigoWebRefProviderType = BuildManager.GetType(IndigoWebRefProviderTypeName, false /*throwOnError*/);
            s_indigoWebRefProviderType = Type.GetType(IndigoWebRefProviderTypeName, false /*throwOnError*/);
            s_triedToGetWebRefType = true;
        }

        // If we have an Indigo provider, instantiate it and forward the GenerateCode call to it
        if (s_indigoWebRefProviderType != null) {
            // TODO: Migration
            // BuildProvider buildProvider = (BuildProvider)HttpRuntime.CreateNonPublicInstance(s_indigoWebRefProviderType);
            BuildProvider buildProvider = (BuildProvider)Activator.CreateInstance(s_indigoWebRefProviderType);
            buildProvider.SetVirtualPath(VirtualPathObject);
            buildProvider.GenerateCode(assemblyBuilder);
        }

        // e.g "/MyApp/Application_WebReferences"
        VirtualPath rootWebRefDirVirtualPath = HttpRuntimeConsts.WebRefDirectoryVirtualPath;

        // e.g "/MyApp/Application_WebReferences/Foo/Bar"
        string currentWebRefDirVirtualPath = _vdir.VirtualPath;

        Debug.Assert(StringUtil.StringStartsWithIgnoreCase(
            currentWebRefDirVirtualPath, rootWebRefDirVirtualPath.VirtualPathString));

        string ns;

        if (rootWebRefDirVirtualPath.VirtualPathString.Length == currentWebRefDirVirtualPath.Length) {
            // If it's the root WebReferences dir, use the empty namespace
            ns = String.Empty;
        }
        else {
            // e.g. "Foo/Bar"
            // TODO: Migration
            // Debug.Assert(rootWebRefDirVirtualPath.HasTrailingSlash);
            currentWebRefDirVirtualPath = UrlPath.RemoveSlashFromPathIfNeeded(currentWebRefDirVirtualPath);
            currentWebRefDirVirtualPath = currentWebRefDirVirtualPath.Substring(
                rootWebRefDirVirtualPath.VirtualPathString.Length);

            // Split it into chunks separated by '/'
            string[] chunks = currentWebRefDirVirtualPath.Split('/');

            // Turn all the relevant chunks into valid namespace chunks
            for (int i=0; i<chunks.Length; i++) {
                chunks[i] = Util.MakeValidTypeNameFromString(chunks[i]);
            }

            // Put the relevant chunks back together to form the namespace
            ns = String.Join(".", chunks);
        }
    }
}

}
