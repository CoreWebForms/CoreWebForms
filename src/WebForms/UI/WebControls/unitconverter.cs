// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

#nullable disable

namespace System.Web.UI.WebControls;
public class UnitConverter : TypeConverter
{

    /// <internalonly/>
    /// <devdoc>
    ///   Returns a value indicating whether the unit converter can 
    ///   convert from the specified source type.
    /// </devdoc>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
    }

    /// <internalonly/>
    /// <devdoc>
    ///   Returns a value indicating whether the converter can
    ///   convert to the specified destination type.
    /// </devdoc>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        return (destinationType == typeof(string)) ||
            (destinationType == typeof(InstanceDescriptor))
            ? true
            : base.CanConvertTo(context, destinationType);
    }

    /// <internalonly/>
    /// <devdoc>
    ///   Performs type conversion from the given value into a Unit.
    /// </devdoc>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value == null)
        {
            return null;
        }

        string stringValue = value as string;
        if (stringValue != null)
        {
            string textValue = stringValue.Trim();
            if (textValue.Length == 0)
            {
                return Unit.Empty;
            }
            return culture != null ? Unit.Parse(textValue, culture) : (object)Unit.Parse(textValue, CultureInfo.CurrentCulture);
        }
        else
        {
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <internalonly/>
    /// <devdoc>
    ///   Performs type conversion to the specified destination type
    /// </devdoc>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return (value == null) || ((Unit)value).IsEmpty ? string.Empty : (object)((Unit)value).ToString(culture);
        }
        else if ((destinationType == typeof(InstanceDescriptor)) && (value != null))
        {
            Unit u = (Unit)value;
            MemberInfo member = null;
            object[] args = null;

            if (u.IsEmpty)
            {
                member = typeof(Unit).GetField("Empty");
            }
            else
            {
                member = typeof(Unit).GetConstructor(new Type[] { typeof(double), typeof(UnitType) });
                args = new object[] { u.Value, u.Type };
            }

            Debug.Assert(member != null, "Looks like we're missing Unit.Empty or Unit::ctor(double, UnitType)");
            return member != null ? new InstanceDescriptor(member, args) : (object)null;
        }
        else
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

