using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpContextServerVariableExtensions = System.Web.HttpContextServerVariableExtensions;

namespace WebForms.Tests;

[TestClass]
public class HttpContextVariableTests
{
    [TestMethod]
    [DataRow("HTTP_REFERER", "http://example.com", "Referer")]
    [DataRow("REMOTE_ADDR", "127.0.0.1", "RemoteIpAddress")]
    [DataRow("SERVER_NAME", "example.com", "Host")]
    [DataRow("SERVER_PORT", "80", "ServerPort")]
    [DataRow("REQUEST_METHOD", "POST", "Method")]
    [DataRow("QUERY_STRING", "?key=value", "QueryString")]
    [DataRow("REMOTE_PORT", "12345", "RemotePort")]
    [DataRow("HTTP_USER_AGENT", "Mozilla/5.0", "User-Agent")]
    [DataRow("PATH_INFO", "/path/info", "Path")]
    public void ServerVariable_ReturnsExpectedValue(string systemWebVariable, string value,
        string aspNetCoreVariable)
    {
        DefaultHttpContext context = new DefaultHttpContext();

        switch (aspNetCoreVariable)
        {
            case "Referer":
                context.Request.Headers.Referer = value;
                break;
            case "RemoteIpAddress":
                context.Connection.RemoteIpAddress = IPAddress.Parse(value);
                break;
            case "Host":
                context.Request.Host = new HostString(value);
                break;
            case "Method":
                context.Request.Method = value;
                break;
            case "QueryString":
                context.Request.QueryString = new QueryString(value);
                break;
            case "RemotePort":
                bool isValidPort = int.TryParse(value, out int remotePort);
                if (!isValidPort)
                {
                    throw new FormatException("Invalid port number");
                }

                context.Connection.RemotePort = remotePort;
                break;
            case "User-Agent":
                context.Request.Headers["User-Agent"] = value;
                break;
            case "Path":
                context.Request.Path = value;
                break;
            case "ServerPort":
                bool isValidServerPort = int.TryParse(value, out int serverPort);
                if (!isValidServerPort)
                {
                    throw new FormatException("Invalid port number");
                }

                context.Request.Host = new HostString(context.Request.Host.Host, serverPort);
                break;
            default:
                throw new Exception("Unknown variable");
        }

        string? result = HttpContextServerVariableExtensions.ServerVariable(context, systemWebVariable, string.Empty);

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [DataRow("NON_EXISTENT_KEY", "sample", "NonExistentKey", "default")]
    public void ServerVariable_ReturnsDefaultValueForNonExistentKey(string systemWebVariable, string value,
        string aspNetCoreVariable, string expectedValue)
    {
        DefaultHttpContext context = new();

        context.Request.Headers[aspNetCoreVariable] = value;
        string result = HttpContextServerVariableExtensions.ServerVariable(context, systemWebVariable, expectedValue);

        Assert.AreEqual(expectedValue, result);
    }
}
