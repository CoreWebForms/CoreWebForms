// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace System.Web;

#pragma warning disable SYSWEB1001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

internal static class RequestEndExtensions
{
    /// <summary>
    /// By design, the System.Web adapters try not to throw when <see cref="HttpResponse.End"/> but rather tracks it in other ways.
    /// However, WebForms was built with that expectation and some common scenarios (such as postback calls) break without this.
    /// </summary>
    public static IApplicationBuilder EnsureRequestEndThrows(this IApplicationBuilder app)
        => app.Use(async (ctx, next) =>
        {
            var feature = new RequestThrowingEndFeature(ctx);

            ctx.Features.Set<IHttpResponseEndFeature>(feature);

            try
            {
                await using (feature)
                {
                    await next(ctx).ConfigureAwait(false);
                }
            }
            catch (RequestEndException)
            {
            }
        });

    private sealed class RequestThrowingEndFeature : IHttpResponseEndFeature, IAsyncDisposable
    {
        private readonly HttpContextCore _context;
        private readonly IHttpResponseEndFeature _original;
        private bool _end;

        public RequestThrowingEndFeature(HttpContextCore context)
        {
            _context = context;
            _original = context.Features.GetRequiredFeature<IHttpResponseEndFeature>();
        }

        public bool IsEnded => _end || _original.IsEnded;

        public Task EndAsync()
        {
            _end = true;
            throw new RequestEndException();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            _context.Features.Set(_original);

            if (_end)
            {
                await _original.EndAsync().ConfigureAwait(false);
            }
        }
    }

    private sealed class RequestEndException : Exception
    {
    }
}
