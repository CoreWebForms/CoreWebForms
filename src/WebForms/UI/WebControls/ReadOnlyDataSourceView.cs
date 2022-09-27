// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System.Collections;
using System.Web.UI;

/// <devdoc>
/// This class is used by ReadOnlyDataSource to represent an individual
/// view of a generic data source.
/// </devdoc>
internal sealed class ReadOnlyDataSourceView : DataSourceView
{

    private readonly IEnumerable _dataSource;

    public ReadOnlyDataSourceView(ReadOnlyDataSource owner, string name, IEnumerable dataSource) : base(owner, name)
    {
        _dataSource = dataSource;
    }

    protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
    {
        arguments.RaiseUnsupportedCapabilitiesError(this);
        return _dataSource;
    }
}

