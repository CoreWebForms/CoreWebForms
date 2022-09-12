// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Interface implemented by objects that support state management.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI;

/// <devdoc>
///    <para>Defines the contract that controls must implement to support state 
///       management.</para>
/// </devdoc>
public interface IStateManager
{
    /*
     * Return true if tracking state changes.
     */

    /// <devdoc>
    ///    <para>Determines if state changes are being tracked.</para>
    ///    </devdoc>
    bool IsTrackingViewState
    {
        get;
    }

    /*
     * Load previously saved state.
     */

    /// <devdoc>
    ///    <para>Loads the specified control's previously saved state.</para>
    ///    </devdoc>
    void LoadViewState(object state);

    /*
     * Return object containing state changes.
     */

    /// <devdoc>
    ///    <para>Returns the object that contains the state changes.</para>
    ///    </devdoc>
    object SaveViewState();

    /*
     * Start tracking state changes.
     */

    /// <devdoc>
    ///    <para>Instructs the control to start tracking changes in state.</para>
    ///    </devdoc>
    void TrackViewState();

}

