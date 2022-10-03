// MIT License.

using System.Collections.Specialized;

namespace System.Web.UI;
/// <devdoc>
/// </devdoc>
public interface IBindableTemplate : ITemplate
{

    /// <devdoc>
    /// Retrives the values of all control properties with two-way bindings.
    /// </devdoc>
    IOrderedDictionary ExtractValues(Control container);
}

