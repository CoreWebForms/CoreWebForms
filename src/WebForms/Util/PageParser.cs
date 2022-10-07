// MIT License.

namespace System.Web.UI;

internal class PageParser : TemplateControlParser
{
    internal static string defaultDirectiveName;

    internal override string UnknownOutputCacheAttributeError => throw new NotImplementedException();

    internal override Type DefaultBaseType => throw new NotImplementedException();

    internal override string DefaultDirectiveName => throw new NotImplementedException();
}
