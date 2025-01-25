// MIT License.
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Primitives;
using AspNetCoreHttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace System.Web;

public static class HttpContextServerVariableExtensions
{
    /// <summary>
    /// Extracts a server variable with <paramref name="key"/>.
    /// </summary>
    /// <param name="context">The context to extract the server variable from.</param>
    /// <param name="key">The key of the server variable.</param>
    /// <param name="defaultValue">When no server variable was found (or empty), this value is returned instead.</param>
    /// <returns>Found server variable (non-empty), otherwise <paramref name="defaultValue"/>.</returns>
    public static string ServerVariable(this HttpContext systemWebContext, string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(systemWebContext);

        AspNetCoreHttpContext context = systemWebContext.AsAspNetCore();

        string value = string.Empty;

        switch (key.ToUpperInvariant())
        {
            case "HTTP_REFERER":
                value = context.Request.Headers.Referer.ToString();
                break;
            case "REMOTE_ADDR":
                value = context.Connection.RemoteIpAddress?.ToString();
                break;
            case "SERVER_NAME":
                value = context.Request.Host.Host;
                break;
            case "SERVER_PORT":
                value = context.Request.Host.Port.ToString();
                break;
            case "REQUEST_METHOD":
                value = context.Request.Method;
                break;
            case "QUERY_STRING":
                value = context.Request.QueryString.Value;
                break;
            case "REMOTE_PORT":
                value = context.Connection.RemotePort.ToString();
                break;
            case "HTTP_USER_AGENT":
                value = context.Request.Headers.UserAgent.ToString();
                break;
            case "PATH_INFO":
                value = context.Request.Path.ToString();
                break;
            default:
                if (context.Request.Headers.TryGetValue(key, out StringValues headerValue))
                {
                    value = headerValue.ToString();
                }

                break;
        }

        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }
}
