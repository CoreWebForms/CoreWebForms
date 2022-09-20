// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Web.UI.WebControls;

class RequiredFieldValidator : BaseValidator
{

    public string InitialValue
    {
        get
        {
            object o = ViewState["InitialValue"];
            return ((o == null) ? String.Empty : (string)o);
        }
        set
        {
            ViewState["InitialValue"] = value;
        }
    }

    protected override void AddAttributesToRender(HtmlTextWriter writer)
    {
        base.AddAttributesToRender(writer);
        if (RenderUplevel)
        {
            string id = ClientID;
            HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;
            AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
            AddExpandoAttribute(expandoAttributeWriter, id, "initialvalue", InitialValue);
        }
    }

    protected override bool EvaluateIsValid()
    {
        // Get the control value, return true if it is not found
        string controlValue = GetControlValidationValue(ControlToValidate);
        if (controlValue == null)
        {
            Debug.Fail("Should have been caught by PropertiesValid check");
            return true;
        }

        // See if the control has changed
        return (!controlValue.Trim().Equals(InitialValue.Trim()));
    }
}
