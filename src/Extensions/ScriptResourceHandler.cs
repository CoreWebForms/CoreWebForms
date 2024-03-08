// MIT License.

using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Web.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace WebForms.Extensions;

internal sealed class ScriptResourceHandler : IScriptResourceHandler
{
    private readonly ILogger<ScriptResourceHandler> _logger;
    private readonly IDataProtector _protector;
    private readonly AssemblyLoadContext _context = AssemblyLoadContext.Default;

    public ScriptResourceHandler(IDataProtectionProvider protector, ILogger<ScriptResourceHandler> logger)
    {
        _logger = logger;
        _protector = protector.CreateProtector("ScriptResource");
    }

    public string Prefix { get; } = "__webforms/_scripts";

    public Stream Resolve(string encodedFile)
    {
        try
        {
            var decoded = WebEncoders.Base64UrlDecode(encodedFile);
            var bytes = _protector.Unprotect(decoded);

            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);

            var assemblyName = new AssemblyName(reader.ReadString());
            var resourceName = reader.ReadString();
            var cultureName = reader.ReadString();
            var zip = reader.ReadBoolean();

            if (_context.LoadFromAssemblyName(assemblyName) is { } assembly)
            {
                if (assembly.GetManifestResourceStream(resourceName) is { } resource)
                {
                    _logger.LogTrace("Found script for {Assembly}/{ResourceName}/{Culture}/{Zip} at {Encoded}", assemblyName, resourceName, cultureName, zip, encodedFile);
                    return resource;
                }
            }

            _logger.LogTrace("Failed to find script for {Assembly}/{ResourceName}/{Culture}/{Zip} at {Encoded}", assemblyName, resourceName, cultureName, zip, encodedFile);

            return null;
        }
        catch
        {
            _logger.LogWarning("Error trying to decode requested script {Encoded}", encodedFile);
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
        var encoded = WebEncoders.Base64UrlEncode(@protected);

        _logger.LogTrace("Getting script URL for {Assembly}/{ResourceName}/{Culture}/{Zip} at {Encoded}", assembly.FullName, resourceName, culture.Name, zip, encoded);

        return Prefix + "?s=" + encoded;
    }

    public string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager, bool enableCdn)
    {
        return GetScriptResourceUrl(type.Assembly, resourceName, CultureInfo.InvariantCulture, zip: false);
    }
}
