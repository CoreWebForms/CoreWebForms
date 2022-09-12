// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;
/// <devdoc>
/// Implemented by objects that have context information about thier own
/// location (or URL) and can resolve relative URLs based on that.
/// </devdoc>
public interface IUrlResolutionService
{

    /// <devdoc>
    /// Return a resolved URL that is suitable for use on the client.
    /// If the specified URL is absolute, it is returned unchanged.
    /// Otherwise, it is turned into a relative URL that is based
    /// on the current request path (which the browser then resolves
    /// to get a complete URL).
    /// </devdoc>
    string ResolveClientUrl(string relativeUrl);
}
