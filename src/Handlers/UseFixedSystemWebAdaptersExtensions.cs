// MIT License.

using System.IO.Pipelines;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

public static class UseFixedSystemWebAdaptersExtensions
{
    public static void UseFixedSystemWebAdapters(this IApplicationBuilder app)
    {
        app.UseSystemWebAdapters();
        app.Use((ctx, next) =>
        {
            ctx.Features.Set<IRequestBodyPipeFeature>(new NewPipe(ctx.Features.GetRequired<IHttpRequestFeature>()));
            return next(ctx);
        });
    }

    private class NewPipe : IRequestBodyPipeFeature
    {
        private readonly IHttpRequestFeature _feature;
        private PipeReader? _pipeReader;

        public NewPipe(IHttpRequestFeature feature)
        {
            _feature = feature;
        }

        PipeReader IRequestBodyPipeFeature.Reader => _pipeReader ??= PipeReader.Create(_feature.Body, new StreamPipeReaderOptions(leaveOpen: true));
    }
}
