// MIT License.

using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

internal sealed class HandlerMetadataProvider
{
    private readonly bool _needsResponseBuffering;

    public HandlerMetadataProvider(IOptions<HttpApplicationOptions> options)
    {
        var custom = options.Value.ApplicationType != typeof(HttpApplication);
        var hasModules = options.Value.Modules.Count > 0;

        _needsResponseBuffering = !custom || hasModules;
    }

    private static class Metadata
    {
        public static object BufferResponse = new BufferResponseStreamAttribute();
        public static object BufferRequest = new PreBufferRequestStreamAttribute();
        public static object Principal = new SetThreadCurrentPrincipalAttribute();

        public static object ReadOnlySession = new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly };
        public static object RequiredSession = new SessionAttribute { SessionBehavior = SessionStateBehavior.Required };
    }

    public void Add(EndpointBuilder builder, IHttpHandlerMetadata metadata)
    {
        if (metadata.Behavior is SessionStateBehavior.ReadOnly)
        {
            builder.Metadata.Add(Metadata.ReadOnlySession);
        }
        else if (metadata.Behavior is SessionStateBehavior.Required)
        {
            builder.Metadata.Add(Metadata.RequiredSession);
        };

        builder.Metadata.Add(metadata);

        builder.Metadata.Add(Metadata.Principal);
        builder.Metadata.Add(Metadata.BufferRequest);

        // A bug in the adapters fails to enable buffering if it is already buffered
        if (_needsResponseBuffering)
        {
            builder.Metadata.Add(Metadata.BufferResponse);
        }
    }
}
