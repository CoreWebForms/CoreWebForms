// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebForms.Features;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilationStartup : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => builder =>
    {
        builder.Use((ctx, next) =>
        {
            ctx.Features.Set<IWebFormsCompilationFeature>(ctx.RequestServices.GetRequiredService<DynamicSystemWebCompilation>().Current);

            return next(ctx);
        });

        next(builder);
    };
}
