// MIT License.

using System.Web.UI.WebControls;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using WebForms.Compiler.Dynamic;

namespace System.Web.UI;

public static class Extensions
{
    public static ISystemWebAdapterBuilder AddWebFormsExtensions(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddOptions<PageCompilationOptions>()
          .Configure(options =>
          {
              options.AddTypeNamespace<ScriptManager>("asp");
              options.AddTypeNamespace<ListView>("asp");
          });
        return builder;
    }
}
