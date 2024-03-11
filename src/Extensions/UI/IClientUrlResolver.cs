// MIT License.

namespace System.Web.UI
{
    internal interface IClientUrlResolver {
        string AppRelativeTemplateSourceDirectory { get; }
        string ResolveClientUrl(string relativeUrl);
    }
}
