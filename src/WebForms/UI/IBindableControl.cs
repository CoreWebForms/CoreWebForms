// MIT License.

using System.Collections.Specialized;

namespace System.Web.UI;
public interface IBindableControl
{

    /// <devdoc>
    /// Retrives the values of all control properties with two-way bindings.
    /// </devdoc>
    void ExtractValues(IOrderedDictionary dictionary);

}

