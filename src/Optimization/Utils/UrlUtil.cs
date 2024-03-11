// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    internal static class UrlUtil
    {
        internal static string Url(string basePath, string path)
        {
            if (basePath != null)
            {
                path = VirtualPathUtility.Combine(basePath, path);
            }

            // Make sure it's not a ~/ path, which the client couldn't handle
            path = VirtualPathUtility.ToAbsolute(path);
            return HttpUtility.UrlPathEncode(path);
        }
    }
}
