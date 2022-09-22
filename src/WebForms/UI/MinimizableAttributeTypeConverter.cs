// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Globalization;

#nullable disable

namespace System.Web.UI;

/// <summary>
/// Summary description for MinimizableAttributeTypeConverter.
/// </summary>
internal class MinimizableAttributeTypeConverter : BooleanConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        string strValue = value as string;
        if (strValue != null)
        {
            if ((strValue.Length > 0) && !String.Equals(strValue, "false", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
