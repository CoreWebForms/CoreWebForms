// MIT License.

using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

namespace Microsoft.AspNetCore.Builder;

internal static class HandlerMetadataProvider
{
    private static class Metadata
    {
        public static object BufferResponse = new BufferResponseStreamAttribute();
        public static object BufferRequest = new PreBufferRequestStreamAttribute();
        public static object Principal = new SetThreadCurrentPrincipalAttribute();

        public static object ReadOnlySession = new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly };
        public static object RequiredSession = new SessionAttribute { SessionBehavior = SessionStateBehavior.Required };
    }

    public static void AddHandler(this EndpointBuilder builder, IHttpHandlerMetadata metadata)
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
        builder.Metadata.Add(Metadata.BufferResponse);
    }
}
