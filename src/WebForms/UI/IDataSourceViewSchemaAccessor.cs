// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

/// <devdoc>
/// Allows a TypeConverter to access schema information stored on an object
/// </devdoc>
public interface IDataSourceViewSchemaAccessor
{

    /// <devdoc>
    /// Returns the schema associated with the object implementing this interface.
    /// </devdoc>
    object DataSourceViewSchema { get; set; }
}

