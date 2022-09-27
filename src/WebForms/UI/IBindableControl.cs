// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

using System.Collections.Specialized;

public interface IBindableControl
{

    /// <devdoc>
    /// Retrives the values of all control properties with two-way bindings.
    /// </devdoc>
    void ExtractValues(IOrderedDictionary dictionary);

}

