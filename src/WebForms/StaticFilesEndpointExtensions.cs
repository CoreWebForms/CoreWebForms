// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Builder;

internal static class StaticFilesEndpointExtensions
{
    public static IEndpointConventionBuilder MapStaticFiles(this IEndpointRouteBuilder endpoints, IFileProvider fileProvider, string Prefix, Func<string, string> displayNameFunc)
    {
        var files = new StaticFileOptions
        {
            RequestPath = Prefix,
            FileProvider = fileProvider,
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

        var composite = new CompositeEndpointBuilder();

        foreach (var file in fileProvider.GetDirectoryContents(string.Empty))
        {
            if (file is IFileInfo f)
            {
                composite.Add(endpoints.MapGet($"{Prefix}/{f.Name}", pipeline)
                    .WithDisplayName(displayNameFunc(f.Name)));
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
    private sealed class CompositeEndpointBuilder : EndpointDataSource, IEndpointConventionBuilder
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
