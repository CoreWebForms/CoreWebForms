// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System.Collections.Specialized;

public interface IPostBackDataHandler
{
    bool LoadPostData(string postDataKey, NameValueCollection postCollection);

    void RaisePostDataChangedEvent();
}
