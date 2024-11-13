// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection;
using WebForms.Features;

namespace WebForms.Compiler.Dynamic;

internal class SetCompilationFeatureMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var compiler = context.RequestServices.GetRequiredService<IWebFormsCompiler>();

        if (compiler.CompilationFeature != null && context.Features.Get<IWebFormsCompilationFeature>() is null)
        {
            context.Features.Set(compiler.CompilationFeature);
        }

        return next(context);
    }
}
