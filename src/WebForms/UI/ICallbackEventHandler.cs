// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;

public interface ICallbackEventHandler
{

    /// <devdoc>
    /// Process the eventargs that this control wanted fired from an out-of-band callback
    /// </devdoc>
    void RaiseCallbackEvent(string eventArgument);

    /// <devdoc>
    /// Render the control for the callback event
    /// </devdoc>
    string GetCallbackResult();
}
