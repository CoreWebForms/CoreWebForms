// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Controls that can accept postback data should implement this interface.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI
{

    using System;
    using System.Collections;
    using System.Collections.Specialized;

    /// <devdoc>
    ///    <para>Defines the contract that controls must implement in order to
    ///       automatically load post back data.</para>
    /// </devdoc>
    public interface IPostBackDataHandler
    {
        /*
         * Processes the post data returned from the client for this control.
         * Answer true if the post data causes our state to change.
         */

        /// <devdoc>
        ///    <para>Processes the post back data for the specified control. </para>
        ///    </devdoc>
        bool LoadPostData(string postDataKey, NameValueCollection postCollection);

        /*
         * Notify any listeners that our state has changed as the result of
         * a post back.
         */

        /// <devdoc>
        ///    <para>Signals the control to notify any listeners that the state of the
        ///       control has changed.</para>
        /// </devdoc>
        void RaisePostDataChangedEvent();
    }

}
