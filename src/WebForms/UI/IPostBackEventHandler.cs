// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Controls that will generate postback events from the client should implement this interface.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI;

/// <devdoc>
///    <para> Defines the contract that controls must implement to
///       handle low-level post back events.</para>
/// </devdoc>
public interface IPostBackEventHandler
{
    /*
     * Process the event that this control wanted fired from a form post back.
     */

    /// <devdoc>
    ///    <para>
    ///       Enables a control to process the event fired by a form post back.
    ///    </para>
    /// </devdoc>
    void RaisePostBackEvent(string? eventArgument);
}
