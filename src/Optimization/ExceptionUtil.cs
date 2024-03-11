// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    internal static class ExceptionUtil
    {
        internal static ArgumentException ParameterNullOrEmpty(string parameter)
        {
            return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.Parameter_NullOrEmpty, parameter), parameter);
        }

        internal static ArgumentException PropertyNullOrEmpty(string property)
        {
            return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.Property_NullOrEmpty, property), property);
        }

        internal static Exception ValidateVirtualPath(string virtualPath, string argumentName)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                return ExceptionUtil.ParameterNullOrEmpty(argumentName);
            }
            if (!virtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.UrlMappings_only_app_relative_url_allowed, virtualPath), argumentName);
            }
            return null;
        }

        // DevDiv2 175919: For security purposes, we block * and *.* pure wildcards to prevent unintentional access to config/code etc
        internal static bool IsPureWildcardSearchPattern(string searchPattern)
        {
            if (!string.IsNullOrEmpty(searchPattern))
            {
                var trimmed = searchPattern.Trim();
                if (string.Equals(trimmed, "*", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "*.*", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

