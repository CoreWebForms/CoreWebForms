// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsEndpointExtensions
{
    public static IEndpointConventionBuilder MapWebForms(this IEndpointRouteBuilder endpoints)
    {
        // This ensures they're mapped which always returns the same convention builder
        endpoints.MapHttpHandlers();

        if (endpoints.DataSources.OfType<WebFormsConventionBuilder>().FirstOrDefault() is { } existing)
        {
            return existing;
        }

        const string Prefix = "/__webforms/scripts/system.web";

        var provider = new EmbeddedFileProvider(typeof(Page).Assembly, "System.Web.UI.WebControls.RuntimeScripts");
        var files = new StaticFileOptions
        {
            RequestPath = Prefix,
            FileProvider = provider,
            OnPrepareResponse = SetCacheHeaders,
        };

        var app = endpoints.CreateApplicationBuilder();

        app.Use((context, next) =>
        {
            context.SetEndpoint(null);
            return next(context);
        });

        app.UseStaticFiles(files);

        var pipeline = app.Build();

        var composite = new WebFormsConventionBuilder();

        foreach (var file in provider.GetDirectoryContents(string.Empty))
        {
            if (file is IFileInfo f)
            {
                composite.Add(endpoints.MapGet($"{Prefix}/{f.Name}", pipeline)
                    .WithDisplayName($"WebForms Static Files [{f.Name}]"));
            }
        }

        ((IEndpointConventionBuilder)composite).Add(builder => ((RouteEndpointBuilder)builder).Order = int.MinValue);

        return composite;
    }

    private static void SetCacheHeaders(StaticFileResponseContext context)
    {
        // By setting "Cache-Control: no-cache", we're allowing the browser to store
        // a cached copy of the response, but telling it that it must check with the
        // server for modifications (based on Etag) before using that cached copy.
        // Longer term, we should generate URLs based on content hashes (at least
        // for published apps) so that the browser doesn't need to make any requests
        // for unchanged files.
        var headers = context.Context.Response.GetTypedHeaders();
        if (headers.CacheControl == null)
        {
            headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
        }
    }

    // This is an empty endpoint so that we can track it in the IEndpointRouteBuilder
    private sealed class WebFormsConventionBuilder : EndpointDataSource, IEndpointConventionBuilder
    {
        private readonly List<IEndpointConventionBuilder> _builders = new();

        public override IReadOnlyList<Endpoint> Endpoints => [];

        public void Add(IEndpointConventionBuilder builder)
        {
            _builders.Add(builder);
        }

        public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

        void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention)
        {
            foreach (var builder in _builders)
            {
                builder.Add(convention);
            }
        }
    }
}
