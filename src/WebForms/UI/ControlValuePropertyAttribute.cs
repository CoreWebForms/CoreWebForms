// MIT License.

#nullable disable

using System.ComponentModel;

namespace System.Web.UI;

/// <devdoc>
/// Specifies the default value property for a control.
/// </devdoc>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ControlValuePropertyAttribute : Attribute
{
    /// <devdoc>
    /// Initializes a new instance of the <see cref='System.Web.UI.ControlValuePropertyAttribute'/> class.
    /// </devdoc>
    public ControlValuePropertyAttribute(string name)
    {
        Name = name;
    }

    /// <devdoc>
    /// Initializes a new instance of the class, using the specified value as the default value.
    /// </devdoc>
    public ControlValuePropertyAttribute(string name, object defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    /// <devdoc>
    /// Initializes a new instance of the class, converting the specified value to the
    /// specified type.
    /// </devdoc>
    public ControlValuePropertyAttribute(string name, Type type, string defaultValue)
    {
        Name = name;
        // The try/catch here is because attributes should never throw exceptions.  We would fail to
        // load an otherwise normal class.
        try
        {
            DefaultValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(defaultValue);
        }
        catch
        {
            System.Diagnostics.Debug.Fail("ControlValuePropertyAttribute: Default value of type " + type.FullName + " threw converting from the string '" + defaultValue + "'.");
        }
    }

    /// <devdoc>
    /// Gets the name of the default value property for the control this attribute is bound to.
    /// </devdoc>
    public string Name { get; }

    /// <devdoc>
    /// Gets the value of the default value property for the control this attribute is bound to.
    /// </devdoc>
    public object DefaultValue { get; }

    public override bool Equals(object obj)
    {
        if (obj is ControlValuePropertyAttribute other)
        {
            if (string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                if (DefaultValue != null)
                {
                    return DefaultValue.Equals(other.DefaultValue);
                }
                else
                {
                    return other.DefaultValue == null;
                }
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, DefaultValue);
    }
}
