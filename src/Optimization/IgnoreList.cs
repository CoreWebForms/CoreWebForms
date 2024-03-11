// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    /// <summary>
    /// A list of filename patterns to be ignored and thereby excluded from bundles.
    /// </summary>
    public sealed class IgnoreList
    {
        private HashSet<string> _exactAlways;
        private HashSet<string> _exactWhenOptimized;
        private HashSet<string> _exactWhenUnoptimized;
        private List<IgnoreMatch> _matches;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreList"/> class.
        /// </summary>
        public IgnoreList()
        {
            InitializeMatches();
        }

        /// <summary>
        /// Clears entire ignore list.
        /// </summary>
        public void Clear()
        {
            InitializeMatches();
        }

        private void InitializeMatches()
        {
            _exactAlways = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _exactWhenOptimized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _exactWhenUnoptimized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _matches = new List<IgnoreMatch>();
        }

        /// <summary>
        /// Ignores the specified pattern regardless of the value set in <see cref="BundleTable.EnableOptimizations"/>
        /// </summary>
        /// <param name="item">The ignore pattern.</param>
        public void Ignore(string item)
        {
            Ignore(item, OptimizationMode.Always);
        }

        private static Exception ValidateIgnoreMode(OptimizationMode mode, string argName)
        {
            switch (mode)
            {
                case OptimizationMode.Always:
                case OptimizationMode.WhenEnabled:
                case OptimizationMode.WhenDisabled:
                    return null;
                default:
                    return new ArgumentException(OptimizationResources.InvalidOptimizationMode, argName);
            }
        }

        /// <summary>
        /// Ignores the specified pattern for a specific <see cref="OptimizationMode"/>.
        /// </summary>
        /// <param name="pattern">The ignore pattern.</param>
        /// <param name="mode"><see cref="OptimizationMode"/> in which to apply the ignore pattern.</param>
        public void Ignore(string pattern, OptimizationMode mode)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("pattern");
            }
            var type = PatternHelper.GetPatternType(pattern);
            var error = PatternHelper.ValidatePattern(type, pattern, "item");
            if (error != null)
            {
                throw error;
            }
            error = ValidateIgnoreMode(mode, "mode");
            if (error != null)
            {
                throw error;
            }

            switch (type)
            {
                case PatternType.All:
                    _matches.Add(new AllMatch(mode));
                    break;
                case PatternType.Exact:
                    switch (mode)
                    {
                        case OptimizationMode.Always:
                            _exactAlways.Add(pattern);
                            break;
                        case OptimizationMode.WhenEnabled:
                            _exactWhenOptimized.Add(pattern);
                            break;
                        case OptimizationMode.WhenDisabled:
                            _exactWhenUnoptimized.Add(pattern);
                            break;

                    }
                    break;
                case PatternType.Prefix:
                    _matches.Add(new PrefixMatch(pattern.Substring(0, pattern.Length - 1), mode));
                    break;
                case PatternType.Suffix:
                    _matches.Add(new SuffixMatch(pattern.Substring(1), mode));
                    break;
                case PatternType.Version:
                    _matches.Add(new VersionMatch(pattern, mode));
                    break;
            }
        }

        /// <summary>
        /// Determines whether a file should be ignored based on the ignore list. 
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="fileName">The name of the file to compare with the ignore list.</param>
        /// <returns>True if the <paramref name="fileName"/> matches a pattern in the <see cref="IgnoreList"/>.</returns>
        public bool ShouldIgnore(BundleContext context, string fileName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var optimizationEnabled = context.EnableOptimizations;
            // ignore if the filename is empty, or we find an exact match
            if (string.IsNullOrEmpty(fileName) ||
                _exactAlways.Contains(fileName) ||
                (optimizationEnabled && _exactWhenOptimized.Contains(fileName)) ||
                (!optimizationEnabled && _exactWhenUnoptimized.Contains(fileName)))
            {
                return true;
            }

            // Otherwise try pattern matching if they are used in the optimization mode
            return _matches.Any(m => m.UseMatch(optimizationEnabled) && m.IsMatch(fileName));
        }

        /// <summary>
        /// Filters a set of files and returns a new set that excludes ignored files.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="files">Set of input files to compare with the ignore list.</param>
        /// <returns>Set of files with ignored files excluded.</returns>
        public IEnumerable<BundleFile> FilterIgnoredFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files.Where(f => !ShouldIgnore(context, f.VirtualFile.Name));
        }

        /// <summary>
        /// Helper classes that contain the logic to do prefix/sufix/all matching
        /// </summary>
        private abstract class IgnoreMatch
        {
            public IgnoreMatch()
            {
            }

            public IgnoreMatch(string pattern, OptimizationMode mode)
            {
                Mode = mode;
                Pattern = pattern;
            }

            public OptimizationMode Mode { get; set; }
            public string Pattern { get; set; }

            /// <summary>
            /// Returns true if the match should be used in this optimization mode
            /// </summary>
            /// <param name="optimizationMode"></param>
            /// <returns></returns>
            public bool UseMatch(bool optimizationMode)
            {
                switch (Mode)
                {
                    case OptimizationMode.Always:
                        return true;
                    case OptimizationMode.WhenEnabled:
                        return optimizationMode;
                    case OptimizationMode.WhenDisabled:
                        return !optimizationMode;
                }
                return false;
            }

            public abstract bool IsMatch(string input);
        }

        private sealed class AllMatch : IgnoreMatch
        {
            public AllMatch(OptimizationMode mode)
            {
                Mode = mode;
            }

            public override bool IsMatch(string input)
            {
                return true;
            }
        }

        private sealed class PrefixMatch : IgnoreMatch
        {
            public PrefixMatch(string pattern, OptimizationMode mode) : base(pattern, mode)
            {
            }

            public override bool IsMatch(string input)
            {
                return input.StartsWith(Pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class SuffixMatch : IgnoreMatch
        {
            public SuffixMatch(string pattern, OptimizationMode mode) : base(pattern, mode)
            {
            }

            public override bool IsMatch(string input)
            {
                return input.EndsWith(Pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class VersionMatch : IgnoreMatch
        {
            public VersionMatch(string pattern, OptimizationMode mode) : base(pattern, mode)
            {
                RegEx = PatternHelper.BuildRegex(Pattern);
            }

            private Regex RegEx { get; set; }

            public override bool IsMatch(string input)
            {
                return RegEx.IsMatch(input);
            }
        }

    }
}
