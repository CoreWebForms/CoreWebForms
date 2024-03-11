// MIT License.

using System.Globalization;
using System.Web.Util;

namespace System.Web.UI;
/// <devdoc>
/// </devdoc>
public sealed class ExpressionBinding
{

    private readonly string _propertyName;
    private readonly Type _propertyType;
    private string _expression;
    private string _expressionPrefix;
    private readonly bool _generated;
    private readonly object _parsedExpressionData;

    public ExpressionBinding(string propertyName, Type propertyType, string expressionPrefix, string expression) : this(propertyName, propertyType, expressionPrefix, expression, false, null)
    {
    }

    /// <devdoc>
    /// </devdoc>
    internal ExpressionBinding(string propertyName, Type propertyType, string expressionPrefix, string expression, bool generated, object parsedExpressionData)
    {
        _propertyName = propertyName;
        _propertyType = propertyType;
        _expression = expression;
        _expressionPrefix = expressionPrefix;
        _generated = generated;
        _parsedExpressionData = parsedExpressionData;
    }

    /// <devdoc>
    /// </devdoc>
    public string Expression
    {
        get
        {
            return _expression;
        }
        set
        {
            _expression = value;
        }
    }

    /// <devdoc>
    /// </devdoc>G
    public string ExpressionPrefix
    {
        get
        {
            return _expressionPrefix;
        }
        set
        {
            _expressionPrefix = value;
        }
    }

    public bool Generated
    {
        get
        {
            return _generated;
        }
    }

    public object ParsedExpressionData
    {
        get
        {
            return _parsedExpressionData;
        }
    }

    /// <devdoc>
    /// </devdoc>
    public string PropertyName
    {
        get
        {
            return _propertyName;
        }
    }

    /// <devdoc>
    /// </devdoc>
    public Type PropertyType
    {
        get
        {
            return _propertyType;
        }
    }

    /// <devdoc>
    /// </devdoc>
    public override int GetHashCode()
    {
        return _propertyName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
    }

    /// <devdoc>
    /// </devdoc>
    public override bool Equals(object obj)
    {
        if ((obj != null) && (obj is ExpressionBinding))
        {
            ExpressionBinding binding = (ExpressionBinding)obj;

            return StringUtil.EqualsIgnoreCase(_propertyName, binding.PropertyName);
        }
        return false;
    }
}

