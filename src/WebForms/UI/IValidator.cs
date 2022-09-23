// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI;

/// <devdoc>
///    <para>Defines the contract that the validation controls must implement.</para>
/// </devdoc>
public interface IValidator
{

    /// <devdoc>
    ///    <para>Indicates whether the content entered in a control is valid.</para>
    /// </devdoc>
    bool IsValid
    {
        get;
        set;
    }

    /// <devdoc>
    ///    <para>Indicates the error message text generated when the control's content is not 
    ///       valid.</para>
    /// </devdoc>
    string ErrorMessage
    {
        get;
        set;
    }

    /// <devdoc>
    ///    <para>Compares the entered content with the valid parameters provided by the 
    ///       validation control.</para>
    /// </devdoc>
    void Validate();
}

