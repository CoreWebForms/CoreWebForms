// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using WebForms.Features;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilationStartup : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> _next) => builder =>
    {
        builder.Use((ctx, next) =>
        {
            ctx.Features.Set<IEndpointFeature>(new CompilationEndpoint(ctx));

            return next(ctx);
        });

        _next(builder);
    };

    private sealed class CompilationEndpoint(HttpContext context) : IEndpointFeature
    {
        // the http handler feature should add one and will always be there
        private readonly IEndpointFeature _endpoint = context.Features.GetRequiredFeature<IEndpointFeature>();

        public Endpoint? Endpoint
        {
            get => _endpoint.Endpoint;
            set
            {
                if (value?.Metadata.GetMetadata<IWebFormsCompilationFeature>() is { } f)
                {
                    context.Features.Set<IWebFormsCompilationFeature>(f);
                }

                _endpoint.Endpoint = value;
            }
        }
    }
}
