// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// OptimizationMode used by IgnoreList and FileExtensionReplacement.
    /// </summary>
    public enum OptimizationMode
    {
        /// <summary>
        /// Always: Always ignore
        /// </summary>
        Always,

        /// <summary>
        /// WhenEnabled: Only when <see cref="BundleTable.EnableOptimizations"/> = true
        /// </summary>
        WhenEnabled,

        /// <summary>
        /// WhenDisabled: Only when <see cref="BundleTable.EnableOptimizations"/> = false
        /// </summary>
        WhenDisabled
    }
}
