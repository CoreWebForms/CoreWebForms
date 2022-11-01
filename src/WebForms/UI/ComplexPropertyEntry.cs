// MIT License.

namespace System.Web.UI;
/// <devdoc>
/// PropertyEntry for read/write and readonly complex properties
/// </devdoc>
public class ComplexPropertyEntry : BuilderPropertyEntry
{
    private bool _readOnly;
    private readonly bool _isCollectionItem;

    internal ComplexPropertyEntry()
    {
    }

    internal ComplexPropertyEntry(bool isCollectionItem)
    {
        _isCollectionItem = isCollectionItem;
    }

    /// <devdoc>
    /// Indicates whether the property is a collection property.
    /// </devdoc>
    public bool IsCollectionItem
    {
        get
        {
            return _isCollectionItem;
        }
    }

    /// <devdoc>
    /// </devdoc>
    public bool ReadOnly
    {
        get
        {
            return _readOnly;
        }
        set
        {
            _readOnly = value;
        }
    }
}

