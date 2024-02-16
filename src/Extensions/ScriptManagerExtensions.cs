// MIT License.

using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http.HttpResults;
#endif
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: TagPrefix("System.Web.UI", "asp")]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]

namespace Microsoft.Extensions.DependencyInjection;

public static class ScriptManagerExtensions
{
    public static IWebFormsBuilder AddScriptManager(this IWebFormsBuilder builder)
    {
        builder.Services.TryAddSingleton<ScriptResourceHandler>();
        builder.Services.AddSingleton<IScriptResourceHandler>(sp => sp.GetRequiredService<ScriptResourceHandler>());

        return builder;
    }

    public static void MapScriptManager(this IEndpointRouteBuilder endpoints)
    {
        //var provider = new EmbeddedFileProvider(typeof(ScriptManager).Assembly, "System.Web.Script.js.dist");
        //var path = "/__webforms/scripts";

        //endpoints.MapStaticFiles(provider, path, name => $"AJAX [{name}]");

#if NET8_0_OR_GREATER
        endpoints.Map($"{endpoints.ServiceProvider.GetRequiredService<ScriptResourceHandler>().Prefix}", Results<FileStreamHttpResult, NotFound> (HttpRequest request, [FromServices] ScriptResourceHandler handler) =>
        {
            if (request.Query["s"] is [{ } file] && handler.Resolve(file) is { } resource)
            {
                return TypedResults.Stream(resource);
            }
            else
            {
                return TypedResults.NotFound();
            }
        });
#endif
    }

    private sealed class ScriptResourceHandler : IScriptResourceHandler
    {
        private readonly IDataProtector _protector;
        private readonly AssemblyLoadContext _context = AssemblyLoadContext.Default;

        public ScriptResourceHandler(IDataProtectionProvider protector)
        {
            _protector = protector.CreateProtector("ScriptResource");
        }

        public string Prefix { get; } = "__webforms/scripts";

        public Stream Resolve(string file)
        {
            try
            {
                var decoded = AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(file);
                var bytes = _protector.Unprotect(decoded);

                var stream = new MemoryStream(bytes);
                var reader = new BinaryReader(stream);

                var name = new AssemblyName(reader.ReadString());
                var resourceName = reader.ReadString();
                var cultureName = reader.ReadString();
                var zip = reader.ReadBoolean();

                if (_context.LoadFromAssemblyName(name) is { } assembly && assembly.GetManifestResourceStream(resourceName) is { } resource)
                {
                    return resource;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public string GetScriptResourceUrl(Assembly assembly, string resourceName, CultureInfo culture, bool zip)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(assembly.FullName);
            writer.Write(resourceName);
            writer.Write(culture.Name);
            writer.Write(zip);

            var @protected = _protector.Protect(ms.ToArray());
            var encoded = AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(@protected);

            return Prefix + "?s=" + encoded;
        }
    }
}
