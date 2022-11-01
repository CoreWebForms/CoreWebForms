// MIT License.

namespace System.Web.UI;

/// <devdoc>
/// PropertyEntry for ITemplate properties
/// </devdoc>
public class TemplatePropertyEntry : BuilderPropertyEntry
{
    private readonly bool _bindableTemplate;

    internal TemplatePropertyEntry()
    {
    }

    internal TemplatePropertyEntry(bool bindableTemplate)
    {
        _bindableTemplate = bindableTemplate;
    }

    internal bool IsMultiple
    {
        get
        {
            return Util.IsMultiInstanceTemplateProperty(PropertyInfo);
        }
    }

    public bool BindableTemplate
    {
        get
        {
            return _bindableTemplate;
        }
    }
}

