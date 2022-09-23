//------------------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;

namespace System.Web.UI {
    public interface IDataSource {

        event EventHandler DataSourceChanged;

        DataSourceView GetView(string viewName);

        ICollection GetViewNames();
    }
}

