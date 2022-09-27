// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

using System.Collections;

public delegate void DataSourceViewSelectCallback(IEnumerable data);

// returns whether the exception was handled
public delegate bool DataSourceViewOperationCallback(int affectedRecords, Exception ex);
