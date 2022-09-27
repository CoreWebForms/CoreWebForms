// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System;

/// <devdoc>
///  Derived version of ImageButton used within a DataControl.
/// </devdoc>
[SupportsEventValidation]
internal sealed class DataControlImageButton : ImageButton
{
    readonly IPostBackContainer _container;
    string _callbackArgument;
    bool _enableCallback;

    internal DataControlImageButton(IPostBackContainer container)
    {
        _container = container;
    }

    public override bool CausesValidation
    {
        get
        {
            return false;
        }
        set
        {
            throw new NotSupportedException(SR.GetString(SR.CannotSetValidationOnDataControlButtons));
        }
    }

    internal void EnableCallback(string argument)
    {
        _enableCallback = true;
        _callbackArgument = argument;
    }

    protected sealed override PostBackOptions GetPostBackOptions()
    {
        if (_container != null)
        {
            return _container.GetPostBackOptions(this);
        }

        return base.GetPostBackOptions();
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        SetCallbackProperties();
        base.Render(writer);
    }

    private void SetCallbackProperties()
    {
        if (_enableCallback)
        {
            ICallbackContainer _callbackContainer = _container as ICallbackContainer;
            if (_callbackContainer != null)
            {
                string callbackScript = _callbackContainer.GetCallbackScript(this, _callbackArgument);
                if (!String.IsNullOrEmpty(callbackScript))
                {
                    this.OnClientClick = callbackScript;
                }
            }
        }
    }
}

