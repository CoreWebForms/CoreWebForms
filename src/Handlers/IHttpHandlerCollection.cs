// MIT License.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

/// <summary>
/// A collection to manage the known <see cref="System.Web.IHttpHandler"/> paths.
/// </summary>
public interface IHttpHandlerCollection
{
    /// <summary>
    /// Gets the known <see cref="IHttpHandlerMetadata"/> instances to generate endpoints.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IHttpHandlerMetadata> GetHandlerMetadata();

    /// <summary>
    /// Gets the named routes from the collection that can be used to map routes to known paths.
    /// </summary>
    IEnumerable<NamedHttpHandlerRoute> NamedRoutes { get; }

    /// <summary>
    /// A change token for when the collection changes.
    /// </summary>
    /// <returns></returns>
    IChangeToken GetChangeToken();
}
