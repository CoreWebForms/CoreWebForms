// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel;

namespace System.Web.UI.WebControls;
public sealed class PostBackOptions
{
    public PostBackOptions(Control targetControl) :
        this(targetControl, null, null, false, false, false, true, false, null)
    {
    }

    public PostBackOptions(Control targetControl, string argument) :
        this(targetControl, argument, null, false, false, false, true, false, null)
    {
    }

    public PostBackOptions(Control targetControl, string? argument, string? actionUrl, bool autoPostBack,
         bool requiresJavaScriptProtocol, bool trackFocus, bool clientSubmit, bool performValidation, string? validationGroup)
    {

        if (targetControl == null)
        {
            throw new ArgumentNullException(nameof(targetControl));
        }

        ActionUrl = actionUrl;
        Argument = argument;
        AutoPostBack = autoPostBack;
        ClientSubmit = clientSubmit;
        RequiresJavaScriptProtocol = requiresJavaScriptProtocol;
        PerformValidation = performValidation;
        TrackFocus = trackFocus;
        TargetControl = targetControl;
        ValidationGroup = validationGroup;
    }

    [DefaultValue("")]
    public string? ActionUrl { get; set; }

    [DefaultValue("")]
    public string? Argument { get; set; }

    [DefaultValue(false)]
    public bool AutoPostBack { get; set; }

    [DefaultValue(true)]
    public bool ClientSubmit { get; set; } = true;

    [DefaultValue(true)]
    public bool RequiresJavaScriptProtocol { get; set; }

    [DefaultValue(false)]
    public bool PerformValidation { get; set; }

    [DefaultValue("")]
    public string? ValidationGroup { get; set; }

    [DefaultValue(null)]
    public Control TargetControl { get; }

    [DefaultValue(false)]
    public bool TrackFocus { get; set; }
}
