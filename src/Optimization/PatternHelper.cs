// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    internal enum PatternType
    {
        Exact = 0, // Pattern has no wildcards
        All = 1, // Pattern is exactly '*'
        Suffix = 2, // Pattern starts with a *, i.e. *.js
        Prefix = 3, // Pattern ends with a *, i.e. jquery*
        Version = 4 // Pattern contains a {version}, not allowed to have *
    }

    /// <summary>
    /// Helper that does validation for patterns to ensure they are supported
    /// </summary>
    internal static class PatternHelper
    {
        internal const string VersionToken = "{version}";
        private const RegexOptions _flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        internal const string VersionRegEx = @"(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?";

        internal static PatternType GetPatternType(string input)
        {
            if (input.Contains(VersionToken))
            {
                return PatternType.Version;
            }
            if (!input.Contains('*'))
            {
                return PatternType.Exact;
            }
            if (input.Length == 1)
            { // "*" is everything
                return PatternType.All;
            }
            if (input.StartsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                return PatternType.Suffix;
            }
            else
            {
                return PatternType.Prefix;
            }
        }

        /// <summary>
        /// Returns a regex that replaces the {version} with the actual regex
        /// Assumption is this input has had ValidatePattern called and is a Regex
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static Regex BuildRegex(string input)
        {
            // Need to escape the string before building the regex, need to swap out {version} with something that doesn't get escaped
            input = input.Replace(VersionToken, "<version>");
            input = Regex.Escape(input);
            input = input.Replace("<version>", VersionRegEx);
            // Make sure the pattern is anchored
            return new Regex("^" + input + "$", _flags);
        }

        /// <summary>
        /// Returns a regex that replaces the * with the actual regex
        /// Assumption is this input has had ValidatePattern called and is a Regex
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static Regex BuildWildcardRegex(string input)
        {
            // Need to escape the string before building the regex
            input = input.Replace("*", "<star>");
            input = Regex.Escape(input);
            input = input.Replace("<star>", ".*");
            // Make sure the pattern is anchored
            return new Regex("^" + input + "$", RegexOptions.IgnoreCase);
        }

        internal static Exception ValidatePattern(PatternType type, string pattern, string argumentName)
        {
            switch (type)
            {
                case PatternType.All:
                case PatternType.Exact:
                    // No validation needed for these cases, also not going to be hit
                    break;
                case PatternType.Prefix:
                    var prefix = pattern.Substring(0, pattern.Length - 1);
                    if (prefix.Contains('*'))
                    {
                        return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.InvalidPattern, pattern), argumentName);
                    }
                    break;
                case PatternType.Suffix:
                    var suffix = pattern.Substring(1);
                    if (suffix.Contains('*'))
                    {
                        return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.InvalidPattern, pattern), argumentName);
                    }
                    break;
                case PatternType.Version:
                    if (pattern.Contains('*'))
                    {
                        return new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.InvalidPattern, pattern), argumentName);
                    }
                    break;

            }
            return null;
        }
    }
}
