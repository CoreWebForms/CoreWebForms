// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace System.Web.UI {
    public interface IDataSource {

        event EventHandler DataSourceChanged;

        DataSourceView GetView(string viewName);

        ICollection GetViewNames();
    }
}

