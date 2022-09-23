// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Interface implemented by objects that need to expose string properties
 * with arbitrary names.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public interface IAttributeAccessor {
    /*
     * Get the string value of a named property
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    string GetAttribute(string key);

    /*
     * Set a named property with a string value
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    void SetAttribute(string key, string value);

}

}
