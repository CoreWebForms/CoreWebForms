// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{

    /// <summary>
    /// Defines methods for transformation of bundle item contents
    /// </summary>
    public interface IItemTransform
    {
        /// <summary>
        /// Transforms the input string and returns the output
        /// </summary>
        /// <param name="includedVirtualPath">The virtual path that was included in the bundle for this item that is being transformed</param>
        /// <param name="input"></param>
        string Process(string includedVirtualPath, string input);
    }

}
