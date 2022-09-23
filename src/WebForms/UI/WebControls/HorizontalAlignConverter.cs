// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Globalization;

// 

#nullable disable

namespace System.Web.UI.WebControls;
internal sealed class HorizontalAlignConverter : EnumConverter
{

    static readonly string[] stringValues = new string[(int)HorizontalAlign.Justify + 1];

    static HorizontalAlignConverter()
    {
        stringValues[(int)HorizontalAlign.NotSet] = "NotSet";
        stringValues[(int)HorizontalAlign.Left] = "Left";
        stringValues[(int)HorizontalAlign.Center] = "Center";
        stringValues[(int)HorizontalAlign.Right] = "Right";
        stringValues[(int)HorizontalAlign.Justify] = "Justify";
    }

    // this constructor needs to be public despite the fact that it's in an internal
    // class so it can be created by Activator.CreateInstance.
    public HorizontalAlignConverter() : base(typeof(HorizontalAlign)) { }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string)
        {
            string textValue = ((string)value).Trim();
            if (textValue.Length == 0)
            {
                return HorizontalAlign.NotSet;
            }

            switch (textValue)
            {
                case "NotSet":
                    return HorizontalAlign.NotSet;
                case "Left":
                    return HorizontalAlign.Left;
                case "Center":
                    return HorizontalAlign.Center;
                case "Right":
                    return HorizontalAlign.Right;
                case "Justify":
                    return HorizontalAlign.Justify;
            }
        }
        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) ? true : base.CanConvertTo(context, sourceType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        return destinationType == typeof(string) && ((int)value <= (int)HorizontalAlign.Justify)
            ? stringValues[(int)value]
            : base.ConvertTo(context, culture, value, destinationType);
    }
}

