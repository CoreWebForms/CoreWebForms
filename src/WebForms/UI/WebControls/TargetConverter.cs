// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System.ComponentModel;

/// <devdoc>
/// </devdoc>
public class TargetConverter : StringConverter
{

    private static readonly string[] targetValues = {
        "_blank",
        "_parent",
        "_search",
        "_self",
        "_top"
    };

    private StandardValuesCollection values;

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        if (values == null)
        {
            values = new StandardValuesCollection(targetValues);
        }
        return values;
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return false;
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

}
