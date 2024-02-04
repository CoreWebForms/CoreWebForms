// MIT License.

using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Builder;
using WebForms.Compiler.Dynamic;

namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IWebFormsBuilder AddWebFormsExtensions(this IWebFormsBuilder builder)
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
