// MIT License.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface IHttpHandlerBuilder
{
    void Add(string path, IHttpHandler handler);
}

