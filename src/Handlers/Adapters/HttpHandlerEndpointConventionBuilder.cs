// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface IHttpHandlerManager
{
    IChangeToken GetChangeToken();

    IEnumerable<EndpointBuilder> GetBuilders();
}

internal sealed class HttpHandlerEndpointConventionBuilder : EndpointDataSource, IEndpointConventionBuilder
{
    private readonly IHttpHandlerManager _handlers;
    private List<Action<EndpointBuilder>> _conventions = [];

    internal HttpHandlerEndpointConventionBuilder(IHttpHandlerManager handlers)
    {
        _handlers = handlers;
    }

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            var endpoints = new List<Endpoint>();

            foreach (var builder in _handlers.GetBuilders())
            {
                foreach (var convention in _conventions)
                {
                    convention(builder);
                }

#if NET7_0_OR_GREATER
                if (builder.FilterFactories.Count > 0)
                {
                    throw new NotSupportedException("Filter factories are not supported for handlers");
                }
#endif
                endpoints.Add(builder.Build());
            }

            return endpoints;
        }
    }

    public void Add(Action<EndpointBuilder> convention)
        => (_conventions ??= []).Add(convention);

    public override IChangeToken GetChangeToken() => _handlers.GetChangeToken();
}
