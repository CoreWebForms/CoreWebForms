// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web;

internal sealed class VirtualPath
{
    public VirtualPath Parent => Directory.GetParent(Path)!.FullName;

    public VirtualPath(string path)
    {
        Path = path;
    }

    public string Path { get; }
    public string VirtualPathStringNoTrailingSlash { get; internal set; }

    public static implicit operator VirtualPath(string path) => new(path);
    public static implicit operator string(VirtualPath vpath) => vpath.Path;

    public static VirtualPath CreateAllowNull(string path) => new(path);

    internal static VirtualPath CreateNonRelativeAllowNull(string v)
    {
        throw new NotImplementedException();
    }

    internal static string GetAppRelativeVirtualPathStringOrEmpty(VirtualPath templateControlVirtualDirectory)
    {
        throw new NotImplementedException();
    }
}
