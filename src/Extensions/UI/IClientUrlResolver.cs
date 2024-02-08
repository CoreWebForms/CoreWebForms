// MIT License.

namespace System.Web.UI {
    using System;

    internal interface IClientUrlResolver {
        string AppRelativeTemplateSourceDirectory { get; }
        string ResolveClientUrl(string relativeUrl);
    }
}
