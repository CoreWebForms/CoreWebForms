//------------------------------------------------------------------------------
// <copyright file="WizardStep.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace System.Web.UI.WebControls
{
    [
    Bindable(false),
    ControlBuilderAttribute(typeof(WizardStepControlBuilder)),
    ToolboxItem(false)
    ]

    public sealed class WizardStep : WizardStepBase {
    }
}
