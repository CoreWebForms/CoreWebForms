// MIT License.

using System.Web;
using System.Web.SessionState;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

public static class HandlerMetadata
{
    public static IHttpHandlerMetadata Create<T>(string path) where T : IHttpHandler => new TypeHandlerMetadata(path, typeof(T));

    public static IHttpHandlerMetadata Create(string path, Type type) => new TypeHandlerMetadata(path, type);

    public static IHttpHandlerMetadata Create(string path, IHttpHandler handler) => new SingletonHandlerMetadata(path, handler);

    private sealed class SingletonHandlerMetadata(string path, IHttpHandler handler) : IHttpHandlerMetadata
    {
        public string Route => path;

        public SessionStateBehavior Behavior => handler switch
        {
            IReadOnlySessionState => SessionStateBehavior.ReadOnly,
            IRequiresSessionState => SessionStateBehavior.Required,
            _ => SessionStateBehavior.Default,
        };

        public ValueTask<IHttpHandler> Create(HttpContextCore context) => ValueTask.FromResult(handler);
    }

    private sealed class TypeHandlerMetadata(string path, Type type) : IHttpHandlerMetadata
    {
        private IHttpHandler? _handler;
        private readonly ObjectFactory _factory = ActivatorUtilities.CreateFactory(type, []);

        string IHttpHandlerMetadata.Route => path;

        public SessionStateBehavior Behavior
        {
            get
            {
                if (type.IsAssignableTo(typeof(IReadOnlySessionState)))
                {
                    return SessionStateBehavior.ReadOnly;
                }

                if (type.IsAssignableTo(typeof(IRequiresSessionState)))
                {
                    return SessionStateBehavior.Required;
                }

                return SessionStateBehavior.Default;
            }
        }

        public ValueTask<IHttpHandler> Create(HttpContextCore context)
        {
            if (_handler is { } h)
            {
                return ValueTask.FromResult(h);
            }

            var newHandler = (IHttpHandler)_factory(context.RequestServices, null);

            if (newHandler.IsReusable)
            {
                Interlocked.Exchange(ref _handler, newHandler);
            }

            return ValueTask.FromResult(newHandler);
        }
    }
}

