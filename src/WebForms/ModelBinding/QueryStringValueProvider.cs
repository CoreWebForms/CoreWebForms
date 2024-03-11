//MIT license

using System.Globalization;

namespace System.Web.ModelBinding; 
public sealed class QueryStringValueProvider : NameValueCollectionValueProvider {

    // QueryString should use the invariant culture since it's part of the URL, and the URL should be
    // interpreted in a uniform fashion regardless of the origin of a particular request.
    public QueryStringValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
        : this(modelBindingExecutionContext, null) {
        //TODO check https://github.com/twsouthwick/systemweb-adapters-ui/issues/27
        //modelBindingExecutionContext.HttpContext.Request.Unvalidated) {
    }

    // For unit testing
    internal QueryStringValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, UnvalidatedRequestValuesBase unvalidatedValues)
    //TODO incorporate Unvalidated
    //: base(modelBindingExecutionContext.HttpContext.Request.QueryString, unvalidatedValues.QueryString, CultureInfo.InvariantCulture)
        : base(modelBindingExecutionContext.HttpContext.Request.QueryString, null, CultureInfo.InvariantCulture) {
        }

}
